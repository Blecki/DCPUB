using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class InlineASMNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            var lines = AsString.Split(new String[2]{"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
            int stackChange = 0;
            foreach (var str in lines)
            {
                if (str.ToUpper().Contains("PUSH")) stackChange += 1;
                if (str.ToUpper().Contains("POP")) stackChange -= 1;
                assembly.Add(str);
            }
            scope.stackDepth += stackChange;
        }
    }

    
}
