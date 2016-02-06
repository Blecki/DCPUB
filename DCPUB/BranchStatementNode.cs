using System;
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

        public override Assembly.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var conditionFetchToken = Child(0).GetFetchToken();
            if (conditionFetchToken != null &&
                (conditionFetchToken.semantics & Assembly.OperandSemantics.Constant) == Assembly.OperandSemantics.Constant)
            {
                if (conditionFetchToken.constant == 0)
                    clauseOrder = ClauseOrder.ConstantFail;
                else
                    clauseOrder = ClauseOrder.ConstantPass;
            }
            else if (Child(0) is ComparisonNode)
            {
                var @operator = Child(0).AsString;
                firstOperand = Child(0).Child(0);
                secondOperand = Child(0).Child(1);
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
                firstOperand = Child(0);
                var nln = new NumberLiteralNode();
                nln.Value = 0;
                secondOperand = nln;
            }

            var r = new Assembly.StatementNode();

            if (clauseOrder == ClauseOrder.ConstantPass || clauseOrder == ClauseOrder.ConstantFail) return r;

            Assembly.Operand secondToken = secondOperand.GetFetchToken();
            Assembly.Operand firstToken = firstOperand.GetFetchToken();

            if (secondToken == null)
            {
                var secondTarget = Target.Register(context.AllocateRegister());
                r.AddChild(secondOperand.Emit(context, scope, secondTarget));
                secondToken = secondTarget.GetOperand(TargetUsage.Pop);
            }

            if (firstToken == null)
            {
                var firstTarget = Target.Register(context.AllocateRegister());
                r.AddChild(firstOperand.Emit(context, scope, firstTarget));
                firstToken = firstTarget.GetOperand(TargetUsage.Pop);
            }
            
            r.AddInstruction(comparisonInstruction, firstToken, secondToken);
            return r;
        }

        public static Assembly.IRNode EmitBlock(CompileContext context, Scope scope, CompilableNode block, bool restoreStack = true)
        {
            if (block is BlockNode)
                return block.Emit(context, scope, Target.Discard);

            var r = new Assembly.TransientNode();

            var blockScope = scope.Push();
            r.AddChild(block.Emit(context, blockScope, Target.Discard));
            if (restoreStack && blockScope.variablesOnStack - scope.variablesOnStack > 0)
                r.AddInstruction(Assembly.Instructions.ADD, Operand("SP"),
                    Constant((ushort)(blockScope.variablesOnStack - scope.variablesOnStack)));
            return r;
        }

    }

    
}
