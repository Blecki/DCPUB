using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BranchStatementNode : CompilableNode
    {
        public enum ClauseOrder
        {
            ConstantPass,
            ConstantFail,
            PassFirst,
            FailFirst
        }

        public ClauseOrder clauseOrder = ClauseOrder.ConstantFail;
        public String comparisonInstruction = "IFE";
        public CompilableNode firstOperand = null;
        public CompilableNode secondOperand = null;
        public Register firstOperandTarget = Register.STACK;
        public Register secondOperandTarget = Register.STACK;

        public override string TreeLabel()
        {
            return "branch " + clauseOrder.ToString();
        }

        public void FindClauseOrder(CompilableNode conditionNode)
        {
            if (conditionNode.IsIntegralConstant())
            {
                var value = conditionNode.GetConstantValue();
                if (value != 0) clauseOrder = ClauseOrder.ConstantPass;
                else clauseOrder = ClauseOrder.ConstantFail;
            }
            else if (conditionNode is ComparisonNode)
            {
                var @operator = conditionNode.AsString;
                firstOperand = conditionNode.Child(0);
                secondOperand = conditionNode.Child(1);
                if (@operator == "==") comparisonInstruction = "IFE";
                if (@operator == "!=") comparisonInstruction = "IFN";
                if (@operator == ">") comparisonInstruction = "IFG";
                if (@operator == "<") comparisonInstruction = "IFL";
                if (@operator == ">" && (firstOperand.ResultType == "signed" || secondOperand.ResultType == "signed"))
                    comparisonInstruction = "IFA";
                if (@operator == "<" && (firstOperand.ResultType == "signed" || secondOperand.ResultType == "signed"))
                    comparisonInstruction = "IFU";

                clauseOrder = ClauseOrder.FailFirst;
            }
            else
            {
                clauseOrder = ClauseOrder.FailFirst;
                comparisonInstruction = "IFN";
                firstOperand = conditionNode;
                var nln = new NumberLiteralNode();
                nln.Value = 0;
                secondOperand = nln;
            }
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            FindClauseOrder(Child(0));
            return this;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            foreach (var child in ChildNodes) (child as CompilableNode).GatherSymbols(context, enclosingScope);
        }

        public override void  ResolveTypes(CompileContext context, Scope enclosingScope)
        {
         	 base.ResolveTypes(context, enclosingScope);
            if (Child(0) is ComparisonNode)
            {
                firstOperand = Child(0).Child(0);
                secondOperand = Child(0).Child(1);
                if (firstOperand.ResultType != secondOperand.ResultType)
                    context.AddWarning(Span, "Comparison between " + firstOperand.ResultType + " and " +
                        secondOperand.ResultType + ". Comparison might be invalid.");
            }

        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (target != Register.DISCARD) throw new CompileError("Branch not at top level");
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantFail:
                    if (ChildNodes.Count > 2) Child(2).AssignRegisters(context, parentState, Register.DISCARD);
                    break;
                case ClauseOrder.ConstantPass:
                    Child(1).AssignRegisters(context, parentState, Register.DISCARD);
                    break;
                default:
                    {
                        if (!secondOperand.IsIntegralConstant())
                        {
                            secondOperandTarget = parentState.FindAndUseFreeRegister();
                            secondOperand.AssignRegisters(context, parentState, secondOperandTarget);
                        }
                        if (!firstOperand.IsIntegralConstant())
                        {
                            firstOperandTarget = parentState.FindAndUseFreeRegister();
                            firstOperand.AssignRegisters(context, parentState, firstOperandTarget);
                        }
                        parentState.FreeRegisters(firstOperandTarget, secondOperandTarget);

                        Child(1).AssignRegisters(context, parentState, Register.DISCARD);
                        if (ChildNodes.Count > 2) Child(2).AssignRegisters(context, parentState, Register.DISCARD);
                    }
                    break;
            }
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            if (!secondOperand.IsIntegralConstant())
                secondOperand.Emit(context, scope);
            if (!firstOperand.IsIntegralConstant())
                firstOperand.Emit(context, scope);

            if (!firstOperand.IsIntegralConstant() && firstOperandTarget == Register.STACK)
            {
                firstOperandTarget = Register.J;
                context.Add("SET", Scope.TempRegister, "POP");
            }

            context.Add(comparisonInstruction,
                firstOperand.IsIntegralConstant() ? firstOperand.GetConstantToken() : Scope.GetRegisterLabelSecond((int)firstOperandTarget),
                secondOperand.IsIntegralConstant() ? secondOperand.GetConstantToken() : Scope.GetRegisterLabelSecond((int)secondOperandTarget));
            if (firstOperandTarget == Register.STACK) scope.stackDepth -= 1;
            if (secondOperandTarget == Register.STACK) scope.stackDepth -= 1;
        }

        public static void EmitBlock(CompileContext context, Scope scope, CompilableNode block)
        {
            var blockScope = scope.Push();
            block.Emit(context, blockScope);
            if (blockScope.stackDepth - scope.stackDepth > 0)
                context.Add("ADD", "SP", Hex.hex(blockScope.stackDepth - scope.stackDepth));
        }
        

    }

    
}
