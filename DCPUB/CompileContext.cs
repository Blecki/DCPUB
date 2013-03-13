using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class CompileContext
    {
        public static String Version { get { return "DCPUB 0.3"; } }

        public RootProgramNode rootNode = null;
        public Scope globalScope = new Scope();
        private Irony.Parsing.Parser Parser = new Irony.Parsing.Parser(new DCPUB.Grammar());
        private List<Tuple<Assembly.Label, Object>> dataElements = new List<Tuple<Assembly.Label, Object>>();
        public int externalCount = 0;
        private int nextLabelID = 0;
        public string source = null;
        public CompileOptions options = new CompileOptions();
        public Action<String> onWarning = null;
        public Variable end_of_program = null;
        public Assembly.Peephole.Peepholes peepholes;
        public int nextVirtualRegister = 0;
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

        public void AddData(Assembly.Label label, List<ushort> data)
        {
            //var strings = new List<string>();
            //foreach (var item in data) strings.Add(Hex.hex(item));
            dataElements.Add(new Tuple<Assembly.Label, Object>(label, data));
        }

        public void AddData(Assembly.Label label, Assembly.Label data)
        {
            dataElements.Add(new Tuple<Assembly.Label, Object>(label, data));
        }

        public void AddData(Assembly.Label label, ushort word)
        {
            var data = new List<ushort>();
            data.Add(word);
            dataElements.Add(new Tuple<Assembly.Label, Object>(label, data));
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
            //Assembly.Peephole.Peepholes.InitializePeepholes();
        }

        public bool Parse(String code, Action<string> onError)
        {
            if (!String.IsNullOrEmpty(options.peephole)) peepholes = new Assembly.Peephole.Peepholes(options.peephole);

            source = code;
            globalScope = new Scope();
            dataElements.Clear();
            nextLabelID = 0;
            externalCount = 0;

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

            var root = program.Root.AstNode as DCPUB.CompilableNode;
            rootNode = new RootProgramNode();
            rootNode.ChildNodes.Add(root);
            return true;
        }

        public void Compile(Action<string> onError)
        {
            end_of_program = new Variable();
            end_of_program.type = VariableType.ConstantLabel;
            end_of_program.name = "__endofprogram";
            end_of_program.staticLabel = new Assembly.Label("ENDOFPROGRAM");
            globalScope.variables.Add(end_of_program);

            globalScope.variables.Add(new Variable
            {
                name = "true",
                type = VariableType.Constant,
                constantValue = 1
            });

            globalScope.variables.Add(new Variable
            {
                name = "false",
                type = VariableType.Constant,
                constantValue = 0
            });

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
                var r = new Assembly.Node();

                if (options.externals)
                {
                    r.AddInstruction(Assembly.Instructions.SET,
                        new Assembly.Operand
                        {
                            register = Assembly.OperandRegister.PC
                        }, new Assembly.Operand
                        {
                            semantics = Assembly.OperandSemantics.Label,
                            label = new Assembly.Label("STARTOFPROGRAM")
                        });
                    r.AddChild(new Assembly.Annotation("External data block. Your assembler or program loader should fill these in."));
                    r.AddLabel(new Assembly.Label("EXTERNALS"));
                    foreach (var variable in globalScope.variables)
                    {
                        var blankList = new List<ushort>();
                        blankList.Add(0);
                        if (variable.type == VariableType.External)
                        {
                            r.AddChild(new Assembly.StaticData
                            {
                                label = new Assembly.Label("__external_" + variable.name),
                                data = blankList
                            });
                        }
                    }

                    r.AddLabel(new Assembly.Label("STARTOFPROGRAM"));
                }

                r.AddChild(rootNode.CompileFunction(this));
                foreach (var dataItem in dataElements)
                {
                    if (dataItem.Item2 is Assembly.Label)
                        r.AddChild(new Assembly.StaticLabelData { label = dataItem.Item1, data = dataItem.Item2 as Assembly.Label });
                    else
                        r.AddChild(new Assembly.StaticData { label = dataItem.Item1, data = dataItem.Item2 as List<ushort> });
                }
                r.AddLabel(end_of_program.staticLabel);


                r.CollapseTree(peepholes);
                return r;
            }
            catch (DCPUB.CompileError c)
            {
                ReportError(onError, c);
                return null;
            }

        }

        public Assembly.Node Compile2(Action<string> onError)
        {
            var end_label = new Assembly.Label("ENDOFPROGRAM");
            globalScope.variables.Add(new Variable { type = VariableType.ConstantLabel, name = "__endofprogram", staticLabel = end_label });
            globalScope.variables.Add(new Variable { name = "true", type = VariableType.Constant, constantValue = 1 });
            globalScope.variables.Add(new Variable { name = "false", type = VariableType.Constant, constantValue = 0 });

            try
            {
                rootNode.GatherSymbols(this, globalScope);
                rootNode.ResolveTypes(this, globalScope);
                //rootNode.FoldConstants(this);
            
                var r = new Assembly.Node();
                
                r.AddChild(rootNode.CompileFunction2(this));
                foreach (var dataItem in dataElements)
                {
                    if (dataItem.Item2 is Assembly.Label)
                        r.AddChild(new Assembly.StaticLabelData { label = dataItem.Item1, data = dataItem.Item2 as Assembly.Label });
                    else
                        r.AddChild(new Assembly.StaticData { label = dataItem.Item1, data = dataItem.Item2 as List<ushort> });
                }
                r.AddLabel(end_label);


                //r.CollapseTree(peepholes);
                return r;
            }
            catch (DCPUB.CompileError c)
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
