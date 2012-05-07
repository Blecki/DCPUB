using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUCCL
{
    public class FileEmissionStream : DCPUC.Assembly.EmissionStream
    {
        public System.IO.StreamWriter stream = null;

        public FileEmissionStream(System.IO.StreamWriter stream)
        {
            this.stream = stream;
        }

        public override void WriteLine(string line)
        {
            stream.WriteLine(line);
        }
    }
}
