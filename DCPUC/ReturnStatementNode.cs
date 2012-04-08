using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class ReturnStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("value", treeNode.ChildNodes[1]);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
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
