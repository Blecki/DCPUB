using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSAValue
    {
        public override string ToString()
        {
            throw new InternalError("Generic SSAValue should not exist in tree.");
        }
    }
}
