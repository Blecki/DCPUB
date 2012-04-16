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
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return "varref " + variableName + " [into:" + target.ToString() + "]";
        }

        public override bool IsIntegralConstant()
        {
            if (variable.type == VariableType.Constant)
                return true;
            return false;
        }

        public override ushort GetConstantValue()
        {
            return variable.constantValue;
        }

        public override string GetConstantToken()
        {
            return Hex.hex(GetConstantValue());
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

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            if (variable.type == VariableType.Constant)
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), Hex.hex(variable.constantValue));
            }
            else if (variable.type == VariableType.ConstantReference)
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), variable.staticLabel);
            }
            else if (variable.type == VariableType.Static)
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + variable.staticLabel + "]");
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.location == Register.STACK)
                {
                    var stackOffset = scope.StackOffset(variable.stackOffset);
                    if (stackOffset > 0)
                    {
                        context.Add("SET", Scope.TempRegister, "SP");
                        context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + Hex.hex(stackOffset) + "+" + Scope.TempRegister + "]", "Fetching variable");
                    }
                    else
                        context.Add("SET", Scope.GetRegisterLabelFirst((int)target), "PEEK", "Fetching variable");
                }
                else if (variable.location == target)
                {
                    //context.Add(";SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location));
                }
                else
                {
                    context.Add("SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location));
                }
            }

            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }

    
}
