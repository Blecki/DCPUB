using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public class Instruction : Node
    {
        public Instructions instruction;
        public Operand firstOperand;
        public Operand secondOperand;

        public Operand operand(int n) { if (n == 0) return firstOperand; else return secondOperand; }

        public override void Emit(EmissionStream stream)
        {
            if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                stream.WriteLine(instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine(instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public override void EmitIR(EmissionStream stream)
        {
            if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                stream.WriteLine("[i /] " + instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine("[i /] " + instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public static Node Make(Instructions instruction, Operand firstOperand, Operand secondOperand = null)
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
            
            if (instruction < Instructions.SINGLE_OPERAND_INSTRUCTIONS)
            {
                var A = DCPU.EncodeOperand(secondOperand, OperandUsage.A);
                if (A.Item2 != null) binary.Add(A.Item2);
                ins.data += (ushort)(A.Item1 << 10);

                var B = DCPU.EncodeOperand(firstOperand, OperandUsage.B);
                if (B.Item2 != null) binary.Add(B.Item2);
                ins.data += (ushort)(B.Item1 << 5);

                ins.data += (ushort)instruction;
            }
            else
            {
                var A = DCPU.EncodeOperand(firstOperand, OperandUsage.A);
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

        //public override void AssignRegisters(Dictionary<ushort, OperandRegister> mapping)
        //{
        //    firstOperand.AssignRegisters(mapping);
        //    if (secondOperand != null) secondOperand.AssignRegisters(mapping);
        //}
    }
}
