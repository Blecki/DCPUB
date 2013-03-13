﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class BinaryOperator
    {
        public Assembly.Instructions instruction;
        public Func<ushort, ushort, ushort> fold;
    }

    public class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, BinaryOperator> opcodes = null;
        public Register firstOperandResult = Register.STACK;
        public Register secondOperandResult = Register.STACK;

        public override string TreeLabel()
        {
            return AsString + " " + ResultType;
        }

        public void SetOp(string op) { AsString = op; }

        public static BinaryOperator MakeBinOp(Assembly.Instructions ins, Func<ushort, ushort, ushort> fold)
        {
            return new BinaryOperator
            {
                instruction = ins,
                fold = fold
            };
        }

        public void initOps()
        {
            if (opcodes == null)
            {
                opcodes = new Dictionary<string, BinaryOperator>();

                opcodes.Add("+", MakeBinOp(Assembly.Instructions.ADD, (a, b) => (ushort)((ushort)a + (ushort)b) ));
                opcodes.Add("-", MakeBinOp(Assembly.Instructions.SUB, (a, b) => (ushort)((ushort)a - (ushort)b)));
                opcodes.Add("*", MakeBinOp(Assembly.Instructions.MUL, (a, b) => (ushort)((ushort)a * (ushort)b)));
                opcodes.Add("/", MakeBinOp(Assembly.Instructions.DIV, (a, b) => (ushort)((ushort)a / (ushort)b)));
                opcodes.Add("-*", MakeBinOp(Assembly.Instructions.MLI, (a, b) => (ushort)((short)a * (short)b)));
                opcodes.Add("-/", MakeBinOp(Assembly.Instructions.DVI, (a, b) => (ushort)((short)a / (short)b)));
                opcodes.Add("%", MakeBinOp(Assembly.Instructions.MOD, (a, b) => (ushort)((ushort)a % (ushort)b)));
                opcodes.Add("-%", MakeBinOp(Assembly.Instructions.MDI, (a, b) => (ushort)((short)a % (short)b)));
                opcodes.Add("<<", MakeBinOp(Assembly.Instructions.SHL, (a, b) => (ushort)((ushort)a << (ushort)b)));
                opcodes.Add(">>", MakeBinOp(Assembly.Instructions.SHR, (a, b) => (ushort)((ushort)a >> (ushort)b)));
                opcodes.Add("&", MakeBinOp(Assembly.Instructions.AND, (a, b) => (ushort)((ushort)a & (ushort)b)));
                opcodes.Add("|", MakeBinOp(Assembly.Instructions.BOR, (a, b) => (ushort)((ushort)a | (ushort)b)));
                opcodes.Add("^", MakeBinOp(Assembly.Instructions.XOR, (a, b) => (ushort)((ushort)a ^ (ushort)b)));
                opcodes.Add("==", MakeBinOp(Assembly.Instructions.IFE, (a, b) => a == b ? (ushort)1 : (ushort)0));
                opcodes.Add("!=", MakeBinOp(Assembly.Instructions.IFN, (a, b) => a != b ? (ushort)1 : (ushort)0));
                opcodes.Add(">", MakeBinOp(Assembly.Instructions.IFG, (a, b) => a > b ? (ushort)1 : (ushort)0));
                opcodes.Add("<", MakeBinOp(Assembly.Instructions.IFL, (a, b) => a < b ? (ushort)1 : (ushort)0));
                opcodes.Add("->", MakeBinOp(Assembly.Instructions.IFA, (a, b) => (short)a > (short)b ? (ushort)1 : (ushort)0));
                opcodes.Add("-<", MakeBinOp(Assembly.Instructions.IFU, (a, b) => (short)a < (short)b ? (ushort)1 : (ushort)0));
            }
        }

        protected bool SkipInit = false;
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (SkipInit) return;
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            ResultType = "word";
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            initOps();

            var first = Child(0).FoldConstants(context);
            var second = Child(1).FoldConstants(context);

            if (first.IsIntegralConstant() && second.IsIntegralConstant())
            {
                var a = first.GetConstantValue();
                var b = second.GetConstantValue();
                var ins = opcodes[AsString];
                return new NumberLiteralNode { Value = ins.fold((ushort)a,(ushort)b), 
                    WasFolded = true, ResultType = ResultType, Span = Span };
            }

            return this;
        }

        public override Assembly.Operand GetFetchToken()
        {
            initOps();
            var A = Child(0).GetFetchToken();
            var B = Child(1).GetFetchToken();
            if (A != null && (A.semantics & Assembly.OperandSemantics.Constant) == Assembly.OperandSemantics.Constant
                && B != null && (B.semantics & Assembly.OperandSemantics.Constant) == Assembly.OperandSemantics.Constant)
                return Constant(opcodes[AsString].fold(A.constant, B.constant));
            return null;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            initOps();
            this.target = target;

             var opcode = opcodes[AsString];
             bool isComparison =
                (opcode.instruction >= Assembly.Instructions.IFB && opcode.instruction <= Assembly.Instructions.IFU);

             var secondFetchToken = Child(1).GetFetchToken();
            
             if (secondFetchToken == null)
             {
                 secondOperandResult = parentState.FindAndUseFreeRegister();
                 Child(1).AssignRegisters(context, parentState, secondOperandResult);
             }

             if (isComparison)
             {
                 firstOperandResult = parentState.FindAndUseFreeRegister();
                 Child(0).AssignRegisters(context, parentState, firstOperandResult);
             }
             else
             {
                 firstOperandResult = target;
                 Child(0).AssignRegisters(context, parentState, firstOperandResult);
             }
             
             if (secondFetchToken == null) parentState.FreeMaybeRegister(secondOperandResult);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.TransientNode();

            var opcode = opcodes[AsString];
            bool isComparison =
                (opcode.instruction >= Assembly.Instructions.IFB && opcode.instruction <= Assembly.Instructions.IFU);

            if (isComparison)
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), Constant(0));

            var firstFetchToken = Child(0).GetFetchToken();
            var secondFetchToken = Child(1).GetFetchToken();

            if (secondFetchToken == null)
            {
                r.AddChild(Child(1).Emit(context, scope));
                secondFetchToken = Operand(Scope.GetRegisterLabelSecond((int)secondOperandResult));
            }

            if (firstFetchToken == null)
                r.AddChild(Child(0).Emit(context, scope));
            else
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                    firstFetchToken);

            firstFetchToken = Operand(Scope.GetPeekLabel((int)firstOperandResult));
            

            if (isComparison)
                r.AddInstruction(opcode.instruction, firstFetchToken, secondFetchToken);
            else
            {
                if (target != firstOperandResult)
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), firstFetchToken);
                r.AddInstruction(opcode.instruction, Operand(Scope.GetPeekLabel((int)target)), secondFetchToken);
            }
                
            if (isComparison)
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetPeekLabel((int)target)), Constant(1));
            
            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            initOps();
            Target firstTarget = null;
            Target secondTarget = null;
            
            var r = new Assembly.TransientNode();

            var opcode = opcodes[AsString];
            bool isComparison =
                (opcode.instruction >= Assembly.Instructions.IFB && opcode.instruction <= Assembly.Instructions.IFU);

            if (isComparison)
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), Constant(0));
                firstTarget = Target.Register(context.AllocateRegister());
            }
            else
                firstTarget = target;

            var firstFetchToken = Child(0).GetFetchToken();
            var secondFetchToken = Child(1).GetFetchToken();

            if (secondFetchToken == null)
            {
                secondTarget = Target.Register(context.AllocateRegister());
                r.AddChild(Child(1).Emit2(context, scope, secondTarget));
                secondFetchToken = secondTarget.GetOperand(TargetUsage.Pop);
            }

            if (firstFetchToken == null)
                r.AddChild(Child(0).Emit2(context, scope, firstTarget));
            else
                r.AddInstruction(Assembly.Instructions.SET, firstTarget.GetOperand(TargetUsage.Push), firstFetchToken);
            firstFetchToken = firstTarget.GetOperand(TargetUsage.Peek);

            if (isComparison)
            {
                r.AddInstruction(opcode.instruction, firstFetchToken, secondFetchToken);
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Peek), Constant(1));
            }
            else
            {
                if (target != firstTarget)
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Peek), firstFetchToken);
                r.AddInstruction(opcode.instruction, target.GetOperand(TargetUsage.Peek), secondFetchToken);
            }


            return r;
        }
    }
}
