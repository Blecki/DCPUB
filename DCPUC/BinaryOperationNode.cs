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

        public override string TreeLabel()
        {
            return AsString + " " + ResultType;
        }

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

                opcodes.Add("+signed", "ADD");
                opcodes.Add("-signed", "SUB");
                opcodes.Add("*signed", "MLI");
                opcodes.Add("/signed", "DVI");

                opcodes.Add("%", "MOD");
                opcodes.Add("<<", "SHL");
                opcodes.Add(">>", "SHR");
                opcodes.Add("&", "AND");
                opcodes.Add("|", "BOR");
                opcodes.Add("^", "XOR");
            }

        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);

            if (Child(0).ResultType != Child(1).ResultType)
            {
                context.AddWarning(this.Span, "Conversion between types. Possible loss of data.");
                //Promote to signed?

                if (AsString == "+" || AsString == "-" || AsString == "*" || AsString == "/") ResultType = "signed";
                else ResultType = "unsigned";
            }
            else
                ResultType = Child(0).ResultType;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            var first = Child(0).FoldConstants(context);
            var second = Child(1).FoldConstants(context);

            

            if (first.IsIntegralConstant() && second.IsIntegralConstant())
            {
                var a = first.GetConstantValue();
                var b = second.GetConstantValue();
                var promote = first.ResultType == "signed" || second.ResultType == "signed";

                if (AsString == "+") if (promote) a = (int)((short)a + (short)b); else a = (int)((ushort)a + (ushort)b);
                if (AsString == "-") if (promote) a = (int)((short)a - (short)b); else a = (int)((ushort)a - (ushort)b);
                if (AsString == "*") if (promote) a = (int)((short)a * (short)b); else a = (int)((ushort)a * (ushort)b);
                if (AsString == "/") if (promote) a = (int)((short)a / (short)b); else a = (int)((ushort)a / (ushort)b);
                if (AsString == "%") a = (int)((ushort)a % (ushort)b);
                if (AsString == "<<") a = (int)((ushort)a << (ushort)b);
                if (AsString == ">>") a = (int)((ushort)a >> (ushort)b);
                if (AsString == "&") a = (int)((ushort)a & (ushort)b);
                if (AsString == "|") a = (int)((ushort)a | (ushort)b);
                if (AsString == "^") a = (int)((ushort)a ^ (ushort)b);

                return new NumberLiteralNode { Value = a, WasFolded = true, ResultType = ResultType };
            }

            return this;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (!Child(1).IsIntegralConstant())
            {
                secondOperandResult = parentState.FindAndUseFreeRegister();
                Child(1).AssignRegisters(context, parentState, secondOperandResult);
            }

            firstOperandResult = target;

            if (!Child(0).IsIntegralConstant())
                Child(0).AssignRegisters(context, parentState, firstOperandResult);

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
                        context.Add(ResultType == "signed" ? opcodes[AsString+"signed"] : opcodes[AsString], "PEEK", Scope.TempRegister);
                        scope.stackDepth -= 1;
                    }
                    else
                        context.Add(ResultType == "signed" ? opcodes[AsString + "signed"] : opcodes[AsString], "PEEK", Scope.GetRegisterLabelSecond((int)secondOperandResult));
                }
                else
                {
                    context.Add(ResultType == "signed" ? opcodes[AsString + "signed"] : opcodes[AsString],
                        Scope.GetRegisterLabelFirst((int)firstOperandResult),
                        Scope.GetRegisterLabelSecond((int)secondOperandResult));
                    if (secondOperandResult == Register.STACK) scope.stackDepth -= 1;
                }
            }
            else
            {
                if (firstOperandResult == Register.STACK)
                    context.Add(ResultType == "signed" ? opcodes[AsString + "signed"] : opcodes[AsString], "PEEK", Child(1).GetConstantToken());
                else
                    context.Add(ResultType == "signed" ? opcodes[AsString + "signed"] : opcodes[AsString], Scope.GetRegisterLabelFirst((int)firstOperandResult),
                        Child(1).GetConstantToken());
            }
        }
    }
}
