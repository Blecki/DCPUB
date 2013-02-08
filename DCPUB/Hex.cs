using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class Hex
    {
        private static string hexDigits = "0123456789ABCDEF";
        public static String htoa(int x)
        {
            return x.ToString("X");
        }

        public static ushort atoh(string s)
        {
            return ushort.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }

        public static string btoa(ushort b)
        {
            var s = "";
            for (int i = 0; i < 16; ++i)
            {
                s = (char)('0' + (b % 2)) + s;
                b >>= 1;
            }
            return s;
        }

        public static ushort atob(string s)
        {
            ushort a = 0;
            foreach (var c in s)
            {
                a *= 2;
                a += (ushort)(c - '0');
            }
            return a;
        }

        public static String hex(int x) { return "0x" + htoa((ushort)x); }
        public static String hex(string x) { return "0x" + htoa((ushort)Convert.ToInt16(x)); }
    }
}
