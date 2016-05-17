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
        public bool preprocess = true;
        public bool emit_ir = false;
        public bool strip = false;
        public bool tidy = false;

        public bool collapse_statements = false;
        public bool ssa = false;

        public bool tidy_ir = false;
    }
}
