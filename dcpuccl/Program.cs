using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUCCL
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DCPUC.CompileOptions();
            DCPUC.CompileOptions.ParseCommandLine(args, options, (s) => { Console.WriteLine(s); });
            Console.WriteLine("Done.");
        }
    }
}
