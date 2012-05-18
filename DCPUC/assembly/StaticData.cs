using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class StaticData : Node
    {
        public Assembly.Label label;
        public List<string> data;

        public override void Emit(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + String.Join(" ", data);
            stream.WriteLine(str);
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
    }
}
