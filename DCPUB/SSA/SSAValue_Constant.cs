using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSAConstantValue : SSAValue
    {
        public ushort Value;

        public SSAConstantValue(ushort Value)
        {
            this.Value = Value;
        }

        public override string ToString()
        {
            return String.Format("{0:X4}", Value);
        }
    }
}
