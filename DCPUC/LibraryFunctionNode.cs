using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class LibraryFunctionNode : FunctionDeclarationNode
    {
        public List<string> code;

        public override void CompileFunction(Assembly assembly, Scope topscope)
        {
            //if (references == 0) return;
            foreach (var line in code)
            {
                assembly.Barrier();
                assembly.Add(line, "", "");
            }
        }

    }
}
