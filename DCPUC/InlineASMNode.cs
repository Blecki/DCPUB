using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class InlineASMNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            var lines = AsString.Split(new String[2]{"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
            assembly.Barrier();
            foreach (var str in lines)
                assembly.Add(str + " ;", "", "");
        }
    }

    
}
