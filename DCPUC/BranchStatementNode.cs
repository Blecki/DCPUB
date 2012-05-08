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
        public Assembly.Instructions comparisonInstruction = Assembly.Instructions.IFE;
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
                if (@operator == "==") comparisonInstruction = Assembly.Instructions.IFE;
                if (@operator == "!=") comparisonInstruction = Assembly.Instructions.IFN;
                if (@operator == ">") comparisonInstruction = Assembly.Instructions.IFG;
                if (@operator == "<") comparisonInstruction = Assembly.Instructions.IFL;
                if (@operator == ">" && (firstOperand.ResultType == "signed" || secondOperand.ResultType == "signed"))
                    comparisonInstruction = Assembly.Instructions.IFA;
                if (@operator == "<" && (firstOperand.ResultType == "signed" || secondOperand.ResultType == "signed"))
                    comparisonInstruction = Assembly.Instructions.IFU;

                clauseOrder = ClauseOrder.FailFirst;
            }
            else
            {
                clauseOrder = ClauseOrder.FailFirst;
                comparisonInstruction = Assembly.Instructions.IFN;
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
            //if (target != Register.DISCARD) throw new CompileError("Branch not at top level");
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantFail:
                    if (ChildNodes.Count > 2) Child(2).AssignRegisters(context, parentState, target);
                    break;
                case ClauseOrder.ConstantPass:
                    Child(1).AssignRegisters(context, parentState, target);
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

                        Child(1).AssignRegisters(context, parentState, target);
                        if (ChildNodes.Count > 2) Child(2).AssignRegisters(context, parentState, target);
                    }
                    break;
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            if (!secondOperand.IsIntegralConstant())
                r.AddChild(secondOperand.Emit(context, scope));
            if (!firstOperand.IsIntegralConstant())
                r.AddChild(firstOperand.Emit(context, scope));

            if (!firstOperand.IsIntegralConstant() && firstOperandTarget == Register.STACK)
            {
                firstOperandTarget = Register.J;
                r.AddInstruction(Assembly.Instructions.SET, Scope.TempRegister, "POP");
            }

            r.AddInstruction(comparisonInstruction,
                firstOperand.IsIntegralConstant() ? firstOperand.GetConstantToken() : Scope.GetRegisterLabelSecond((int)firstOperandTarget),
                secondOperand.IsIntegralConstant() ? secondOperand.GetConstantToken() : Scope.GetRegisterLabelSecond((int)secondOperandTarget));
            if (firstOperandTarget == Register.STACK) scope.stackDepth -= 1;
            if (secondOperandTarget == Register.STACK) scope.stackDepth -= 1;

            return r;
        }

        public static Assembly.Node EmitBlock(CompileContext context, Scope scope, CompilableNode block, bool restoreStack = true)
        {
            var r = new Assembly.StatementNode();
            var blockScope = scope.Push();
            r.AddChild(block.Emit(context, blockScope));
            if (restoreStack && blockScope.stackDepth - scope.stackDepth > 0)
                r.AddInstruction(Assembly.Instructions.ADD, "SP", Hex.hex(blockScope.stackDepth - scope.stackDepth));
            return r;
        }
        

    }

    
}
