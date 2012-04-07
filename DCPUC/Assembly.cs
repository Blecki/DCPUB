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
            if (ins[0] == ':' || ins == "BRK") return ins + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            else if (ins == "JSR") return ins + " " + a + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            else return ins + " " + a + ", " + b + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
        }
    }

    public class Assembly
    {
        public List<Instruction> instructions = new List<Instruction>();
        public bool barrierFlag = false;

        public void Add(string ins, string a, string b, string comment = null)
        {
            var instruction = new Instruction();
            instruction.ins = ins;
            instruction.a = a;
            instruction.b = b;
            instruction.comment = comment;

            //if (barrierFlag)
            //{
            //    barrierFlag = false;
                instructions.Add(instruction);
            //}
            //else
            //{
            //    bool ignore = false;
            //    if (instructions.Count > 0)
            //    {
            //        var lastIns = instructions[instructions.Count - 1];
            //        if (lastIns.ins == "SET" && instruction.ins == "SET" && instruction.b == "POP")
            //        {
            //            if (lastIns.a == "PUSH")
            //            {
            //                lastIns.a = instruction.a;
            //                ignore = true;
            //            }
            //        }
            //        else if (lastIns.ins == "SET" && instruction.ins == "SET" 
            //            && instruction.b == lastIns.a && instruction.b != "PUSH")
            //        {
            //            lastIns.a = instruction.a;
            //            ignore = true;
            //        }
            //    }
            //    if (!ignore) instructions.Add(instruction);
            //}

        }

        public void Barrier() { barrierFlag = true; }

        
    }
}
