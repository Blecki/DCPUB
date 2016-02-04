using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public class InternalError : Exception
    {
        public InternalError(String msg) : base(msg) { }
        public InternalError(Exception InnerException) : base("Internal Error", InnerException) { }
    }
}
