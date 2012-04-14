using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class VariableDeclarationNode : CompilableNode
    {
        string declLabel = "";
        Variable variable = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Value", treeNode.ChildNodes[3].FirstChild);
            declLabel = treeNode.ChildNodes[0].FindTokenAndGetText();
            variable = new Variable();
            variable.name = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return declLabel + " " + variable.name;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            Child(0).GatherSymbols(context, enclosingScope);

            enclosingScope.variables.Add(variable);
            variable.scope = enclosingScope;
            
            if (declLabel == "var")
            {
                variable.type = VariableType.Local;
            }
            else if (declLabel == "static")
            {
                variable.type = VariableType.Static;
                variable.location = Register.STATIC;
                if (Child(0) is DataLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, (Child(0) as DataLiteralNode).dataLabel);
                }
                else if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, (Child(0) as BlockLiteralNode).dataLabel);
                }
                else if (Child(0).IsIntegralConstant()) //Other expressions should fold if they are constant.
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, Child(0).GetConstantValue());
                }
                else
                    throw new CompileError("Statics must be initialized to a constant value.");
            }
            else if (declLabel == "const")
            {
                variable.location = Register.CONST;

                if (Child(0) is DataLiteralNode)
                {
                    variable.staticLabel = (Child(0) as DataLiteralNode).dataLabel;
                    variable.type = VariableType.ConstantReference;
                }
                else if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = (Child(0) as DataLiteralNode).dataLabel;
                    variable.type = VariableType.ConstantReference;
                }
                else if (Child(0) is NumberLiteralNode) //Other expressions should fold if they are constant.
                {
                    variable.constantValue = (Child(0) as NumberLiteralNode).Value;
                    variable.type = VariableType.Constant;
                }
                else
                    throw new CompileError("Consts must be initialized to a constant value.");
            }
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            variable.stackOffset = scope.stackDepth;
            if (variable.type == VariableType.Local)
            {
                variable.location = (Register)scope.FindAndUseFreeRegister();
                if (variable.location == Register.I)
                {
                    variable.location = Register.STACK;
                    scope.FreeRegister((int)Register.I);
                }

                if (Child(0) is BlockLiteralNode)
                {
                    var size = (Child(0) as BlockLiteralNode).dataSize;
                    scope.stackDepth += size;
                    context.Add("SUB", "SP", Hex.hex(size));
                    context.Add("SET", Scope.GetRegisterLabelFirst((int)variable.location), "SP");
                    if (variable.location == Register.STACK) scope.stackDepth += 1;
                }
                else
                    Child(0).Compile(context, scope, variable.location);
            }
        }
    }
}
