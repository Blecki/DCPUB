using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public class StaticData : Node
    {
        public Assembly.Label label;
        public List<ushort> data;

        public override void Emit(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + String.Join(" ", data.Select(u => string.Format("0x{0:X}", u)));
            stream.WriteLine(str);
        }

        public override void EmitIR(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + String.Join(" ", data.Select(u => string.Format("0x{0:X}", u)));
            stream.WriteLine(str);
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            label.position.data = (ushort)binary.Count;
            foreach (var u in data)
                binary.Add(new Box<ushort> { data = u });
        }
    }

    public class StaticLabelData : Node
    {
        public Assembly.Label label;
        public Assembly.Label data;

        public override void Emit(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + data;
            stream.WriteLine(str);
        }

        public override void EmitIR(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + data;
            stream.WriteLine(str);
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            label.position.data = (ushort)binary.Count;
            binary.Add(data.position);
        }
    }
}
