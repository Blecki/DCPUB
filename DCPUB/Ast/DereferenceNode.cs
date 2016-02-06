using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class DereferenceNode : CompilableNode, AssignableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            ResultType = "word";
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new TransientNode();
            Target childTarget = target;
            if (target.target == Targets.Stack) childTarget = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, childTarget));
            r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), 
                childTarget.GetOperand(TargetUsage.Pop, Intermediate.OperandSemantics.Dereference));
            return r;
        }

        Intermediate.IRNode AssignableNode.EmitAssignment(CompileContext context, Scope scope, Intermediate.Operand from, Instructions opcode)
        {
            var r = new TransientNode();
            var target = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, target));
            if (target.target == Targets.Stack)
            {
                r.AddInstruction(Instructions.SET, Operand("A"), Operand("POP"));
                target = Target.Raw(Register.A);
            }
            r.AddInstruction(opcode, target.GetOperand(TargetUsage.Push, Intermediate.OperandSemantics.Dereference), from);
            return r;
        }
    }
    
}
