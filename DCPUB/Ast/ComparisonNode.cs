using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class ComparisonNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            throw new InternalError("Branch node should have handled this.");
        }

    }

    
}
