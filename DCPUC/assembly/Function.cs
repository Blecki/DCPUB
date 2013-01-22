using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Function : Node
    {
        public String functionName;
        public Assembly.Label entranceLabel;
        public int parameterCount;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(";DCPUC FUNCTION " + functionName + " " + entranceLabel + " " + parameterCount);
            base.Emit(stream);
            stream.WriteLine(";END FUNCTION");
            stream.WriteLine("");
        }
    }
}
