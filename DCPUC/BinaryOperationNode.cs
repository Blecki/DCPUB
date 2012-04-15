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
        public Register firstOperandResult = Register.STACK;
        public Register secondOperandResult = Register.STACK;

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

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            if (!Child(1).IsIntegralConstant())
            {
                secondOperandResult = parentState.FindAndUseFreeRegister();
                Child(1).AssignRegisters(parentState, secondOperandResult);
            }

            firstOperandResult = target;

            if (!Child(0).IsIntegralConstant())
                Child(0).AssignRegisters(parentState, firstOperandResult);

            parentState.FreeRegisters(secondOperandResult);
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            if (Child(0).IsIntegralConstant())
            {
                context.Add("SET", Scope.GetRegisterLabelFirst((int)firstOperandResult), Child(0).GetConstantToken());
                if (firstOperandResult == Register.STACK) scope.stackDepth += 1;
            }
            else
                Child(0).Emit(context, scope);

            if (!Child(1).IsIntegralConstant())
            {
                Child(1).Emit(context, scope);

                if (firstOperandResult == Register.STACK)
                {
                    if (secondOperandResult == Register.STACK)
                    {
                        context.Add("SET", Scope.TempRegister, "POP");
                        context.Add(opcodes[AsString], "PEEK", Scope.TempRegister);
                        scope.stackDepth -= 1;
                    }
                    else
                        context.Add(opcodes[AsString], "PEEK", Scope.GetRegisterLabelSecond((int)secondOperandResult));
                }
                else
                {
                    context.Add(opcodes[AsString],
                        Scope.GetRegisterLabelFirst((int)firstOperandResult),
                        Scope.GetRegisterLabelSecond((int)secondOperandResult));
                    if (secondOperandResult == Register.STACK) scope.stackDepth -= 1;
                }
            }
            else
            {
                if (firstOperandResult == Register.STACK)
                    context.Add(opcodes[AsString], "PEEK", Child(1).GetConstantToken());
                else
                    context.Add(opcodes[AsString], Scope.GetRegisterLabelFirst((int)firstOperandResult),
                        Child(1).GetConstantToken());
            }
        }
    }
}
