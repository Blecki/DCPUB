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
            return declLabel + " " + AsString;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            enclosingScope.variables.Add(variable);
            variable.scope = enclosingScope;
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            variable.stackOffset = scope.stackDepth;
            if (declLabel == "var")
            {
                variable.location = (Register)scope.FindAndUseFreeRegister();
                if (variable.location == Register.I) variable.location = Register.STACK;
                if (ChildNodes[0] is BlockLiteralNode)
                {
                    var size = Child(0).GetConstantValue();
                    scope.stackDepth += size;
                    context.Add("SUB", "SP", Hex.hex(size));
                    context.Add("SET", Scope.GetRegisterLabelFirst((int)variable.location), "SP");
                    if (variable.location == Register.STACK) scope.stackDepth += 1;                  
                }
                else
                    Child(0).Compile(context, scope, variable.location);

            }
            else if (declLabel == "static")
            {
                variable.location = Register.STATIC;
                if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + AsString;
                    var datLabel = context.GetLabel() + "_STATIC_" + AsString + "_DATA";
                    context.AddData(datLabel, (ChildNodes[0] as BlockLiteralNode).MakeData());
                    context.AddData(variable.staticLabel, datLabel);
                }
                else if ((ChildNodes[0] as CompilableNode).IsConstant())
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + AsString;
                    context.AddData(variable.staticLabel, new List<ushort>(new ushort[] { (ChildNodes[0] as CompilableNode).GetConstantValue() }));
                }
                else if (ChildNodes[0] is DataLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + AsString;
                    var datLabel = context.GetLabel() + "_STATIC_" + AsString + "_DATA";
                    context.AddData(datLabel, (ChildNodes[0] as DataLiteralNode).data);
                    context.AddData(variable.staticLabel, datLabel);
                }
                else
                    throw new CompileError("Statics must be initialized to a constant value");
            }
            else if (declLabel == "const")
            {
                variable.location = Register.CONST;
                if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + AsString;
                    context.AddData(variable.staticLabel, (ChildNodes[0] as BlockLiteralNode).MakeData());
                }
                else if ((ChildNodes[0] as CompilableNode).IsConstant())
                {
                    //throw new CompileError("Initializing const to integrals is not supported yet");
                    variable.staticLabel = (ChildNodes[0] as CompilableNode).GetConstantToken();
                    //Scope.AddData(newVariable.staticLabel, (ChildNodes[0] as CompilableNode).GetConstantToken());
                }
                else if (ChildNodes[0] is DataLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + AsString;
                    context.AddData(variable.staticLabel, (ChildNodes[0] as DataLiteralNode).data);
                }
            }

            scope.variables.Add(variable);
            //scope.stackDepth += 1;

        }
    }
}
