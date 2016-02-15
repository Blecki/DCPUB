using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSADereferenceOffsetVirtualValue : SSAValue
    {
        public SSAVirtualValue Virtual;
        public ushort Offset;

        public SSADereferenceOffsetVirtualValue(SSAVirtualValue Virtual, ushort Offset)
        {
            this.Virtual = Virtual;
            this.Offset = Offset;
        }

        public override string ToString()
        {
            return String.Format("[{0:X4}+VR{1}]", Offset, Virtual.VirtualIndex);
        }
    }
}
