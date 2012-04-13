using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class Hex
    {
        private static string hexDigits = "0123456789ABCDEF";
        public static String htoa(int x)
        {
            var s = "";
            while (x > 0)
            {
                s = hexDigits[x % 16] + s;
                x /= 16;
            }
            while (s.Length < 4) s = '0' + s;
            return s;
        }

        public static ushort atoh(string s)
        {
            ushort h = 0;
            s = s.ToUpper();
            for (int i = 0; i < s.Length; ++i)
            {
                h <<= 4;
                ushort d = (ushort)hexDigits.IndexOf(s[i]);
                h += d;
            }
            return h;
        }

        public static String hex(int x) { return "0x" + htoa((ushort)x); }
        public static String hex(string x) { return "0x" + htoa((ushort)Convert.ToInt16(x)); }

        public static Scope BeginBlock(Scope scope)
        {
            return scope.Push(new Scope());
        }

    }
}
