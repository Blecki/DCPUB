﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class DereferenceNode : CompilableNode
    {
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            ResultType = "unsigned";
        }

        public override string TreeLabel()
        {
            return "deref [into:" + target.ToString() + "]";
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
            Child(0).AssignRegisters(context, parentState, target);
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            Child(0).Emit(context, scope);
            if (target == Register.STACK)
            {
                context.Add("SET", "PEEK", "[PEEK]");
            }
            else
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target),
                    "[" + Scope.GetRegisterLabelSecond((int)target) + "]");
            }
        }

    }
    
}
