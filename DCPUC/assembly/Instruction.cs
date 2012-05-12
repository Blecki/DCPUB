using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
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
                stream.WriteLine(new String(' ', stream.indentDepth * 3) + instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine(new String(' ', stream.indentDepth * 3) + instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public static Node Make(Instructions instruction, Operand firstOperand, Operand secondOperand = null)
        {
            var r = new Instruction();
            r.instruction = instruction;
            r.firstOperand = firstOperand;
            r.secondOperand = secondOperand;
            return r;
        }
    }
}
