using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class ReturnStatementNode : CompilableNode
    {
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("value", treeNode.ChildNodes[1]);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = parentState.FindAndUseFreeRegister();
            Child(0).AssignRegisters(context, parentState, this.target);
            parentState.FreeMaybeRegister(this.target);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (Child(0).ResultType != enclosingScope.activeFunction.ResultType)
                context.AddWarning(Span, CompileContext.TypeWarning(Child(0).ResultType, enclosingScope.activeFunction.ResultType));
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            r.AddChild(Child(0).Emit(context, scope));
            if (target != Register.A) r.AddInstruction(Assembly.Instructions.SET, "A", Scope.GetRegisterLabelSecond((int)target));
            r.AddChild(scope.activeFunction.CompileReturn(context, scope));
            return r;
        }

    }
}
