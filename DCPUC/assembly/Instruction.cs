using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Instruction : Node
    {
        public Instructions instruction;
        public String firstOperand;
        public String secondOperand;

        public override void Emit(EmissionStream stream)
        {
            if (instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                stream.WriteLine(new String(' ', stream.indentDepth * 3) + instruction.ToString() + " " + firstOperand);
            else
                stream.WriteLine(new String(' ', stream.indentDepth * 3) + instruction.ToString() + " " + firstOperand + ", " + secondOperand);
        }

        public static Node Make(Instructions instruction, String firstOperand, String secondOperand = null)
        {
            var r = new Instruction();
            r.instruction = instruction;
            r.firstOperand = firstOperand;
            r.secondOperand = secondOperand;
            return r;
        }
    }
}
