using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;
using DCPUB.Model;

namespace DCPUB.Ast
{
    public class BinaryOperator
    {
        public Func<ushort, ushort, ushort> fold;

        public virtual IRNode Emit(CompileContext Context, Model.Scope Scope, Target Target, CompilableNode Lhs, CompilableNode Rhs)
        {
            throw new NotImplementedException();
        }
    }

    public class BasicBinaryOperator : BinaryOperator
    {
        public Instructions Instruction;

        public override IRNode Emit(CompileContext Context, Scope Scope, Target Target, CompilableNode Lhs, CompilableNode Rhs)
        {
            Target firstTarget = Target;
            Target secondTarget = null;

            var r = new TransientNode();

            var firstFetchToken = Lhs.GetFetchToken();
            var secondFetchToken = Rhs.GetFetchToken();

            if (secondFetchToken == null)
            {
                secondTarget = Target.Register(Context.AllocateRegister());
                r.AddChild(Rhs.Emit(Context, Scope, secondTarget));
                secondFetchToken = secondTarget.GetOperand(TargetUsage.Pop);
            }

            if (firstFetchToken == null)
                r.AddChild(Lhs.Emit(Context, Scope, firstTarget));
            else
                r.AddInstruction(Instructions.SET, firstTarget.GetOperand(TargetUsage.Push), firstFetchToken);
            firstFetchToken = firstTarget.GetOperand(TargetUsage.Peek);

            r.AddInstruction(Instruction, Target.GetOperand(TargetUsage.Peek), secondFetchToken);
            
            return r;
        }
    }

    public class ComparisonBinaryOperator : BinaryOperator
    {
        public Instructions Instruction;

        public override IRNode Emit(CompileContext Context, Scope Scope, Target Target, CompilableNode Lhs, CompilableNode Rhs)
        {
            Target firstTarget = null;
            Target secondTarget = null;

            var r = new TransientNode();


                r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Push), CompilableNode.Constant(0));
                firstTarget = Target.Register(Context.AllocateRegister());

            var firstFetchToken = Lhs.GetFetchToken();
            var secondFetchToken = Rhs.GetFetchToken();

            if (secondFetchToken == null)
            {
                secondTarget = Target.Register(Context.AllocateRegister());
                r.AddChild(Rhs.Emit(Context, Scope, secondTarget));
                secondFetchToken = secondTarget.GetOperand(TargetUsage.Pop);
            }

            if (firstFetchToken == null)
                r.AddChild(Lhs.Emit(Context, Scope, firstTarget));
            else
                r.AddInstruction(Instructions.SET, firstTarget.GetOperand(TargetUsage.Push), firstFetchToken);

            firstFetchToken = firstTarget.GetOperand(TargetUsage.Peek);

