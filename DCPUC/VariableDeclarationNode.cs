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

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Value", treeNode.ChildNodes[3].FirstChild);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
            declLabel = treeNode.ChildNodes[0].FindTokenAndGetText();

        }

        public override string TreeLabel()
        {
            return "Vardecl " + declLabel + " " + AsString;
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
                if (ChildNodes[0] is BlockLiteralNode)
                {
                    var size = Child(0).GetConstantValue();
                    scope.stackDepth += size;
                    assembly.Add("SUB", "SP", hex(size));
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)newVariable.location), "SP");
                    if (newVariable.location == Register.STACK) scope.stackDepth += 1;                  
                }
                else
                    (ChildNodes[0] as CompilableNode).Compile(assembly, scope, newVariable.location);

            }
            else if (declLabel == "static")
            {
                newVariable.location = Register.STATIC;
                if (Child(0) is BlockLiteralNode)
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    var datLabel = Scope.GetLabel() + "_STATIC_" + AsString + "_DATA";
                    Scope.AddData(datLabel, (ChildNodes[0] as BlockLiteralNode).MakeData());
                    Scope.AddData(newVariable.staticLabel, datLabel);
                }
                else if ((ChildNodes[0] as CompilableNode).IsConstant())
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    Scope.AddData(newVariable.staticLabel, new List<ushort>(new ushort[] { (ChildNodes[0] as CompilableNode).GetConstantValue() }));
                }
                else if (ChildNodes[0] is DataLiteralNode)
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    var datLabel = Scope.GetLabel() + "_STATIC_" + AsString + "_DATA";
                    Scope.AddData(datLabel, (ChildNodes[0] as DataLiteralNode).data);
                    Scope.AddData(newVariable.staticLabel, datLabel);
                }
                else
                    throw new CompileError("Statics must be initialized to a constant value");
            }
            else if (declLabel == "const")
            {
                newVariable.location = Register.CONST;
                if (Child(0) is BlockLiteralNode)
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    Scope.AddData(newVariable.staticLabel, (ChildNodes[0] as BlockLiteralNode).MakeData());
                }
                else if ((ChildNodes[0] as CompilableNode).IsConstant())
                {
                    //throw new CompileError("Initializing const to integrals is not supported yet");
                    newVariable.staticLabel = (ChildNodes[0] as CompilableNode).GetConstantToken();
                    //Scope.AddData(newVariable.staticLabel, (ChildNodes[0] as CompilableNode).GetConstantToken());
                }
                else if (ChildNodes[0] is DataLiteralNode)
                {
                    newVariable.staticLabel = Scope.GetLabel() + "_STATIC_" + AsString;
                    Scope.AddData(newVariable.staticLabel, (ChildNodes[0] as DataLiteralNode).data);
                }
            }

            scope.variables.Add(newVariable);
            //scope.stackDepth += 1;

        }
    }
}
