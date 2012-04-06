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
            AddChild("Name", treeNode.ChildNodes[1]);
            //if (treeNode.ChildNodes.Count == 4)
            //    AddChild("Value", treeNode.ChildNodes[3]);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            var newVariable = new Variable();
            newVariable.name = AsString;
            newVariable.scope = scope;
            newVariable.stackOffset = scope.stackDepth;

            scope.variables.Add(newVariable);

            assembly.Add("SUB SP, 0x0001");
            scope.stackDepth += 1;

        }
    }
}