            r.AddInstruction(Instruction, firstFetchToken, secondFetchToken);
            r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Peek), CompilableNode.Constant(1));

            return r;
        }
    }

    public class LogicalBinaryOperator : BinaryOperator
    {
        public Instructions Instruction;
        public ushort ShortCircuitValue; // Short circuit the logic if the first value == this.

        public override IRNode Emit(CompileContext Context, Scope Scope, Target Target, CompilableNode Lhs, CompilableNode Rhs)
        {
            var r = new TransientNode();

            var shortCircuitLabel = Intermediate.Label.Make("SHORTCIRCUIT");

            var firstTarget = Target.Register(Context.AllocateRegister());
            var firstFetchToken = Lhs.GetFetchToken();
            if (firstFetchToken == null)
            {
                r.AddChild(Lhs.Emit(Context, Scope, firstTarget));
                r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Push), CompilableNode.Constant(0));
                r.AddInstruction(Instructions.IFN, Target.GetOperand(TargetUsage.Peek), firstTarget.GetOperand(TargetUsage.Peek));
                r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Peek), CompilableNode.Constant(1));
            }
            else
            {
                r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Push), CompilableNode.Constant(0));
                r.AddInstruction(Instructions.IFN, Target.GetOperand(TargetUsage.Peek), firstFetchToken);
                r.AddInstruction(Instructions.SET, Target.GetOperand(TargetUsage.Peek), CompilableNode.Constant(1));
            }

            r.AddInstruction(Instructions.IFE, Target.GetOperand(TargetUsage.Peek), CompilableNode.Constant(ShortCircuitValue));
            r.AddInstruction(Instructions.SET, CompilableNode.Operand("PC"), CompilableNode.Label(shortCircuitLabel));

            var secondTarget = Target.Register(Context.AllocateRegister());
            var intermediate = Target.Register(Context.AllocateRegister());
            var secondFetchToken = Rhs.GetFetchToken();
            if (secondFetchToken == null)
            {
                r.AddChild(Rhs.Emit(Context, Scope, secondTarget));
                r.AddInstruction(Instructions.SET, intermediate.GetOperand(TargetUsage.Push), CompilableNode.Constant(0));
                r.AddInstruction(Instructions.IFN, intermediate.GetOperand(TargetUsage.Peek), secondTarget.GetOperand(TargetUsage.Peek));
                r.AddInstruction(Instructions.SET, intermediate.GetOperand(TargetUsage.Peek), CompilableNode.Constant(1));
            }
            else
            {
                r.AddInstruction(Instructions.SET, intermediate.GetOperand(TargetUsage.Push), CompilableNode.Constant(0));
                r.AddInstruction(Instructions.IFN, intermediate.GetOperand(TargetUsage.Peek), secondFetchToken);
                r.AddInstruction(Instructions.SET, intermediate.GetOperand(TargetUsage.Peek), CompilableNode.Constant(1));
            }

            r.AddInstruction(Instruction, Target.GetOperand(TargetUsage.Peek), intermediate.GetOperand(TargetUsage.Peek));

            r.AddLabel(shortCircuitLabel);
            
            return r;
        }
    }




    public class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, BinaryOperator> opcodes = null;

        public void SetOp(string op) { AsString = op; }

        public static BinaryOperator MakeBinOp(Instructions ins, Func<ushort, ushort, ushort> fold)
        {
            return new BasicBinaryOperator
            {
                Instruction = ins,
                fold = fold
            };
        }

        public static ComparisonBinaryOperator MakeComparatorOp(Instructions ins, Func<ushort, ushort, ushort> fold)
        {
            return new ComparisonBinaryOperator
            {
                Instruction = ins,
                fold = fold
            };
        }

        public static LogicalBinaryOperator MakeLogicalOp(Instructions ins, 
            ushort ShortCircuitValue,
            Func<ushort, ushort, ushort> fold)
        {
            return new LogicalBinaryOperator
            {
                Instruction = ins,
                ShortCircuitValue = ShortCircuitValue,
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
                opcodes.Add("==", MakeComparatorOp(Instructions.IFE, (a, b) => a == b ? (ushort)1 : (ushort)0));
                opcodes.Add("!=", MakeComparatorOp(Instructions.IFN, (a, b) => a != b ? (ushort)1 : (ushort)0));
                opcodes.Add(">", MakeComparatorOp(Instructions.IFG, (a, b) => a > b ? (ushort)1 : (ushort)0));
                opcodes.Add("<", MakeComparatorOp(Instructions.IFL, (a, b) => a < b ? (ushort)1 : (ushort)0));
                opcodes.Add("->", MakeComparatorOp(Instructions.IFA, (a, b) => (short)a > (short)b ? (ushort)1 : (ushort)0));
                opcodes.Add("-<", MakeComparatorOp(Instructions.IFU, (a, b) => (short)a < (short)b ? (ushort)1 : (ushort)0));
                opcodes.Add("&&", MakeLogicalOp(Instructions.AND, 0, (a, b) => (ushort)((a != 0 && b != 0) ? 1 : 0)));
                opcodes.Add("||", MakeLogicalOp(Instructions.BOR, 1, (a, b) => (ushort)((a != 0 || b != 0) ? 1 : 0)));
            }
        }

        protected bool SkipInit = false;
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (SkipInit) return;
            //Console.WriteLine("{0} {1} {2}\n", treeNode.ChildNodes[0].ToString(), treeNode.ChildNodes[1].ToString(), treeNode.ChildNodes[2].ToString());

            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
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

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            initOps();
            var opcode = opcodes[AsString];
            return opcode.Emit(context, scope, target, Child(0), Child(1));
        }
    }
}
