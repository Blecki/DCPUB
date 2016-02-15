using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSAVirtualValue : SSAValue
    {
        public ushort VirtualIndex;
        public ushort Version;

        public SSAVirtualValue(ushort VirtualIndex)
        {
            this.VirtualIndex = VirtualIndex;
            this.Version = 0;
        }

        public SSAVirtualValue NewVersion()
        {
            var r = new SSAVirtualValue(VirtualIndex);
            r.Version = (ushort)(Version + 1);
            return r;
        }

        public override string ToString()
        {
            return "VR" + VirtualIndex + "(" + Version + ")";
        }
    }
}
