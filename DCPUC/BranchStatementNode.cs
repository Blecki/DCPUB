using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BranchStatementNode : CompilableNode
    {
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
                        assembly.Add("IFE", (conditionNode.ChildNodes[0] as CompilableNode).GetConstantToken(), Scope.GetRegisterLabelSecond(secondRegister));
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

        

    }

    
}
