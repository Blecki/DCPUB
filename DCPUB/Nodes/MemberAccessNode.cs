﻿using System;
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

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            Target objectTarget = target;
            if (target.target == Targets.Stack)
                objectTarget = Target.Register(context.AllocateRegister());

            r.AddChild(Child(0).Emit(context, scope, objectTarget));
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
            var target = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, target));
            r.AddInstruction(opcode, target.GetOperand(TargetUsage.Peek,
                Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Offset, (ushort)member.offset),
                from);
            return r;
        }
    }

    
}
