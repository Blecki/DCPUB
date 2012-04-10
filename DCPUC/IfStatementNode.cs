using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class IfStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
            AddChild("Block", treeNode.ChildNodes[2]);
            if (treeNode.ChildNodes.Count == 5) AddChild("Else", treeNode.ChildNodes[4]);
            this.AsString = "If";
        }

        private static void releaseRegister(Scope scope, int reg)
        {
            scope.FreeMaybeRegister(reg);
            if (reg == (int)Register.STACK) scope.stackDepth -= 1;
        }

        public enum ClauseOrder
        {
            ConstantPass,
            ConstantFail,
            PassFirst,
            FailFirst
        }

        public static ClauseOrder CompileConditional(Assembly assembly, Scope scope, CompilableNode conditionNode)
        {
            if (!(conditionNode is ComparisonNode))
            {
                var condTarget = scope.FindAndUseFreeRegister();
                conditionNode.Compile(assembly, scope, (Register)condTarget);
                assembly.Add("IFE", Scope.GetRegisterLabelSecond(condTarget), "0x0", "If from expression");
                scope.FreeMaybeRegister(condTarget);
                return ClauseOrder.PassFirst;
            }
            else
            {
                var op = conditionNode.AsString;
                ushort firstConstantValue = 0, secondConstantValue = 0;

                var firstIsConstant = (conditionNode.ChildNodes[0] as CompilableNode).IsConstant();
                var secondIsConstant = (conditionNode.ChildNodes[1] as CompilableNode).IsConstant();

                int firstRegister = (int)Register.STACK;
                int secondRegister = (int)Register.STACK;

                if (secondIsConstant)
                    secondConstantValue = (conditionNode.ChildNodes[1] as CompilableNode).GetConstantValue();
                else
                {
                    secondRegister = scope.FindAndUseFreeRegister();
                    (conditionNode.ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)secondRegister);
                }

                if (firstIsConstant)
                    firstConstantValue = (conditionNode.ChildNodes[0] as CompilableNode).GetConstantValue();
                else
                {
                    firstRegister = scope.FindAndUseFreeRegister();
                    (conditionNode.ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)firstRegister);
                }

                if (op == "==")
                {
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue == secondConstantValue) { return ClauseOrder.ConstantPass; }
                        else { return ClauseOrder.ConstantFail; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFE", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFE", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else
                    {
                        assembly.Add("IFE", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                }
                else if (op == "!=")
                {
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue != secondConstantValue) { return ClauseOrder.ConstantPass; }
                        else { return ClauseOrder.ConstantFail; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFN", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFN", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else
                    {
                        assembly.Add("IFN", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                }
                else if (op == ">")
                {
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue > secondConstantValue) { return ClauseOrder.ConstantPass; }
                        else { return ClauseOrder.ConstantFail; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFG", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFG", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        return ClauseOrder.FailFirst;
                    }
                    else
                    {
                        assembly.Add("IFG", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        return ClauseOrder.FailFirst;
                    }
                }

            }

            throw new CompileError("Impossible situation reached");
        }

        public static void CompileBlock(Assembly assembly, Scope scope, CompilableNode block)
        {
            var blockScope = BeginBlock(scope);
            assembly.Barrier();
            block.Compile(assembly, blockScope, Register.DISCARD);
            assembly.Barrier();
            EndBlock(assembly, blockScope);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var clauseOrder = CompileConditional(assembly, scope, ChildNodes[0] as CompilableNode);

            if (clauseOrder == ClauseOrder.ConstantPass)
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
            else if (clauseOrder == ClauseOrder.ConstantFail)
            {
                if (ChildNodes.Count == 3) CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
            }
            else if (clauseOrder == ClauseOrder.PassFirst)
            {
                var elseLabel = Scope.GetLabel() + "ELSE";
                var endLabel = Scope.GetLabel() + "END";

                assembly.Add("SET", "PC", elseLabel);
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                
                if (ChildNodes.Count == 3)
                {
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                    CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
                }

                assembly.Add(":" + endLabel, "", "");
            }
            else if (clauseOrder == ClauseOrder.FailFirst)
            {
                var elseLabel = Scope.GetLabel() + "ELSE";
                var endLabel = Scope.GetLabel() + "END";
                if (ChildNodes.Count == 3)
                {
                    assembly.Add("SET", "PC", elseLabel);
                    CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                }
                else
                {
                    assembly.Add("SET", "PC", elseLabel);
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                }

                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                assembly.Add(":" + endLabel, "", "");
            }

        }

    }

    
}
