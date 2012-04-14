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

        public override CompilableNode FoldConstants()
        {
            var first = Child(0).FoldConstants();
            var second = Child(1).FoldConstants();

            if (first.IsIntegralConstant() && second.IsIntegralConstant())
            {
                var a = first.GetConstantValue();
                var b = second.GetConstantValue();

                if (AsString == "+") a = (ushort)(a + b);
                if (AsString == "-") a = (ushort)(a - b);
                if (AsString == "*") a = (ushort)(a * b);
                if (AsString == "/") a = (ushort)(a / b);
                if (AsString == "%") a = (ushort)(a % b);
                if (AsString == "<<") a = (ushort)(a << b);
                if (AsString == ">>") a = (ushort)(a >> b);
                if (AsString == "&") a = (ushort)(a & b);
                if (AsString == "|") a = (ushort)(a | b);
                if (AsString == "^") a = (ushort)(a ^ b);

                return new NumberLiteralNode { Value = a, WasFolded = true };
            }

            return this;
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
            return Hex.hex(GetConstantValue());
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            int secondTarget = (int)Register.STACK;

            var secondConstant = (ChildNodes[1] as CompilableNode).IsIntegralConstant();
            var firstConstant = (ChildNodes[0] as CompilableNode).IsIntegralConstant();

            if (firstConstant && secondConstant)
            {
                throw new CompileError("Constant binary operation was not folded");
            }
                     
            if (!secondConstant) 
            {
                secondTarget = scope.FindAndUseFreeRegister();
                Child(1).Compile(assembly, scope, (Register)secondTarget);
            }

           if (!firstConstant) Child(0).Compile(assembly, scope, target);


           if (target == Register.STACK)
           {
               if (firstConstant)
                   assembly.Add("SET", Scope.TempRegister, Child(0).GetConstantToken());
               else
                   assembly.Add("SET", Scope.TempRegister, "POP");
               if (secondConstant)
                   assembly.Add(opcodes[AsString], Scope.TempRegister, Child(1).GetConstantToken());
               else
                   assembly.Add(opcodes[AsString], Scope.TempRegister, Scope.GetRegisterLabelSecond(secondTarget));
               assembly.Add("SET", "PUSH", Scope.TempRegister);
           }

           else
           {

               if (firstConstant) assembly.Add("SET", "PUSH", Child(0).GetConstantToken());
               assembly.Add(opcodes[AsString], Scope.GetRegisterLabelFirst((int)target), secondConstant ? Child(1).GetConstantToken() :
                   Scope.GetRegisterLabelSecond(secondTarget));
           }
            
            if (secondTarget == (int)Register.STACK && !secondConstant)
                scope.stackDepth -= 1;
            else
                scope.FreeMaybeRegister(secondTarget);
        }
    }

    
}
