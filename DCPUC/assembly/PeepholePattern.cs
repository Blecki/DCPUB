using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DCPUC.Assembly
{
    using IIBF = Func<List<Instruction>, Boolean>;

    public class PeepholePattern
    {
        public static List<PeepholePattern> patterns;

        public static void InitializePeepholes()
        {
            if (patterns != null) return;
            patterns = new List<PeepholePattern>();

            patterns.Add(new PeepholePattern
            {
                Length = 2,
                Match = And(InstructionIs(0, Instructions.SET), InstructionIs(1, Instructions.SET),
                    EqualOperands(0, 0, 1, 1), EqualOperands(0, 1, 1, 0)),
                Replace = First
            });
        }


        public int Length = 2;
        public IIBF Match = null;
        public Func<List<Instruction>, List<Instruction>> Replace = null;

        private static T[] SubArray<T>(T[] array, int start, int length = -1)
        {
            if (length == -1) length = array.Length - start;
            var r = new T[length];
            for (int i = 0; i < length; ++i)
                r[i] = array[start + i];
            return r;
        }

        public static IIBF And(params IIBF[] f)
        {
            if (f.Length == 1) return f[0];
            var first = f[0];
            var second = And(SubArray(f,1));
            return new IIBF((_list) => { return first(_list) && second(_list); });
        }

        public static IIBF InstructionIs(int n, Instructions instruction)
        {
            return (_list) => { return _list[n].instruction == instruction; };
        }

        public static IIBF EqualOperands(int first, int firstOperand, int second, int secondOperand)
        {
            return (_list) => { return _list[first].operand(firstOperand) == _list[second].operand(secondOperand); };
        }

        public static List<Instruction> First(List<Instruction> ins)
        {
            return new List<Instruction>(new Instruction[] { ins[0] });
        }

    }
}
