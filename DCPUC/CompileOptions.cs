using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class CompileOptions
    {
        public string @in = null;
        public string @out = null;
        public bool binary = false;
        public bool externals = false;
        public string peephole = null;
        public bool be = false;
    }
}
