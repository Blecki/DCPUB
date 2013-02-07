using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly.Peephole
{
    public class Matcher : AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
                AddChild("matcher", child);
        }

        public bool Match(List<Node> assembly, int startIndex, Dictionary<string, Operand> values)
        {
            for (var i = 0; i < ChildNodes.Count; ++i)
            {
                if (i + startIndex >= assembly.Count) return false;
                if (!(assembly[i + startIndex] is Instruction)) return false;
                if (!(ChildNodes[i] as WholeInstructionMatcher).Match(assembly[i + startIndex] as Instruction, values)) return false;
            }
            return true;
        }
    }
}
