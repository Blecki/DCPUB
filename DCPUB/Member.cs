using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class Member
    {
        public String name;
        public String typeSpecifier;
        public Struct referencedStruct;
        public int offset;
        public int size;
        public bool isArray;
    }
}
