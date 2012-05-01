using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    class IndexOperatorNode : DereferenceNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("deref", treeNode.ChildNodes[0]);

            var expressionNode = new BinaryOperationNode();
            expressionNode.SetOp("+");
            expressionNode.ChildNodes.Add(Child(1));
            expressionNode.ChildNodes.Add(Child(0));

            ChildNodes.Clear();
            ChildNodes.Add(expressionNode);
        }
    }
}
