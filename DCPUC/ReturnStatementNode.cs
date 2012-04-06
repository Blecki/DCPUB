using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class ReturnStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("value", treeNode.ChildNodes[1]);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope);
            assembly.Add("SET A, POP");
            scope.stackDepth -= 1;
            scope.activeFunction.CompileReturn(assembly);
        }
    }
}
