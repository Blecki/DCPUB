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
                        

            if (AsString == "==" || AsString == "!=")
            {
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, Register.STACK);
                if (target == Register.STACK)
                {
                    assembly.Add("SET", "A", "0x0", "Equality onto stack");
                    assembly.Add((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    assembly.Add("SET", "A", "0x1");
                    assembly.Add("SET", "PUSH", "A");
                    //scope.stackDepth += 1;
                   
                }
                else
                {
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "0x0",  "Equality into register");
                    assembly.Add((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "0x1");
                }
            }
            
            else
            {
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, target);
                if (target == Register.STACK)
                {
                    assembly.Add("SET", "A", "POP", "Binary operation onto stack");
                    assembly.Add(opcodes[AsString], "A", Scope.GetRegisterLabelSecond(secondTarget));
                    assembly.Add("SET", "PUSH", "A");
                }
                else
                {
                    assembly.Add(opcodes[AsString], Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond(secondTarget), "Binary operation into register");
                }
            }
            if (secondTarget == (int)Register.STACK)
                scope.stackDepth -= 1;
            else
                scope.FreeRegister(secondTarget);
        }
    }

    
}
