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
        public string source = null;
        public CompileOptions options = new CompileOptions();
        public Action<String> onWarning = null;
        
        public String GetLabel()
        {
            return "L" + nextLabelID++;
        }

        public String GetSourceSpan(Irony.Parsing.SourceSpan span)
        {
            return source.Substring(span.Location.Position, span.Length + 1);
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

        public static String TypeWarning(string A, string B)
        {
            return "Conversion of " + A + " to " + B + ". Possible loss of data.";
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
            Assembly.Peephole.Peepholes.InitializePeepholes();
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

        public void Compile(Action<string> onError)
        {
            var end_of_program = new Variable();
            end_of_program.location = Register.CONST;
            end_of_program.type = VariableType.ConstantReference;
            end_of_program.name = "__endofprogram";
            end_of_program.staticLabel = "ENDOFPROGRAM";
            globalScope.variables.Add(end_of_program);

            try
            {
                rootNode.GatherSymbols(this, globalScope);
                rootNode.ResolveTypes(this, globalScope);
                rootNode.FoldConstants(this);
                rootNode.AssignRegisters(this, new RegisterBank(), Register.DISCARD);
            }
            catch (CompileError e)
            {
                ReportError(onError, e);
            }
        }

        public Assembly.Node Emit(Action<string> onError)
        {
            try
            {
                var r = rootNode.CompileFunction(this);
                foreach (var dataItem in dataElements)
                    r.AddChild(new Assembly.StaticData { label = dataItem.Item1, data = dataItem.Item2 });
                r.AddLabel("ENDOFPROGRAM");
                r.CollapseTree();
                return r;
            }
            catch (DCPUC.CompileError c)
            {
                ReportError(onError, c);
                return null;
            }

        }

        private void ReportError(Action<string> onError, CompileError c)
        {
            var errorString = "";
            var codeLine = "";
            if (c.span.HasValue)
            {
                errorString = "Error on line " + c.span.Value.Location.Line + ": " + c.Message;
                codeLine = extractLine(source, c.span.Value.Location.Line);
                errorString += "\r\n" + codeLine + "\r\n" + new String(' ', c.span.Value.Location.Column) + "^";
            }
            else
                errorString = c.Message;
            onError(errorString);
            return;
        }

    }
}
