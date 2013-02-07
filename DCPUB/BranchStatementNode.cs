﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
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
                if (@operator == "->") comparisonInstruction = Assembly.Instructions.IFA;
                if (@operator == "-<") comparisonInstruction = Assembly.Instructions.IFU;

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
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
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

            if (clauseOrder == ClauseOrder.ConstantPass || clauseOrder == ClauseOrder.ConstantFail) return r;

            Assembly.Operand secondToken = null;
            if (!secondOperand.IsIntegralConstant())
            {
                secondToken = secondOperand.GetFetchToken(scope);
                if (secondToken == null)
                {
                    r.AddChild(secondOperand.Emit(context, scope));
                    secondToken = Operand(Scope.GetRegisterLabelSecond((int)secondOperandTarget));
                }
            }
            else
                secondToken = secondOperand.GetConstantToken();

            if (!firstOperand.IsIntegralConstant())
                r.AddChild(firstOperand.Emit(context, scope));

            if (!firstOperand.IsIntegralConstant() && firstOperandTarget == Register.STACK)
            {
                //throw new CompileError("No free registers to implement the conditional with.");
                firstOperandTarget = Register.A;
                r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
            }

            r.AddInstruction(comparisonInstruction,
                firstOperand.IsIntegralConstant() ? firstOperand.GetConstantToken() : Operand(Scope.GetRegisterLabelSecond((int)firstOperandTarget)),
                secondToken);
            return r;
        }

        public static Assembly.Node EmitBlock(CompileContext context, Scope scope, CompilableNode block, bool restoreStack = true)
        {

            if (block is BlockNode)
                return block.Emit(context, scope);

            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation("Entering branchnode emit block"));

            var blockScope = scope.Push();
            r.AddChild(block.Emit(context, blockScope));
            if (restoreStack && blockScope.variablesOnStack - scope.variablesOnStack > 0)
                r.AddInstruction(Assembly.Instructions.ADD, Operand("SP"), 
                    Constant((ushort)(blockScope.variablesOnStack - scope.variablesOnStack)));
            return r;
        }
        

    }

    
}