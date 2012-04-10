using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class VariableDeclarationNode : CompilableNode
    {
        string declLabel = "";

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Value", treeNode.ChildNodes[3].FirstChild);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
            declLabel = treeNode.ChildNodes[0].FindTokenAndGetText();

        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var newVariable = new Variable();
            newVariable.name = AsString;
            newVariable.scope = scope;
            newVariable.stackOffset = scope.stackDepth;
            if (declLabel == "var")
            {
                newVariable.location = (Register)scope.FindAndUseFreeRegister();
                if (newVariable.location == Register.I) newVariable.location = Register.STACK;
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, newVariable.location);
            }
            else if (declLabel == "static")
            {
                newVariable.location = Register.STATIC;
                if ((ChildNodes[0] as CompilableNode).IsConstant())
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    Scope.AddData(newVariable.staticLabel, new List<ushort>(new ushort[] { (ChildNodes[0] as CompilableNode).GetConstantValue() }));
                }
                else
                {
                    throw new CompileError("Statics must be initialized to a constant value");
                }
            }

            scope.variables.Add(newVariable);
            //scope.stackDepth += 1;

        }
    }
}
