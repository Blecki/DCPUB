using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class CompileError : Exception
    {
        public CompileError(String msg) : base(msg) { }
    }
}
