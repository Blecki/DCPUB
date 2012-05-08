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

        public void AddInstruction(Instructions instruction, String firstOperand, String secondOperand = null)
        {
            AddChild(Instruction.Make(instruction, firstOperand, secondOperand));
        }

        public void AddLabel(String label)
        {
            AddChild(new Label { label = label });
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
    }

    public class StatementNode : Node { }

    public class ExpressionNode : Node 
    {
        public override List<Node> CollapseTree()
        {
            base.CollapseTree();
            return children;
        }
    }
}
