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
        public virtual bool IsConstant() { return false; }
        public virtual ushort GetConstantValue() { return 0; }
        public virtual string GetConstantToken() { return "0x0000"; }
        public virtual bool IsIntegralConstant() { return false; }
        public virtual string TreeLabel() { return AsString; }

        public CompilableNode Child(int n) { return ChildNodes[n] as CompilableNode; }

        public virtual AstNode FoldConstants()
        {
            var childrenCopy = new AstNodeList();
            foreach (var child in ChildNodes)
            {
                var nChild = (child as CompilableNode).FoldConstants();
                if (nChild != null) childrenCopy.Add(nChild);
            }
            ChildNodes.Clear();
            ChildNodes.AddRange(childrenCopy);
            return this;
        }

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

        public static void EndBlock(Assembly assembly, Scope scope)
        {
            if (scope.stackDepth - scope.parentDepth > 0) 
                assembly.Add("ADD", "SP", hex(scope.stackDepth - scope.parentDepth), "End block");
        }

        public void InsertLibrary(List<string> library)
        {
            for (int i = 0; i < library.Count; ++i)
            {
                if (library[i].StartsWith(";DCPUC FUNCTION"))
                {
                    //Parse function..
                    var funcHeader = library[i].Split(' ');
                    List<String> funcCode = new List<string>();
                    while (i < library.Count && !library[i].StartsWith(";DCPUC END"))
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    if (i < library.Count)
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    var funcNode = new LibraryFunctionNode();
                    funcNode.AsString = funcHeader[2];
                    funcNode.label = funcHeader[3];
                    funcNode.parameterCount = Convert.ToInt32(funcHeader[4]);
                    funcNode.code = funcCode;
                    ChildNodes.Add(funcNode);
                }
                else if (library[i].StartsWith(";DCPUC STATIC"))
                {
                    List<String> funcCode = new List<string>();
                    while (i < library.Count && !library[i].StartsWith(";DCPUC END"))
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    if (i < library.Count)
                    {
                        funcCode.Add(library[i]);
                        ++i;
                    }
                    var funcNode = new LibraryFunctionNode();
                    funcNode.AsString = "%STATICDATA%";
                    funcNode.code = funcCode;
                    ChildNodes.Add(funcNode);
                }
            }
        }
    }
}
