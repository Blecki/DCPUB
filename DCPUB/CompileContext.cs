﻿using System;
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
        private List<Tuple<Assembly.Label, List<Assembly.Operand>>> dataElements = new List<Tuple<Assembly.Label, List<Assembly.Operand>>>();
        public int externalCount = 0;
        private int nextLabelID = 0;
        public string source = null;
        public CompileOptions options = new CompileOptions();
        public Assembly.Peephole.Peepholes peepholes;
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

        public void AddData(Assembly.Label label, List<Assembly.Operand> FetchTokens)
        {
            dataElements.Add(Tuple.Create(label, FetchTokens));
        }

        public void AddData(Assembly.Label label, Assembly.Label data)
        {
            AddData(label, new List<Assembly.Operand>(new Assembly.Operand[] { CompilableNode.Label(data) }));
        }

        public void AddData(Assembly.Label label, ushort word)
        {
            AddData(label, new List<Assembly.Operand>(new Assembly.Operand[] { CompilableNode.Constant(word) }));
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
                    onError("%PARSER " + msg.Level + ": " + msg.Message + " [line:" + msg.Location.Line + " column:" + msg.Location.Column + "]\r\n");
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

        public Assembly.Node Compile(Action<string> OnError)
        {
            this.OnError = OnError;

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

                if (options.externals)
                {
                    var startOfProgram = new Assembly.Label("STARTOFPROGRAM");
                    r.AddInstruction(Assembly.Instructions.SET, CompilableNode.Operand("PC"), CompilableNode.Label(startOfProgram));
                    r.AddChild(new Assembly.Annotation("External data block. Your assembler or program loader should fill these in."));
                    r.AddLabel(new Assembly.Label("EXTERNALS"));
                    foreach (var variable in globalScope.variables)
                    {
                        if (variable.type == VariableType.External)
                        {
                            var blankList = new List<Assembly.Operand>(new Assembly.Operand[] { CompilableNode.Constant(0) });

                            r.AddChild(new Assembly.MixedStaticData
                            {
                                label = new Assembly.Label("__external_" + variable.name),
                                Data = blankList
                            });
                        }
                    }

                    r.AddLabel(startOfProgram);
                }
                
                r.AddChild(rootNode.CompileFunction(this));
                foreach (var dataItem in dataElements)
                    r.AddChild(new Assembly.MixedStaticData { label = dataItem.Item1, Data = dataItem.Item2 });
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
