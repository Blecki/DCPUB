using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    class IndexOperatorNode : DereferenceNode
    {
        public CompilableNode baseNode;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("deref", treeNode.ChildNodes[0]);

            var expressionNode = new BinaryOperationNode();

            expressionNode.SetOp("+");
            expressionNode.ChildNodes.Add(Child(0)); //offset
            expressionNode.ChildNodes.Add(Child(1)); //base pointer
            baseNode = Child(1);

            ChildNodes.Clear();
            ChildNodes.Add(expressionNode);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            this.ResultType = baseNode.ResultType;
        }
    }
}
