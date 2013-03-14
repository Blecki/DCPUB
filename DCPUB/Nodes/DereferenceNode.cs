using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

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

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            Target childTarget = target;
            if (target.target == Targets.Stack) childTarget = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, childTarget));
            r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), 
                childTarget.GetOperand(TargetUsage.Pop, Assembly.OperandSemantics.Dereference));
            return r;
        }

        Assembly.Node AssignableNode.EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode)
        {
            var r = new Assembly.TransientNode();
            var target = Target.Register(context.AllocateRegister());
            r.AddChild(Child(0).Emit(context, scope, target));
            if (target.target == Targets.Stack)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
                target = Target.Raw(Register.A);
            }
            r.AddInstruction(opcode, target.GetOperand(TargetUsage.Push, Assembly.OperandSemantics.Dereference), from);
            return r;
        }
    }
    
}
