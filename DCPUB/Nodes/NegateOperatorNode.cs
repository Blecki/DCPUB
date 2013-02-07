using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    class NegateOperatorNode : BinaryOperationNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            SkipInit = true;
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[1]);
            ChildNodes.Add(new NumberLiteralNode { Value = 0x8000 });
            AsString = "^";
            
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            ResultType = "word";
        }
    }
}
