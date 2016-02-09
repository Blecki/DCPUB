using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class ReturnStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                AddChild("value", treeNode.ChildNodes[1].FirstChild.FirstChild);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddChild(new Annotation(context.GetSourceSpan(this.Span)));

            if (ChildNodes.Count > 0)
            {
                r.AddChild(Child(0).Emit(context, scope, Target.Stack));
                r.AddInstruction(Instructions.SET, Operand("A"), Operand("POP"));
            }
            r.AddChild(scope.activeFunction.CompileReturn(context, scope));

            return r;
        }

    }
}
