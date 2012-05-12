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

    public class StatementNode : Node 
    {
        public override List<Node> CollapseTree()
        {
            var r = base.CollapseTree();

            /*PeepholePattern.InitializePeepholes();

            for (int i = 0; i < children.Count;)
            {
                foreach (var pattern in PeepholePattern.patterns)
                {
                    if (children.Count - i < pattern.Length) continue;
                    var iList = new List<Instruction>();
                    for (int x = 0; x < pattern.Length; ++x)
                    {
                        if (!(children[i + x] is Instruction)) goto TryNextPattern;
                        else iList.Add(children[i + x] as Instruction);
                    }

                    if (pattern.Match(iList))
                    {
                        children.RemoveRange(i, pattern.Length);
                        children.InsertRange(i, pattern.Replace(iList));
                        goto TryNextChild;
                    }
                    TryNextPattern: ;
                }

                i += 1;
                TryNextChild: ;
            }
            */
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
