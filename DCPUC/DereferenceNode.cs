using System;
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

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            var destRegister = target == Register.STACK ? scope.FindAndUseFreeRegister() : (int)target;
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)destRegister);
            if (target == Register.STACK)
            {
                if (destRegister == (int)Register.STACK)
                    assembly.Add("SET", "PEEK", "[PEEK]");
                else
                {
                    assembly.Add("SET", "PUSH", "[" + Scope.GetRegisterLabelSecond(destRegister) + "]");
                    scope.stackDepth += 1;
                    scope.FreeMaybeRegister(destRegister);
                }
            }
            else
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + Scope.GetRegisterLabelSecond(destRegister) + "]");
                if (destRegister == (int)Register.STACK)
                    scope.stackDepth -= 1;
                else if (destRegister != (int)target)
                    scope.FreeMaybeRegister(destRegister);
            }

        }
    }

    
}
