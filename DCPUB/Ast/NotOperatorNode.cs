using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Ast
{
    class NotOperatorNode : BinaryOperationNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            SkipInit = true;
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
            ChildNodes.Add(new NumberLiteralNode { Value = 0xFFFF });
            AsString = "^";
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            ResultType = "word";
        }
    }
}
