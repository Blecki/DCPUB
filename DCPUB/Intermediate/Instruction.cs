using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    public partial class Instruction : IRNode
    {
        public Instructions instruction;
        public Operand firstOperand;
        public Operand secondOperand;

        internal override void ErrorCheck(CompileContext Context, Ast.CompilableNode Ast)
        {
            if (firstOperand == null)
                Context.ReportError(Ast, "No operands for instruction");
            else if (instruction.GetOperandCount() == 1 && secondOperand != null)
                Context.ReportError(Ast, "Instruction takes one argument - " + instruction.ToString());
            else if (instruction.GetOperandCount() == 2 && secondOperand == null)
                Context.ReportError(Ast, "Instruction takes two arguments - " + instruction.ToString());
            
            if (firstOperand != null) firstOperand.ErrorCheck(Context, Ast);
            if (secondOperand != null) secondOperand.ErrorCheck(Context, Ast);
        }

        public Operand operand(int n) { if (n == 0) return firstOperand; else return secondOperand; }

        public override void Emit(EmissionStream stream)
        {
            if (instruction == Instructions.HLT)
                stream.WriteLine(instruction.ToString());
            else if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                stream.WriteLine(instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine(instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public override string ToString()
        {
            if (instruction == Instructions.HLT)
                return instruction.ToString();
            else if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                return instruction.ToString() + " " + firstOperand;
            else
                return instruction.ToString() + " " + firstOperand + ", " + secondOperand;
        }

        public override void EmitIR(EmissionStream stream, bool Tidy)
        {
            if (instruction == Instructions.HLT)
                stream.WriteLine((Tidy ? "" : "[i /] ") + "HLT");
            else if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                stream.WriteLine((Tidy ? "" : "[i /] ") + instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine((Tidy ? "" : "[i /] ") + instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public static IRNode Make(Instructions instruction, Operand firstOperand, Operand secondOperand = null)
        {
            var r = new Instruction();
            r.instruction = instruction;
            r.firstOperand = firstOperand;
            r.secondOperand = secondOperand;
            return r;
        }

        public override int InstructionCount()
        {
            return 1;
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            var ins = new Box<ushort>{ data = 0 };
            binary.Add(ins);
            
            if (instruction == Instructions.HLT)
            {
                ins.data += (ushort)(((ushort)instruction - (ushort)Instructions.SINGLE_OPERAND_INSTRUCTIONS) << 5);
            }
            else if (instruction < Instructions.SINGLE_OPERAND_INSTRUCTIONS)
            {
                var A = EncodeOperand(secondOperand, OperandUsage.A);
                if (A.Item2 != null) binary.Add(A.Item2);
                ins.data += (ushort)(A.Item1 << 10);

                var B = EncodeOperand(firstOperand, OperandUsage.B);
                if (B.Item2 != null) binary.Add(B.Item2);
                ins.data += (ushort)(B.Item1 << 5);

                ins.data += (ushort)instruction;
            }
            else
            {
                var A = EncodeOperand(firstOperand, OperandUsage.A);
                if (A.Item2 != null) binary.Add(A.Item2);
                ins.data += (ushort)(A.Item1 << 10);

                ins.data += (ushort)(((ushort)instruction - (ushort)Instructions.SINGLE_OPERAND_INSTRUCTIONS) << 5);
            }
        }

        public override void SetupLabels(Dictionary<string, Label> labelTable)
        {
            if ((firstOperand.semantics & OperandSemantics.Label) == OperandSemantics.Label)
                firstOperand.label = labelTable[firstOperand.label.rawLabel];
            if (secondOperand != null && (secondOperand.semantics & OperandSemantics.Label) == OperandSemantics.Label)
                secondOperand.label = labelTable[secondOperand.label.rawLabel];
        }

        public override void MarkUsedRealRegisters(bool[] bank)
        {
            base.MarkUsedRealRegisters(bank);

            firstOperand.MarkRegisters(bank);
            if (secondOperand != null) secondOperand.MarkRegisters(bank);
        }

        public override void CorrectVariableOffsets(int delta)
        {
            firstOperand.AdjustVariableOffsets(delta);
            if (secondOperand != null) secondOperand.AdjustVariableOffsets(delta);
        }
    }
}
