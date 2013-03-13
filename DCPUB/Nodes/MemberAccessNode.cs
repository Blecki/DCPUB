using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class MemberAccessNode : CompilableNode, AssignableNode
    {
        public Member member = null;
        public String memberName;
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
            if (_struct == null) throw new CompileError(this, "Result of expression is not a struct");
            foreach (var _member in _struct.members)
                if (_member.name == memberName) member = _member;
            if (member == null) throw new CompileError(this, "Member " + memberName + " not found on " + _struct.name);
            ResultType = member.typeSpecifier;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (IsAssignedTo)
            {
                if (member.isArray) throw new CompileError(this, "Can't assign to arrays.");
                this.target = parentState.FindAndUseFreeRegister();
                Child(0).AssignRegisters(context, parentState, this.target);
                parentState.FreeMaybeRegister(this.target);
            }
            else
            {
                this.target = target;
                if (Child(0) is VariableNameNode && !member.isArray)
                    Child(0).AssignRegisters(context, parentState, Register.A); //Going to copy it to A anyway.
                else
                    Child(0).AssignRegisters(context, parentState, this.target);
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            r.AddChild(Child(0).Emit(context, scope));
            if (target == Register.STACK)
            {
                if (!member.isArray)
                {
                    if (Child(0).target != Register.A) 
                        r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));

                    if (member.offset > 0)
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"),
                            DereferenceOffset("A", (ushort)member.offset));
                    else
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Dereference("A"));
                }
                else
                    if (member.offset > 0)
                        r.AddInstruction(Assembly.Instructions.ADD, Operand("PEEK"), Constant((ushort)member.offset));
            }
            else
            {
                if (!member.isArray)
                {
                    if (member.offset > 0)
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            DereferenceOffset(Scope.GetRegisterLabelFirst((int)Child(0).target), (ushort)member.offset));
                    else
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            Dereference(Scope.GetRegisterLabelFirst((int)Child(0).target)));
                }
                else
                    if (member.offset > 0)
                        r.AddInstruction(Assembly.Instructions.ADD, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            Constant((ushort)member.offset));
            }
            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            Target objectTarget = target;
            if (target.target == Targets.Stack)
                objectTarget = Target.Register(context.AllocateRegister());

            r.AddChild(Child(0).Emit2(context, scope, objectTarget));
            if (member.isArray)
            {
                if (target != objectTarget)
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), objectTarget.GetOperand(TargetUsage.Pop));
                r.AddInstruction(Assembly.Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)member.offset));
            }
            else
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                    objectTarget.GetOperand(TargetUsage.Pop, Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Offset,
                    (ushort)member.offset));
            }
            
            return r;
        }

        Assembly.Node AssignableNode.EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode)
        {
            var r = new Assembly.TransientNode();
            //assume value is already in 'from'.
            r.AddChild(Child(0).Emit(context, scope));
            if (target == Register.STACK)
            {
                //throw new CompileError("Why wasn't a register available?");
                r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
                target = Register.A;
            }
            if (member.offset > 0)
                r.AddInstruction(opcode, DereferenceOffset(Scope.GetRegisterLabelFirst((int)target), (ushort)member.offset),
                    from);
            else
                r.AddInstruction(opcode, Dereference(Scope.GetRegisterLabelFirst((int)target)),
                    from);
            return r;
        }

        Assembly.Node AssignableNode.EmitAssignment2(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode)
        {
            var r = new Assembly.TransientNode();
            //assume value is already in 'from'.
            var target = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit2(context, scope, target));
            r.AddInstruction(opcode, target.GetOperand(TargetUsage.Peek,
                Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Offset, (ushort)member.offset),
                from);
            return r;
        }
    }

    
}
