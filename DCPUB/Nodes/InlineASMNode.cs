using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class RegisterBindingNode : CompilableNode
    {
        public string targetRegisterName = "";
        public bool preserveTarget = false;
        public Register targetRegister;
        public Scope rememberScope = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            targetRegisterName = treeNode.ChildNodes[0].FindTokenAndGetText();
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                AddChild("expression", treeNode.ChildNodes[1].FirstChild.ChildNodes[1]);
        }

        public override void  ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            targetRegister = (Register)Enum.Parse(typeof(Register), this.targetRegisterName);
            rememberScope = enclosingScope;
            if (ChildNodes.Count > 0) Child(0).ResolveTypes(context, enclosingScope);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            //if (preserveTarget)
                r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Target.Raw(targetRegister).GetOperand(TargetUsage.Pop));
            if (ChildNodes.Count > 0) r.AddChild(Child(0).Emit(context, scope, Target.Raw(targetRegister)));
            return r;
        }

        public Assembly.Node Restore(CompileContext context, Scope scope)
        {
            var r = new Assembly.TransientNode();
            if (preserveTarget)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelSecond((int)targetRegister)), 
                    Operand("POP"));
            }
            return r;
        }

        public Assembly.Node Restore2(CompileContext context, Scope scope)
        {
            var r = new Assembly.TransientNode();
            //if (preserveTarget)
                r.AddInstruction(Assembly.Instructions.SET, Target.Raw(targetRegister).GetOperand(TargetUsage.Push),
                    Operand("POP"));
            return r;
        }
    }

    public class InlineASMNode : CompilableNode
    {
        public string rawAssembly = "";
        public static Irony.Parsing.Parser asmParser = new Irony.Parsing.Parser(new Assembly.AssemblyGrammar());
        private Irony.Parsing.ParseTree ParsedAssembly = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                foreach (var child in treeNode.ChildNodes[1].FirstChild.FirstChild.ChildNodes)
                    AddChild("bound register", child);
            rawAssembly = treeNode.ChildNodes[2].FindTokenAndGetText();

            ParsedAssembly = asmParser.Parse(rawAssembly);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();

            if (ParsedAssembly.HasErrors())
            {
                foreach (var error in ParsedAssembly.ParserMessages)
                    context.ReportError(new Irony.Parsing.SourceSpan(error.Location, 20), error.Message);
                return r;
            }

            var parsedNode = (ParsedAssembly.Root.AstNode as Assembly.InstructionListAstNode).resultNode;

            parsedNode.ErrorCheck(context, this);

            for (var i = 0; i < ChildNodes.Count; ++i)
                r.AddChild(Child(i).Emit(context, scope, null));

            r.AddChild(parsedNode);

            for (var i = ChildNodes.Count - 1; i >= 0; --i)
                r.AddChild((Child(i) as RegisterBindingNode).Restore2(context, scope));

            return r;
        }
    }

    
}
