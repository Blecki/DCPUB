using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly.Peephole
{
    public class ReplacementInstruction
    {
        public string ins;
        public string firstOperand;
        public string secondOperand;
    }

    public class ReplacementSet : Irony.Interpreter.Ast.AstNode
    {
        public List<ReplacementInstruction> pattern = new List<ReplacementInstruction>();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var child in treeNode.ChildNodes)
            {
                pattern.Add(new ReplacementInstruction
                {
                    ins = child.ChildNodes[0].FindTokenAndGetText(),
                    firstOperand = child.ChildNodes[1].FindTokenAndGetText(),
                    secondOperand = child.ChildNodes[2].FindTokenAndGetText()
                });
            }
        }
    }
}
