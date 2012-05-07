using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Label : Node
    {
        public String label;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(new String(' ', stream.indentDepth * 3) + ":" + label);
        }
    }
}
