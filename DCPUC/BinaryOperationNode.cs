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

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            //Evaluate in reverse in case both need to go on the stack
            var secondTarget = scope.FindFreeRegister();
            if (Scope.IsRegister((Register)secondTarget)) scope.UseRegister(secondTarget);
            (ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)secondTarget);


            (ChildNodes[0] as CompilableNode).Compile(assembly, scope, target);
            if (target == Register.STACK)
            {
                assembly.Add("SET", Scope.TempRegister, "POP", "Binary operation onto stack");
                assembly.Add(opcodes[AsString], Scope.TempRegister, Scope.GetRegisterLabelSecond(secondTarget));
                assembly.Add("SET", "PUSH", Scope.TempRegister);
            }
            else
            {
                assembly.Add(opcodes[AsString], Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond(secondTarget), "Binary operation into register");
            }

            if (secondTarget == (int)Register.STACK)
                scope.stackDepth -= 1;
            else
                scope.FreeRegister(secondTarget);
        }
    }

    
}
