using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class RegisterBindingNode : CompilableNode
    {
        public string targetRegisterName = "";
        public bool preserveTarget = false;
        public Model.Register targetRegister;
        public Model.Scope rememberScope = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            targetRegisterName = treeNode.ChildNodes[0].FindTokenAndGetText();
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                AddChild("expression", treeNode.ChildNodes[1].FirstChild.ChildNodes[1]);
        }

        public override void  ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            try
            {
                targetRegister = (Model.Register)Enum.Parse(typeof(Model.Register), this.targetRegisterName);
            }
            catch (Exception)
            {
                targetRegister = Model.Register.A;
                context.ReportError(this, "Invalid register name");
            }
            rememberScope = enclosingScope;
            if (ChildNodes.Count > 0) Child(0).ResolveTypes(context, enclosingScope);
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddChild(new Annotation(context.GetSourceSpan(this.Span)));
            //if (preserveTarget)
            //    r.AddInstruction(Instructions.SET, Operand("PUSH"), Target.Raw(targetRegister).GetOperand(TargetUsage.Pop));
            if (ChildNodes.Count > 0) r.AddChild(Child(0).Emit(context, scope, Target.Raw(targetRegister)));
            return r;
        }

        public Intermediate.IRNode Restore2(CompileContext context, Model.Scope scope)
        {
            var r = new TransientNode();
            //if (preserveTarget)
                r.AddInstruction(Instructions.SET, Target.Raw(targetRegister).GetOperand(TargetUsage.Push),
                    Operand("POP"));
            return r;
        }
    }

    public class InlineASMNode : CompilableNode
    {
        public string rawAssembly = "";
        public static Irony.Parsing.Parser asmParser = new Irony.Parsing.Parser(new AssemblyGrammar());
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

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new TransientNode();

            if (ParsedAssembly.HasErrors())
            {
                foreach (var error in ParsedAssembly.ParserMessages)
                    context.ReportError(new Irony.Parsing.SourceSpan(error.Location + this.Location, 20), error.Message);
                return r;
            }

            var parsedNode = (ParsedAssembly.Root.AstNode as Ast.Assembly.InstructionListAstNode).resultNode;

            parsedNode.ErrorCheck(context, this);

            for (var i = 0; i < ChildNodes.Count; ++i)
                r.AddChild(Child(i).Emit(context, scope, null));

            r.AddChild(parsedNode);

            //for (var i = ChildNodes.Count - 1; i >= 0; --i)
            //    r.AddChild((Child(i) as RegisterBindingNode).Restore2(context, scope));

            return r;
        }
    }

    
}
