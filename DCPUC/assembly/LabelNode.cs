using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class LabelNode : Node
    {
        public Label label;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(new String(' ', stream.indentDepth * 3) + ":" + label);
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            label.position.data = (ushort)binary.Count;
        }
    }
}
