using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class NumberLiteralNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();
        }

        public override bool IsConstant()
        {
            return true;
        }

        public override ushort GetConstantValue()
        {
            if (AsString.StartsWith("0x"))
                return atoh(AsString.Substring(2));
            else
                return Convert.ToUInt16(AsString);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target) 
        {
            if (AsString.StartsWith("0x"))
            {
                var hexPart = AsString.Substring(2).ToUpper();
                while (hexPart.Length < 4) hexPart = "0" + hexPart;
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "0x" + hexPart, "Literal");
            }
            else
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex(AsString), "Literal");
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }

    
}
