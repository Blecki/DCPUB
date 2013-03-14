using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public class CompileOptions
    {
        public string @in = null;
        public string @out = null;
        public bool binary = false;
        public bool externals = false;
        public string peephole = null;
        public bool be = false;
        public bool skip_virtual_register_assignment = false;
    }
}
