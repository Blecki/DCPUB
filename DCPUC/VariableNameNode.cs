using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class VariableNameNode : CompilableNode
    {
        public Variable variable = null;
        public String variableName;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            var scope = enclosingScope;
            while (variable == null && scope != null)
            {
                foreach (var v in scope.variables)
                    if (v.name == variableName)
                        variable = v;
                if (variable == null) scope = scope.parent;
            }

            if (variable == null) 
                throw new CompileError("Could not find variable " + variableName);
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            if (variable.location == Register.CONST)
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), variable.staticLabel);
            }
            else if (variable.location == Register.STATIC)
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + variable.staticLabel + "]");
            }
            else if (variable.location == Register.STACK)
            {
                if (scope.stackDepth - variable.stackOffset > 1)
                {
                    context.Add("SET", Scope.TempRegister, "SP");
                    context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + Hex.hex(scope.stackDepth - variable.stackOffset - 1) + "+" + Scope.TempRegister + "]", "Fetching variable");
                }
                else
                    context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "PEEK", "Fetching variable");
            }
            else
            {
                if (target == variable.location) return;
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location), "Fetching variable");
            }
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }

    
}
