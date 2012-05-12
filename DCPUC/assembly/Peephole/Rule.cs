using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly.Peephole
{
    public class Rule : Irony.Interpreter.Ast.AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Replacement Pattern", treeNode.ChildNodes[1]);
            AddChild("Result", treeNode.ChildNodes[3]);
        }
    }
}
