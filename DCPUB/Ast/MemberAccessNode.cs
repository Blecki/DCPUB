﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class MemberAccessNode : CompilableNode, AssignableNode
    {
        public Model.Member member = null;
        public String memberName;
        public Model.Struct _struct = null;
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

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            _struct = enclosingScope.FindType(Child(0).ResultType);
            if (_struct == null)
            {
                context.ReportError(this, "Result of expression is not a struct");
                ResultType = "word";
            }
            else
            {
                foreach (var _member in _struct.members)
                    if (_member.name == memberName) member = _member;
                if (member == null)
                {
                    context.ReportError(this, "Member " + memberName + " not found on " + _struct.name);
                    ResultType = "word";
                }
                else
                    ResultType = member.typeSpecifier;
            }
        }
        
        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new TransientNode();

            if (member == null)
            {
                context.ReportError(this, "Member was not resolved.");
                return r;
            }

            Target objectTarget = target;
            if (target.target == Targets.Stack)
                objectTarget = Target.Register(context.AllocateRegister());

            r.AddChild(Child(0).Emit(context, scope, objectTarget));
            if (member.isArray)
            {
                if (target != objectTarget)
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), objectTarget.GetOperand(TargetUsage.Pop));
                r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)member.offset));
            }
            else
            {
                if (member.offset == 0)
                    r.AddInstruction(Instructions.SET,
                        target.GetOperand(TargetUsage.Push),
                        objectTarget.GetOperand(TargetUsage.Pop, Intermediate.OperandSemantics.Dereference));
                else
                    r.AddInstruction(Instructions.SET,
                        target.GetOperand(TargetUsage.Push),
                        objectTarget.GetOperand(TargetUsage.Pop, 
                            Intermediate.OperandSemantics.Dereference | Intermediate.OperandSemantics.Offset,
                            (ushort)member.offset));
            }
            
            return r;
        }

        Intermediate.IRNode AssignableNode.EmitAssignment(CompileContext context, Model.Scope scope, Intermediate.Operand from, Intermediate.Instructions opcode)
        {
            var r = new TransientNode();

            if (member == null)
            {
                context.ReportError(this, "Member was not resolved");
                return r;
            }

            var target = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, target));
            r.AddInstruction(opcode, target.GetOperand(TargetUsage.Peek,
                Intermediate.OperandSemantics.Dereference | Intermediate.OperandSemantics.Offset, (ushort)member.offset),
                from);
            return r;
        }
    }

    
}
