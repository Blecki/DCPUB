using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public class ConfigurationError : Exception
    {
        public ConfigurationError(String msg) : base(msg) { }
    }
}
