using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
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

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (ChildNodes.Count > 0)
            {
                if (parentState.RegisterInUse(Register.B)) throw new CompileError("Sanity check failed. Return not at top level.");
                this.target = Register.B; //parentState.FindAndUseFreeRegister();
                parentState.UseRegister(Register.B);
                Child(0).AssignRegisters(context, parentState, this.target);
                parentState.FreeRegister(Register.B);
            }
            //parentState.FreeMaybeRegister(this.target);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (ChildNodes.Count > 0 && Child(0).ResultType != enclosingScope.activeFunction.ResultType)
                context.AddWarning(Span, CompileContext.TypeWarning(Child(0).ResultType, enclosingScope.activeFunction.ResultType));
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            if (ChildNodes.Count > 0)
            {
                r.AddChild(Child(0).Emit(context, scope));
                if (target != Register.A) r.AddInstruction(Assembly.Instructions.SET, Operand("A"),
                    Operand(Scope.GetRegisterLabelSecond((int)target)));
            }
            r.AddChild(scope.activeFunction.CompileReturn(context, scope));

            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            if (ChildNodes.Count > 0)
            {
                r.AddChild(Child(0).Emit2(context, scope, Target.Stack));
                r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
            }
            r.AddChild(scope.activeFunction.CompileReturn2(context, scope));

            return r;
        }

    }
}
