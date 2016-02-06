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

        public override Assembly.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Empty statement.");
        }

    }
}
