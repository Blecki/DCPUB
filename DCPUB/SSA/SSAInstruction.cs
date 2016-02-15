using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSAInstruction
    {
        public Instructions Instruction;
        public SSAValue[] Operands = new SSAValue[2];

        public SSAInstruction(Instructions Instruction, SSAValue Op0, SSAValue Op1)
        {
            this.Instruction = Instruction;
            Operands[0] = Op0;
            Operands[1] = Op1;
        }

        public override string ToString()
        {
            var r = Instruction.ToString() + " ";
            if (Operands[0] != null) r += Operands[0].ToString();
            else r += "NULL";
            r += ", ";
            if (Operands[1] != null) r += Operands[1].ToString();
            else r += "NULL";
            return r;
        }
    }
}
