using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class NullStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "NULL STATEMENT";
        }
        public override CompilableNode FoldConstants(CompileContext context)
        {
            return null;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError(this, "Null statement should have been folded.");
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Empty statement.");
        }

    }
}
