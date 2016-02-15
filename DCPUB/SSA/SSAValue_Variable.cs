using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSAVariableValue : SSAValue
    {
        public Operand AccessOperand;

        public SSAVariableValue(Operand AccessOperand)
        {
            this.AccessOperand = AccessOperand;
        }

        public override string ToString()
        {
            return "<" + AccessOperand.ToString() + ">";
        }
    }
}
