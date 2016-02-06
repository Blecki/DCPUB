using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUCCL
{
    public class FileEmissionStream : DCPUB.EmissionStream
    {
        public System.IO.TextWriter stream = null;

        public FileEmissionStream(System.IO.TextWriter stream)
        {
            this.stream = stream;
        }

        public override void WriteLine(string line)
        {
            stream.WriteLine(new string(' ', indentDepth * 3) + line);
        }

    }
}
