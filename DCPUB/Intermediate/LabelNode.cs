﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    public class LabelNode : IRNode
    {
        public Label label;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(new String(' ', stream.indentDepth * 3) + ":" + label);
        }

        public override void EmitIR(EmissionStream stream, bool Tidy)
        {
            stream.WriteLine((Tidy ? "" : "[l /] ") + ":" + label);
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            label.position.data = (ushort)binary.Count;
        }
    }
}
