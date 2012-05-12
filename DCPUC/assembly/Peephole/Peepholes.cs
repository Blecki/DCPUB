using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly.Peephole
{
    public class Peepholes
    {
        public static void InitializePeepholes()
        {
            var Parser = new Irony.Parsing.Parser(new Grammar());
            var defs = System.IO.File.ReadAllText("Assembly/Peephole/peepholedef.txt");
            var program = Parser.Parse(defs);
        }
    }
}
