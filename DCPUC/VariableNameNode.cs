using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class VariableNameNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var variable = scope.FindVariable(AsString);
            if (variable == null) throw new CompileError("Could not find variable " + AsString);
            if (variable.location == Register.STATIC)
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + variable.staticLabel + "]");
            }
            else if (variable.location == Register.STACK)
            {
                if (scope.stackDepth - variable.stackOffset > 1)
                {
                    assembly.Add("SET", Scope.TempRegister, "SP");
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + hex(scope.stackDepth - variable.stackOffset - 1) + "+" + Scope.TempRegister + "]", "Fetching variable");
                }
                else
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "PEEK", "Fetching variable");
            }
            else
            {
                if (target == variable.location) return;
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location), "Fetching variable");
            }
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }

    
}
