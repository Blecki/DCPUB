using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class EmissionStream
    {
        public int indentDepth = 0;

        public virtual void WriteLine(String line) { }
    }
}
