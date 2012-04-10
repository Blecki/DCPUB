using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BinaryOperationNode : CompilableNode
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

        public override bool IsConstant()
        {
            return (ChildNodes[0] as CompilableNode).IsIntegralConstant() && (ChildNodes[1] as CompilableNode).IsIntegralConstant();
        }

        public override ushort GetConstantValue()
        {
            var a = (ChildNodes[0] as CompilableNode).GetConstantValue();
            var b = (ChildNodes[1] as CompilableNode).GetConstantValue();

            if (AsString == "+") return (ushort)(a + b);
            if (AsString == "-") return (ushort)(a - b);
            if (AsString == "*") return (ushort)(a * b);
            if (AsString == "/") return (ushort)(a / b);
            if (AsString == "%") return (ushort)(a % b);
            if (AsString == "<<") return (ushort)(a << b);
            if (AsString == ">>") return (ushort)(a >> b);
            if (AsString == "&") return (ushort)(a & b);
            if (AsString == "|") return (ushort)(a | b);
            if (AsString == "^") return (ushort)(a ^ b);
            return 0;
        }

        public override string GetConstantToken()
        {
            return hex(GetConstantValue());
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            int secondTarget = (int)Register.STACK;

            var secondConstant = (ChildNodes[1] as CompilableNode).IsConstant();
            var firstConstant = (ChildNodes[0] as CompilableNode).IsConstant();

            if (firstConstant && secondConstant)
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex(GetConstantValue()));
                if (target == Register.STACK) scope.stackDepth += 1;
                return;
            }

            if (!secondConstant)
            {
                secondTarget = scope.FindAndUseFreeRegister();
                (ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)secondTarget);
            }

            if (!firstConstant)
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, target);

            if (target == Register.STACK)
            {
                assembly.Add("SET", Scope.TempRegister,
                     firstConstant ? hex((ChildNodes[0] as CompilableNode).GetConstantValue()) : "POP");
                assembly.Add(opcodes[AsString], Scope.TempRegister,
                    secondConstant ? hex((ChildNodes[1] as CompilableNode).GetConstantValue()) : Scope.GetRegisterLabelSecond(secondTarget));
                assembly.Add("SET", "PUSH", Scope.TempRegister);
            }
            else
            {
                if (firstConstant)
                    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex((ChildNodes[0] as CompilableNode).GetConstantValue()));
                assembly.Add(opcodes[AsString], Scope.GetRegisterLabelFirst((int)target),
                    secondConstant ? hex((ChildNodes[1] as CompilableNode).GetConstantValue()) : Scope.GetRegisterLabelSecond(secondTarget));
            }
             

            if (secondTarget == (int)Register.STACK && !secondConstant)
                scope.stackDepth -= 1;
            else
                scope.FreeMaybeRegister(secondTarget);
        }
    }

    
}
