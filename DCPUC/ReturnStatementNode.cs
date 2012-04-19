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

        public override void Emit(CompileContext context, Scope scope)
        {
            Child(0).Emit(context, scope);
            if (target != Register.A) context.Add("SET", "A", Scope.GetRegisterLabelSecond((int)target));
            scope.activeFunction.CompileReturn(context, scope);
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            var reg = scope.FindAndUseFreeRegister();
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)reg);
            if (reg != (int)Register.A) assembly.Add("SET", "A", Scope.GetRegisterLabelSecond(reg));
            scope.FreeMaybeRegister(reg);
            if (reg == (int)Register.STACK) scope.stackDepth -= 1;
            scope.activeFunction.CompileReturn(assembly, scope);
        }
    }
}
