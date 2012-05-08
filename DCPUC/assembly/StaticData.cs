using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class StaticData : Node
    {
        public String label;
        public List<string> data;

        public override void Emit(EmissionStream stream)
        {
            var str = ":" + label + " DAT " + String.Join(" ", data);
            stream.WriteLine(str);
        }
    }
}
