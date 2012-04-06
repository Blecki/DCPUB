using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class NumberLiteralNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();
        }

        public override void Compile(List<String> assembly, Scope scope) 
        {
            if (AsString.StartsWith("0x"))
            {
                var hexPart = AsString.Substring(2).ToUpper();
                while (hexPart.Length < 4) hexPart = "0" + hexPart;
                assembly.Add("SET PUSH, 0x" + hexPart);
            }
            else
                assembly.Add("SET PUSH, " + hex(AsString));
            scope.stackDepth += 1;
        }
    }

    
}
