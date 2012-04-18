using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class CompileContext
    {
        public static String Version { get { return "DCPUC 0.1"; } }

        public RootProgramNode rootNode = null;
        public Scope globalScope = new Scope();
        private Irony.Parsing.Parser Parser = new Irony.Parsing.Parser(new DCPUC.Grammar());
        private List<Tuple<string, List<string>>> dataElements = new List<Tuple<string, List<string>>>();
        private int nextLabelID = 0;
        public List<Instruction> instructions = new List<Instruction>();
        private int _barrier = 0;
        public string source = null;
        public CompileOptions options = new CompileOptions();
        public Action<String> onWarning = null;
        
        public String GetLabel()
        {
            return "L" + nextLabelID++;
        }

        public void AddData(string label, List<ushort> data)
        {
            var strings = new List<string>();
            foreach (var item in data) strings.Add(Hex.hex(item));
            dataElements.Add(new Tuple<string, List<string>>(label, strings));
        }

        public void AddData(string label, string data)
        {
            dataElements.Add(new Tuple<string, List<string>>(label, new List<String>(new string[] { data })));
        }

        public void AddData(string label, ushort word)
        {
            dataElements.Add(new Tuple<string, List<string>>(label, new List<string>(new string[] { Hex.hex(word) })));
        }

        public void AddWarning(Irony.Parsing.SourceSpan location, String message)
        {
            if (onWarning != null)
            {
                onWarning("Warning: " + message + "\n" + source.Substring(location.Location.Position, location.Length));
            }
        }
            
        private static String extractLine(String s, int c)
        {
            int lc = 0;
            int p = 0;
            while (p < s.Length && lc < c)
            {
                if (s[p] == '\n') lc++;
                ++p;
            }

            int ls = p;
            while (p < s.Length && s[p] != '\n') ++p;

            return s.Substring(ls, p - ls);
        }

        public void Initialize(CompileOptions options)
        {
            this.options = options;
        }

        public bool Parse(String code, Action<string> onError)
        {
            source = code;
            globalScope = new Scope();
            dataElements.Clear();
            nextLabelID = 0;

            var program = Parser.Parse(code);
            if (onError == null) onError = (a) => { };
            if (program.HasErrors())
            {
                foreach (var msg in program.ParserMessages)
                {
                    onError(msg.Level + ": " + msg.Message + " [line:" + msg.Location.Line + " column:" + msg.Location.Column + "]\r\n");
                    onError(extractLine(code, msg.Location.Line) + "\r\n");
                    onError(new String(' ', msg.Location.Column) + "^\r\n");
                }
                return false;
            }

            var root = program.Root.AstNode as DCPUC.CompilableNode;
            rootNode = new RootProgramNode();
            rootNode.ChildNodes.Add(root);
            return true;
        }

        public void GatherSymbols(Action<string> onError)
        {
            var end_of_program = new Variable();
            end_of_program.location = Register.STATIC;
            end_of_program.name = "__endofprogram";
            end_of_program.staticLabel = "ENDOFPROGRAM";
            end_of_program.emitBrackets = false;
            globalScope.variables.Add(end_of_program);

            try
            {
                rootNode.GatherSymbols(this, globalScope);
            }
            catch (CompileError e)
            {
                onError(e.Message);
            }
        }

        public void FoldConstants()
        {
            rootNode.FoldConstants(this);
            rootNode.AssignRegisters(new RegisterBank(), Register.DISCARD);
        }

        public void Emit(Action<string> onError)
        {
            

            try
            {
                rootNode.CompileFunction(this);
                foreach (var dataItem in dataElements)
                {
                    var datString = "";
                    foreach (var item in dataItem.Item2)
                    {
                        datString += item;
                        datString += ", ";
                    }
                    Add(":" + dataItem.Item1, "DAT", datString.Substring(0, datString.Length - 2));
                }
                Add(":ENDOFPROGRAM", "", "");
            }
            catch (DCPUC.CompileError c)
            {
                onError(c.Message);
                return;
            }

        }

        public void AddSource(Irony.Parsing.SourceSpan span)
        {
            var instruction = new Instruction { ins = ";" + source.Substring(span.Location.Position, span.Length) };
            instructions.Add(instruction);
        }

        public void Add(string ins, string a, string b, string comment = null)
        {
            var instruction = new Instruction();
            instruction.ins = ins;
            instruction.a = a;
            instruction.b = b;
            instruction.comment = comment;

            //instructions.Add(instruction);
            //return;
            /*
            if (options.p && instructions.Count > _barrier)
            {
                bool ignore = false;
                var lastIns = instructions[instructions.Count - 1];

                //SET A, POP
                //SET PUSH, A
                if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == "POP" && instruction.a == "PUSH" && instruction.b == lastIns.a)
                {
                    instructions.RemoveAt(instructions.Count - 1);
                    ignore = true;
                }
                //SET A, !POP
                //SET !PUSH, A
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.a == instruction.b && lastIns.b != "POP" && instruction.a != "PUSH")
                {
                    if (lastIns.b == instruction.a) instructions.RemoveAt(instructions.Count - 1);
                    else lastIns.a = instruction.a;
                    ignore = true;
                }
                //SET PUSH, A
                //SET A, POP
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == instruction.a && lastIns.a == "PUSH" && instruction.b == "pop")
                {
                    instructions.RemoveAt(instructions.Count - 1);
                    ignore = true;
                }
                //SET PUSH, A
                //SET A, PEEK
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == instruction.a && lastIns.a == "PUSH" && instruction.b == "PEEK")
                {
                    ignore = true;
                }
                //SET A, ?             -> IFN|IFE|IFG ?, A
                //IFN|IFE|IFG ?, A
                else if (lastIns.ins == "SET" && (instruction.ins == "IFN" || instruction.ins == "IFE" || instruction.ins == "IFG")
                    && lastIns.a == instruction.b)
                {
                    lastIns.ins = instruction.ins;
                    lastIns.a = instruction.a;
                    ignore = true;
                }

                if (!ignore) instructions.Add(instruction);
            }
            else*/
                instructions.Add(instruction);

        }

        public void Barrier() { _barrier = instructions.Count; }


    }
}
