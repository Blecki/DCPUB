using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class BinaryOperator
    {
        public Instructions instruction;
        public Func<ushort, ushort, ushort> fold;
    }

    public class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, BinaryOperator> opcodes = null;
        public Register firstOperandResult = Register.STACK;
        public Register secondOperandResult = Register.STACK;

        public void SetOp(string op) { AsString = op; }

        public static BinaryOperator MakeBinOp(Instructions ins, Func<ushort, ushort, ushort> fold)
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

                opcodes.Add("+", MakeBinOp(Instructions.ADD, (a, b) => (ushort)((ushort)a + (ushort)b) ));
                opcodes.Add("-", MakeBinOp(Instructions.SUB, (a, b) => (ushort)((ushort)a - (ushort)b)));
                opcodes.Add("*", MakeBinOp(Instructions.MUL, (a, b) => (ushort)((ushort)a * (ushort)b)));
                opcodes.Add("/", MakeBinOp(Instructions.DIV, (a, b) => (ushort)((ushort)a / (ushort)b)));
                opcodes.Add("-*", MakeBinOp(Instructions.MLI, (a, b) => (ushort)((short)a * (short)b)));
                opcodes.Add("-/", MakeBinOp(Instructions.DVI, (a, b) => (ushort)((short)a / (short)b)));
                opcodes.Add("%", MakeBinOp(Instructions.MOD, (a, b) => (ushort)((ushort)a % (ushort)b)));
                opcodes.Add("-%", MakeBinOp(Instructions.MDI, (a, b) => (ushort)((short)a % (short)b)));
                opcodes.Add("<<", MakeBinOp(Instructions.SHL, (a, b) => (ushort)((ushort)a << (ushort)b)));
                opcodes.Add(">>", MakeBinOp(Instructions.SHR, (a, b) => (ushort)((ushort)a >> (ushort)b)));
                opcodes.Add("&", MakeBinOp(Instructions.AND, (a, b) => (ushort)((ushort)a & (ushort)b)));
                opcodes.Add("|", MakeBinOp(Instructions.BOR, (a, b) => (ushort)((ushort)a | (ushort)b)));
                opcodes.Add("^", MakeBinOp(Instructions.XOR, (a, b) => (ushort)((ushort)a ^ (ushort)b)));
                opcodes.Add("==", MakeBinOp(Instructions.IFE, (a, b) => a == b ? (ushort)1 : (ushort)0));
                opcodes.Add("!=", MakeBinOp(Instructions.IFN, (a, b) => a != b ? (ushort)1 : (ushort)0));
                opcodes.Add(">", MakeBinOp(Instructions.IFG, (a, b) => a > b ? (ushort)1 : (ushort)0));
                opcodes.Add("<", MakeBinOp(Instructions.IFL, (a, b) => a < b ? (ushort)1 : (ushort)0));
                opcodes.Add("->", MakeBinOp(Instructions.IFA, (a, b) => (short)a > (short)b ? (ushort)1 : (ushort)0));
                opcodes.Add("-<", MakeBinOp(Instructions.IFU, (a, b) => (short)a < (short)b ? (ushort)1 : (ushort)0));
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

        public override Intermediate.Operand GetFetchToken()
        {
            initOps();
            var A = Child(0).GetFetchToken();
            var B = Child(1).GetFetchToken();
            if (A != null && (A.semantics & Intermediate.OperandSemantics.Constant) == Intermediate.OperandSemantics.Constant
                && B != null && (B.semantics & Intermediate.OperandSemantics.Constant) == Intermediate.OperandSemantics.Constant)
                return Constant(opcodes[AsString].fold(A.constant, B.constant));
            return null;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            initOps();
            Target firstTarget = null;
            Target secondTarget = null;
            
            var r = new TransientNode();

            var opcode = opcodes[AsString];
            bool isComparison =
                (opcode.instruction >= Instructions.IFB && opcode.instruction <= Instructions.IFU);

            if (isComparison)
            {
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Constant(0));
                firstTarget = Target.Register(context.AllocateRegister());
            }
            else
                firstTarget = target;

            var firstFetchToken = Child(0).GetFetchToken();
            var secondFetchToken = Child(1).GetFetchToken();

            if (secondFetchToken == null)
            {
                secondTarget = Target.Register(context.AllocateRegister());
                r.AddChild(Child(1).Emit(context, scope, secondTarget));
                secondFetchToken = secondTarget.GetOperand(TargetUsage.Pop);
            }

            if (firstFetchToken == null)
                r.AddChild(Child(0).Emit(context, scope, firstTarget));
            else
                r.AddInstruction(Instructions.SET, firstTarget.GetOperand(TargetUsage.Push), firstFetchToken);
            firstFetchToken = firstTarget.GetOperand(TargetUsage.Peek);

            if (isComparison)
            {
                r.AddInstruction(opcode.instruction, firstFetchToken, secondFetchToken);
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Peek), Constant(1));
            }
            else
            {
                if (target != firstTarget)
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Peek), firstFetchToken);
                r.AddInstruction(opcode.instruction, target.GetOperand(TargetUsage.Peek), secondFetchToken);
            }


            return r;
        }
    }
}
