using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public class MixedStaticData : Node
    {
        public Assembly.Label label;
        public List<Operand> Data;

        public override void Emit(EmissionStream stream)
        {
            var str = ":" + label + " DAT ";
            foreach (var item in Data)
            {
                if (item.IsIntegralConstant()) str += string.Format("0x{0:X4}", item.constant);
                else if (item.semantics == OperandSemantics.Label) str += item.label;
                else throw new InternalError("Incorrect operand in mixed static data");
                str += " ";
            }
            stream.WriteLine(str);
        }

        public override void EmitIR(EmissionStream stream)
        {
            Emit(stream);
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            label.position.data = (ushort)binary.Count;
            foreach (var item in Data)
            {
                if (item.IsIntegralConstant()) binary.Add(new Box<ushort> { data = item.constant });
                else if (item.semantics == OperandSemantics.Label) binary.Add(item.label.position);
                else throw new InternalError("Incorrect operand in mixed static data");
            }
        }
    }
}
