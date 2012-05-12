using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly.Peephole
{
    public class RuleSet : Irony.Interpreter.Ast.AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
                AddChild("Rule", child);
        }
    }
}
