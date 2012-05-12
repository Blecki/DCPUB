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
            this.target = Register.A; //parentState.FindAndUseFreeRegister();
            Child(0).AssignRegisters(context, parentState, this.target);
            //parentState.FreeMaybeRegister(this.target);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (Child(0).ResultType != enclosingScope.activeFunction.ResultType)
                context.AddWarning(Span, CompileContext.TypeWarning(Child(0).ResultType, enclosingScope.activeFunction.ResultType));
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            Variable firstParameter = null;

            if (scope.activeFunction.function.parameterCount > 0)
            {
                //First parameter is in A.
                firstParameter = scope.activeFunction.function.localScope.variables[0];
                if (firstParameter.location == Register.A)
                {
                    firstParameter.location = Register.STACK;
                    firstParameter.stackOffset = scope.stackDepth;
                    scope.stackDepth += 1;
                    r.AddChild(new Assembly.Annotation("Moving first parameter to stack for duration of return"));
                    r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Operand("A"));
                }
                else
                    firstParameter = null;
            }

            r.AddChild(Child(0).Emit(context, scope));
            if (target != Register.A) r.AddInstruction(Assembly.Instructions.SET, Operand("A"), 
                Operand(Scope.GetRegisterLabelSecond((int)target)));
            r.AddChild(scope.activeFunction.CompileReturn(context, scope));

            //Move first parameter back. Don't need to restore it.
            if (firstParameter != null)
            {
                firstParameter.location = Register.A;
                scope.stackDepth -= 1;
            }
            return r;
        }

    }
}
