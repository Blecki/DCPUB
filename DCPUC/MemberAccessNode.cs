using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class MemberAccessNode : CompilableNode, AssignableNode
    {
        public Member member = null;
        public String memberName;
        Register target;
        public Struct _struct = null;
        public bool IsAssignedTo { get; set; }

        public MemberAccessNode()
        {
            IsAssignedTo = false;
        }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            memberName = treeNode.ChildNodes[2].FindTokenAndGetText();
            AddChild("Expression", treeNode.ChildNodes[0]);
        }

        public override string TreeLabel()
        {
            return "memref " + memberName + " [into:" + target.ToString() + "]";
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            _struct = enclosingScope.FindType(Child(0).ResultType);
            if (_struct == null) throw new CompileError("Result of expression is not a struct");
            foreach (var _member in _struct.members)
                if (_member.name == memberName) member = _member;
            if (member == null) throw new CompileError("Member " + memberName + " not found on " + _struct.name);
            ResultType = member.typeSpecifier;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (IsAssignedTo)
                this.target = parentState.FindAndUseFreeRegister();
            else
                this.target = target;
            Child(0).AssignRegisters(context, parentState, this.target);
            if (IsAssignedTo) parentState.FreeMaybeRegister(this.target);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            r.AddChild(Child(0).Emit(context, scope));
            if (target == Register.STACK)
            {
                r.AddInstruction(Assembly.Instructions.SET, Scope.TempRegister, "POP");
                if (member.offset > 0)
                    r.AddInstruction(Assembly.Instructions.SET, "PUSH", "[" + Scope.TempRegister + " + " + Hex.hex(member.offset) + "]");
                else
                    r.AddInstruction(Assembly.Instructions.SET, "PUSH", "[" + Scope.TempRegister + "]");
            }
            else
            {
                if (member.offset > 0)
                    r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target),
                        "[" + Scope.GetRegisterLabelFirst((int)target) + " + " + Hex.hex(member.offset) + "]");
                else
                    r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target),
                        "[" + Scope.GetRegisterLabelFirst((int)target) + "]");
            }
            return r;
        }

        Assembly.Node AssignableNode.EmitAssignment(CompileContext context, Scope scope, Register from, Assembly.Instructions opcode)
        {
            var r = new Assembly.ExpressionNode();
            //assume value is already in 'from'.
            r.AddChild(Child(0).Emit(context, scope));
            if (target == Register.STACK)
            {
                r.AddInstruction(Assembly.Instructions.SET, Scope.TempRegister, "POP");
                target = Register.J;
                scope.stackDepth -= 1;
            }
            if (member.offset > 0)
                r.AddInstruction(opcode, "[" + Scope.GetRegisterLabelFirst((int)target) + "+" + Hex.hex(member.offset) + "]",
                    Scope.GetRegisterLabelSecond((int)from));
            else
                r.AddInstruction(opcode, "[" + Scope.GetRegisterLabelFirst((int)target) + "]",
                    Scope.GetRegisterLabelSecond((int)from));
            return r;
        }
    }

    
}
