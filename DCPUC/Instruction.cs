using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class Instruction
    {
        public string ins;
        public string a;
        public string b;
        public string comment;

        public override string ToString()
        {
            if (String.IsNullOrEmpty(a)) return ins;
            else if (String.IsNullOrEmpty(b)) return ins + " " + a;
            else return (ins[0] == ';' ? "" : "   ") + ins + " " + a + (a != "DAT" ? ", " : " ") + b;
        }
    }

}
