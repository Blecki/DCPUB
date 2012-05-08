using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Inline : Node
    {
        public String code;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(code);
        }
    }
}
