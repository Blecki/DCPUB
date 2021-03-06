﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    public class Function : IRNode
    {
        public String functionName;
        public Intermediate.Label entranceLabel;
        public int parameterCount;

        public override void Emit(EmissionStream stream)
        {
            stream.WriteLine(";DCPUB FUNCTION " + functionName + " " + entranceLabel + " " + parameterCount);
            base.Emit(stream);
            stream.WriteLine(";END FUNCTION");
            stream.WriteLine("");
        }

        public override void EmitIR(EmissionStream stream, bool Tidy)
        {
            if (!Tidy) stream.WriteLine("[function node]");
            stream.WriteLine(";DCPUB FUNCTION " + functionName + " " + entranceLabel + " " + parameterCount);
            base.EmitIR(stream, Tidy);
            stream.WriteLine(";END FUNCTION");
            if (!Tidy) stream.WriteLine("[/function node]");
            stream.WriteLine("");
        }
    }
}
