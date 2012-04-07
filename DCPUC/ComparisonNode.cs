using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class ComparisonNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
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
                    assembly.Add("SET", Scope.TempRegister, "0x0", "Equality onto stack");
                    assembly.Add((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    assembly.Add("SET", Scope.TempRegister, "0x1");
                    assembly.Add("SET", "PUSH", Scope.TempRegister);
                }
                else
                {
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "0x0",  "Equality into register");
                    assembly.Add((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "0x1");
                }
            }
            
            if (secondTarget == (int)Register.STACK)
                scope.stackDepth -= 1;
            else
                scope.FreeRegister(secondTarget);
        }
    }

    
}
