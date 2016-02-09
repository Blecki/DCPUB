using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;
using DCPUB.Ast;

namespace DCPUB
{
    public class CompileContext
    {
        public static String Version { get { return "DCPUB 0.3"; } }

        public Ast.RootProgramNode rootNode = null;
        public Model.Scope globalScope = new Model.Scope();
        private Irony.Parsing.Parser Parser = new Irony.Parsing.Parser(new DCPUB.Grammar());
        private List<Tuple<Intermediate.Label, List<Intermediate.Operand>>> dataElements = new List<Tuple<Intermediate.Label, List<Intermediate.Operand>>>();
        public int externalCount = 0;
        private int nextLabelID = 0;
        public string source = null;
        public CompileOptions options = new CompileOptions();
        public Intermediate.Peephole.Peepholes peepholes;
        public int nextVirtualRegister = 0;
        public Action<String> OnError = null;
        public int ErrorCount = 0;

        public int AllocateRegister()
        {
            return nextVirtualRegister++;
        }
        
        public String GetLabel()
        {
            return "L" + nextLabelID++;
        }

        public String GetSourceSpan(Irony.Parsing.SourceSpan span)
        {
            return source.Substring(span.Location.Position, span.Length + 1);
        }

        public void AddData(Intermediate.Label label, List<Intermediate.Operand> FetchTokens)
        {
            dataElements.Add(Tuple.Create(label, FetchTokens));
        }

        public void AddData(Intermediate.Label label, Intermediate.Label data)
        {
            AddData(label, new List<Intermediate.Operand>(new Intermediate.Operand[] { CompilableNode.Label(data) }));
        }

        public void AddData(Intermediate.Label label, ushort word)
        {
            AddData(label, new List<Intermediate.Operand>(new Intermediate.Operand[] { CompilableNode.Constant(word) }));
        }

        public void AddWarning(CompilableNode Node, String Message)
        {
            AddWarning(Node.Span, Message);
        }

        public void AddWarning(Irony.Parsing.SourceSpan location, String message)
        {
            SendLineMessage(location, "WARNING", message);
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
            //Assembly.Peephole.Peepholes.InitializePeepholes();
        }

        public bool Parse(String code, Action<string> onError)
        {
            if (!String.IsNullOrEmpty(options.peephole)) peepholes = new Intermediate.Peephole.Peepholes(options.peephole);

            source = code;
            globalScope = new Model.Scope();
            dataElements.Clear();
            nextLabelID = 0;
            externalCount = 0;

            var program = Parser.Parse(code);
            if (onError == null) onError = (a) => { };
            if (program.HasErrors())
            {
                foreach (var msg in program.ParserMessages)
                {
                    onError("%PARSER " + msg.Level + ": " + msg.Message + " [line:" + msg.Location.Line + " column:" + msg.Location.Column + "]\r\n");
                    onError(extractLine(code, msg.Location.Line) + "\r\n");
                    onError(new String(' ', msg.Location.Column) + "^\r\n");
                }
                return false;
            }

            var root = program.Root.AstNode as CompilableNode;
            rootNode = new Ast.RootProgramNode();
            rootNode.ChildNodes.Add(root);
            return true;
        }

        public Intermediate.IRNode Compile(Action<string> OnError)
        {
            this.OnError = OnError;

            var end_label = new Intermediate.Label("ENDOFPROGRAM");
            globalScope.variables.Add(new Model.Variable { type = Model.VariableType.ConstantLabel, name = "__endofprogram", staticLabel = end_label });
            globalScope.variables.Add(new Model.Variable { name = "true", type = Model.VariableType.Constant, constantValue = 1 });
            globalScope.variables.Add(new Model.Variable { name = "false", type = Model.VariableType.Constant, constantValue = 0 });

            try
            {
                rootNode.GatherSymbols(this, globalScope);
                rootNode.ResolveTypes(this, globalScope);
                //rootNode.FoldConstants(this);
            
                var r = new Intermediate.IRNode();

                if (options.externals)
                {
                    var startOfProgram = new Intermediate.Label("STARTOFPROGRAM");
                    r.AddInstruction(Instructions.SET, CompilableNode.Operand("PC"), CompilableNode.Label(startOfProgram));
                    r.AddChild(new Annotation("External data block. Your assembler or program loader should fill these in."));
                    r.AddLabel(new Intermediate.Label("EXTERNALS"));
                    foreach (var variable in globalScope.variables)
                    {
                        if (variable.type == Model.VariableType.External)
                        {
                            var blankList = new List<Intermediate.Operand>(new Intermediate.Operand[] { CompilableNode.Constant(0) });

                            r.AddChild(new StaticData
                            {
                                label = new Intermediate.Label("__external_" + variable.name),
                                Data = blankList
                            });
                        }
                    }

                    r.AddLabel(startOfProgram);
                }
                
                r.AddChild(rootNode.CompileFunction(this));
                foreach (var dataItem in dataElements)
                    r.AddChild(new StaticData { label = dataItem.Item1, Data = dataItem.Item2 });
                r.AddLabel(end_label);
                
                if (ErrorCount != 0) return null;
                return r;
            }
            catch (DCPUB.InternalError e)
            {
                ErrorCount += 1;
                SendLineMessage((Irony.Parsing.SourceSpan?)null, "INTERNAL ERROR", e.Message);
                return null;
            }

        }

        public void ReportError(Irony.Parsing.SourceSpan? Span, String Message)
        {
            ErrorCount += 1;
            SendLineMessage(Span, "ERROR", Message);
        }

        public void ReportError(CompilableNode Node, String Message)
        {
            ReportError(Node.Span, Message);
        }

        public void SendLineMessage(Irony.Parsing.SourceSpan? Span, String Label, String Message)
        {
            var errorString = "";
            var codeLine = "";
            if (Span.HasValue)
            {
                errorString = "%" + Label + " " + Span.Value.Location.Line + ": " + Message;
                codeLine = extractLine(source, Span.Value.Location.Line);
                errorString += "\r\n" + codeLine + "\r\n" + new String(' ', Span.Value.Location.Column) + "^";
            }
            else
                errorString = "%ERROR: " + Message;
            OnError(errorString);
        }

    }
}
