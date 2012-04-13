using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BlockNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var f in treeNode.ChildNodes)
                AddChild("Statement", f);
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            foreach (var child in ChildNodes)
            {
                assembly.Barrier();
                (child as CompilableNode).Compile(assembly, scope, Register.DISCARD);
            }


        }

    }
}
