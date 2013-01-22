using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Node
    {
        public List<Node> children = new List<Node>();

        public void AddChild(Node child)
        {
            children.Add(child);
        }

        /*public void AddInstruction(Instructions instruction, String firstOperand, String secondOperand = null)
        {
            AddChild(Instruction.Make(instruction, new Operand { label = firstOperand, semantics = OperandSemantics.Label },
                new Operand { label = secondOperand, semantics = OperandSemantics.Label }));
        }*/

        public void AddInstruction(Instructions instruction, Operand firstOperand, Operand secondOperand = null)
        {
            AddChild(Instruction.Make(instruction, firstOperand, secondOperand));
        }

        public void AddLabel(Assembly.Label label)
        {
            AddChild(new LabelNode { label = label });
        }

        public virtual void Emit(EmissionStream stream) 
        {
            if (children.Count > 1) stream.indentDepth += 1;
            foreach (var child in children) child.Emit(stream);
            if (children.Count > 1) stream.indentDepth -= 1;
        }

        public virtual List<Node> CollapseTree() 
        {
            var result = new List<Node>();
            foreach (var child in children) result.AddRange(child.CollapseTree());
            children = result;
            return new List<Node>(new Node[]{this}); 
        }

        public virtual int InstructionCount()
        {
            return children.Sum((node) => { return node.InstructionCount(); });
        }

        public virtual void EmitBinary(List<Box<ushort>> binary)
        {
            foreach (var child in children) child.EmitBinary(binary);
        }

        public virtual void SetupLabels(Dictionary<string, Label> labelTable)
        { }
    }

    public class StatementNode : Node 
    {
        public override List<Node> CollapseTree()
        {
            var r = base.CollapseTree();
            Peephole.Peepholes.InitializePeepholes();
            if (Peephole.Peepholes.root != null)
                Peephole.Peepholes.root.ProcessAssembly(children);
            return r;
        }
    }

    public class ExpressionNode : Node 
    {
        public override List<Node> CollapseTree()
        {
            base.CollapseTree();
            return children;
        }
    }
}
