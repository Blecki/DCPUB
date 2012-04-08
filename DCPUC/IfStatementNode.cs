using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class IfStatementNode : CompilableNode
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

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var firstClause = 0;
            var secondClause = 0;
            var constantClause = 0;

            if (!(ChildNodes[0] is ComparisonNode))
            {
                var condTarget = scope.FindAndUseFreeRegister();
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)condTarget);
                assembly.Add("IFE", Scope.GetRegisterLabelSecond(condTarget), "0x0", "If from expression");
                scope.FreeMaybeRegister(condTarget);
                firstClause = 1;
                secondClause = 2;
            }
            else
            {
                var op = ChildNodes[0].AsString;
                ushort firstConstantValue = 0, secondConstantValue = 0;

                var firstIsConstant = (ChildNodes[0].ChildNodes[0] as CompilableNode).IsConstant();
                var secondIsConstant = (ChildNodes[0].ChildNodes[1] as CompilableNode).IsConstant();

                int firstRegister = (int)Register.STACK;
                int secondRegister = (int)Register.STACK;

                if (secondIsConstant)
                    secondConstantValue = (ChildNodes[0].ChildNodes[1] as CompilableNode).GetConstantValue();
                else
                {
                    secondRegister = scope.FindAndUseFreeRegister();
                    (ChildNodes[0].ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)secondRegister);
                }

                if (firstIsConstant)
                    firstConstantValue = (ChildNodes[0].ChildNodes[0] as CompilableNode).GetConstantValue();
                else
                {
                    firstRegister = scope.FindAndUseFreeRegister();
                    (ChildNodes[0].ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)firstRegister);
                }

                if (op == "==")
                {
                    firstClause = 2;
                    secondClause = 1;
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue == secondConstantValue) { constantClause = 1; goto Emit; }
                        else { constantClause = 2; goto Emit; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFE", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFE", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        goto Emit;
                    }
                    else
                    {
                        assembly.Add("IFE", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }                        
                }
                else if (op == "!=")
                {
                    firstClause = 2;
                    secondClause = 1;
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue != secondConstantValue) { constantClause = 1; goto Emit; }
                        else { constantClause = 2; goto Emit; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFN", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFN", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        goto Emit;
                    }
                    else
                    {
                        assembly.Add("IFN", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }
                }
                else if (op == ">")
                {
                    firstClause = 2;
                    secondClause = 1;
                    if (firstIsConstant && secondIsConstant)
                    {
                        if (firstConstantValue > secondConstantValue) { constantClause = 1; goto Emit; }
                        else { constantClause = 2; goto Emit; }
                    }
                    else if (firstIsConstant)
                    {
                        assembly.Add("IFG", hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }
                    else if (secondIsConstant)
                    {
                        assembly.Add("IFG", Scope.GetRegisterLabelSecond(firstRegister), hex(secondConstantValue));
                        releaseRegister(scope, firstRegister);
                        goto Emit;
                    }
                    else
                    {
                        assembly.Add("IFG", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                        releaseRegister(scope, firstRegister);
                        releaseRegister(scope, secondRegister);
                        goto Emit;
                    }
                }

            }

            Emit:
            if (constantClause != 0)
            {
                if (constantClause < ChildNodes.Count)
                {
                    var blockScope = BeginBlock(scope);
                    assembly.Barrier();
                    (ChildNodes[constantClause] as CompilableNode).Compile(assembly, blockScope, Register.DISCARD);
                    assembly.Barrier();
                    EndBlock(assembly, blockScope);
                }
            }
            else
            {
                var elseLabel = Scope.GetLabel() + "ELSE";
                var endLabel = Scope.GetLabel() + "END";



                assembly.Add("SET", "PC", elseLabel);
                if (firstClause != 0 && firstClause < ChildNodes.Count)
                {
                    var blockScope = BeginBlock(scope);
                    assembly.Barrier();
                    (ChildNodes[firstClause] as CompilableNode).Compile(assembly, blockScope, Register.DISCARD);
                    assembly.Barrier();
                    EndBlock(assembly, blockScope);
                }
                if (secondClause != 0 && secondClause < ChildNodes.Count)
                {
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                    var elseScope = BeginBlock(scope);
                    assembly.Barrier();
                    (ChildNodes[secondClause] as CompilableNode).Compile(assembly, elseScope, Register.DISCARD);
                    assembly.Barrier();
                    EndBlock(assembly, elseScope);
                    assembly.Add(":" + endLabel, "", "");
                }
                else
                    assembly.Add(":" + elseLabel, "", "");
            }


        }

    }

    
}
