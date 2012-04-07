using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class CompilableNode : AstNode
    {
        public virtual void Compile(Assembly assembly, Scope scope, Register target) { throw new NotImplementedException(); }

        private static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
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

        public static String hex(int x) { return "0x" + htoa(x); }
        public static String hex(string x) { return "0x" + htoa(Convert.ToInt16(x)); }

        public static Scope BeginBlock(Scope scope)
        {
            return scope.Push(new Scope());
        }

        public static void EndBlock(Assembly assembly, Scope scope)
        {
            if (scope.stackDepth - scope.parentDepth > 0) 
                assembly.Add("ADD", "SP", hex(scope.stackDepth - scope.parentDepth), "End block");
        }
    }
}
