using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class VariableDeclarationNode : CompilableNode
    {

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Value", treeNode.ChildNodes[3]);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            var newVariable = new Variable();
            newVariable.name = AsString;
            newVariable.scope = scope;
            newVariable.stackOffset = scope.stackDepth;

            (ChildNodes[0] as CompilableNode).Compile(assembly, scope);
            scope.variables.Add(newVariable);
            scope.stackDepth += 1;

        }
    }
}
