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
            else return ins + " " + a + (a != "DAT" ? ", " : " ") + b;
            //if (ins[0] == ':' || ins == "BRK") return ins;// + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            //else if (ins == "JSR") return ins + " " + a;// + (String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
            //else return ins + " " + a + ", " + b;// +(String.IsNullOrEmpty(comment) ? "" : (" ;" + comment));
        }
    }

    public class Assembly
    {
        public List<Instruction> instructions = new List<Instruction>();
        public int _barrier = 0;

        public void Add(string ins, string a, string b, string comment = null)
        {
            var instruction = new Instruction();
            instruction.ins = ins;
            instruction.a = a;
            instruction.b = b;
            instruction.comment = comment;

            //instructions.Add(instruction);
            //return;

            if (instructions.Count > _barrier)
            {
                bool ignore = false;
                var lastIns = instructions[instructions.Count - 1];

                    //SET A, POP
                    //SET PUSH, A
                if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == "POP" && instruction.a == "PUSH" && instruction.b == lastIns.a)
                {
                    instructions.RemoveAt(instructions.Count - 1);
                    ignore = true;
                }
                    //SET A, !POP
                    //SET !PUSH, A
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.a == instruction.b && lastIns.b != "POP" && instruction.a != "PUSH")
                {
                    lastIns.a = instruction.a;
                    ignore = true;
                } 
                    //SET PUSH, A
                    //SET A, POP
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == instruction.a && lastIns.a == "PUSH" && instruction.b == "pop")
                {
                    instructions.RemoveAt(instructions.Count - 1);
                    ignore = true;
                }
                    //SET PUSH, A
                    //SET A, PEEK
                else if (lastIns.ins == "SET" && instruction.ins == "SET" && lastIns.b == instruction.a && lastIns.a == "PUSH" && instruction.b == "PEEK")
                {
                    ignore = true;
                }
                    //SET A, ?             -> IFN|IFE|IFG ?, A
                    //IFN|IFE|IFG ?, A
                else if (lastIns.ins == "SET" && (instruction.ins == "IFN" || instruction.ins == "IFE" || instruction.ins == "IFG")
                    && lastIns.a == instruction.b)
                {
                    lastIns.ins = instruction.ins;
                    lastIns.a = instruction.a;
                    ignore = true;
                }

                if (!ignore) instructions.Add(instruction);
            }
            else
                instructions.Add(instruction);
            
        }

        public void Barrier() { _barrier = instructions.Count; }

        
    }
}
