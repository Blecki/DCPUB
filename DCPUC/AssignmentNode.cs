using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("LValue", treeNode.ChildNodes[0].ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            if (ChildNodes[0] is VariableNameNode)
            {
                var variable = scope.FindVariable(ChildNodes[0].AsString);
                if (variable == null) throw new CompileError("Could not find variable " + ChildNodes[0].AsString);


                if (variable.location == Register.STACK)
                {
                    var register = scope.FindAndUseFreeRegister();
                    (ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)register);
                    scope.FreeMaybeRegister(register);

                    if (scope.stackDepth - variable.stackOffset > 1)
                    {
                        assembly.Add("SET", Scope.TempRegister, "SP");
                        assembly.Add("SET", "[" + Hex.hex(scope.stackDepth - variable.stackOffset - 1) + "+" + Scope.TempRegister + "]",
                            Scope.GetRegisterLabelSecond(register), "Fetching variable");
                    }
                    else
                        assembly.Add("SET", "PEEK", Scope.GetRegisterLabelSecond(register), "Fetching variable");

                    if (register == (int)Register.STACK) scope.stackDepth -= 1;
                }
                else if (variable.location == Register.STATIC)
                {
                    //if (!variable.emitBrackets) throw new CompileError("Can't assign to data pointers!");
                     var register = scope.FindAndUseFreeRegister();
                    (ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)register);
                    scope.FreeMaybeRegister(register);
                    assembly.Add("SET", "[" + variable.staticLabel + "]", Scope.GetRegisterLabelSecond(register));
                    if (register == (int)Register.STACK) scope.stackDepth -= 1;
                }
                else if (variable.location == Register.CONST)
                {
                    throw new CompileError("Can't assign to const");
                }
                else
                    (ChildNodes[1] as CompilableNode).Compile(assembly, scope, variable.location);

            }
            else if (ChildNodes[0] is DereferenceNode)
            {
                bool firstConstant = false;
                int firstRegister = (int)Register.STACK;

                if ((ChildNodes[0].ChildNodes[0] as CompilableNode).IsConstant())
                    firstConstant = true;
                else
                {
                    firstRegister = scope.FindAndUseFreeRegister();
                    (ChildNodes[0].ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)firstRegister);
                }

                var secondRegister = scope.FindAndUseFreeRegister();
                (ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)secondRegister);

                if (firstConstant)
                {
                    assembly.Add("SET", "[" + Hex.hex((ChildNodes[0].ChildNodes[0] as CompilableNode).GetConstantValue()) + "]",
                        Scope.GetRegisterLabelSecond(secondRegister));
                    if (secondRegister == (int)Register.STACK)
                        scope.stackDepth -= 1;
                    else
                        scope.FreeMaybeRegister(secondRegister);
                }
                else
                {
                    if (firstRegister == (int)Register.STACK && secondRegister == (int)Register.STACK)
                    {
                        assembly.Add("SET", Scope.TempRegister, "POP");
                        assembly.Add("SET", "[" + Scope.TempRegister + "]", "POP");
                        scope.stackDepth -= 2;
                    }
                    else if (secondRegister == (int)Register.STACK)
                    {
                        assembly.Add("SET", "[" + Scope.GetRegisterLabelFirst(firstRegister) + "]", "POP");
                        scope.stackDepth -= 1;
                        scope.FreeMaybeRegister(firstRegister);
                        return;
                    }
                    else if (firstRegister == (int)Register.STACK)
                    {
                        throw new CompileError("Impossible situation entered");
                    }
                    else
                    {
                        assembly.Add("SET", "[" + Scope.GetRegisterLabelFirst(firstRegister) + "]", Scope.GetRegisterLabelSecond(secondRegister));
                        scope.FreeMaybeRegister(firstRegister);
                        scope.FreeMaybeRegister(secondRegister);
                    }
                }
            }
        }
    }

    
}
