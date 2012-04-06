using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();

            if (opcodes == null)
            {
                opcodes = new Dictionary<string, string>();
                opcodes.Add("+", "ADD");
                opcodes.Add("-", "SUB");
                opcodes.Add("*", "MUL");
                opcodes.Add("/", "DIV");
                opcodes.Add("%", "MOD");
                opcodes.Add("<<", "SHL");
                opcodes.Add(">>", "SHR");
                opcodes.Add("&", "AND");
                opcodes.Add("|", "BOR");
                opcodes.Add("^", "XOR");
            }
        }

        public override void Compile(List<String> assembly, Scope scope, Register target) 
        {
            (ChildNodes[1] as CompilableNode).Compile(assembly, scope, Register.STACK);
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope, Register.STACK);

            if (AsString == "==")
            {
                assembly.Add("SET A, 0x0");
                assembly.Add("IFE POP, POP");
                assembly.Add("SET A, 0x1");
            }
            else if (AsString == "!=")
            {
                assembly.Add("SET A, 0x0");
                assembly.Add("IFN POP, POP");
                assembly.Add("SET A, 0x1");
            }
            else
            {
                assembly.Add("SET A, POP");
                assembly.Add(opcodes[AsString] + " A, POP");
            }
            assembly.Add("SET PUSH, A");
            scope.stackDepth -= 1;
        }
    }

    
}
