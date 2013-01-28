using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, Assembly.Instructions> opcodes = null;
        public Register firstOperandResult = Register.STACK;
        public Register secondOperandResult = Register.STACK;

        public override string TreeLabel()
        {
            return AsString + " " + ResultType;
        }

        public void SetOp(string op) { AsString = op; }

        public void initOps()
        {
            if (opcodes == null)
            {
                opcodes = new Dictionary<string, Assembly.Instructions>();
                opcodes.Add("+", Assembly.Instructions.ADD);
                opcodes.Add("-", Assembly.Instructions.SUB);
                opcodes.Add("*", Assembly.Instructions.MUL);
                opcodes.Add("/", Assembly.Instructions.DIV);

                opcodes.Add("-*", Assembly.Instructions.MLI);
                opcodes.Add("-/", Assembly.Instructions.DVI);

                opcodes.Add("%", Assembly.Instructions.MOD);
                opcodes.Add("-%", Assembly.Instructions.MDI);
                opcodes.Add("<<", Assembly.Instructions.SHL);
                opcodes.Add(">>", Assembly.Instructions.SHR);
                opcodes.Add("&", Assembly.Instructions.AND);
                opcodes.Add("|", Assembly.Instructions.BOR);
                opcodes.Add("^", Assembly.Instructions.XOR);

                opcodes.Add("==", Assembly.Instructions.IFE);
                opcodes.Add("!=", Assembly.Instructions.IFN);
                opcodes.Add(">", Assembly.Instructions.IFG);
                opcodes.Add("<", Assembly.Instructions.IFL);
                opcodes.Add("->", Assembly.Instructions.IFA);
                opcodes.Add("-<", Assembly.Instructions.IFU);
            }
        }

        protected bool SkipInit = false;
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (SkipInit) return;
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            ResultType = "word";
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            var first = Child(0).FoldConstants(context);
            var second = Child(1).FoldConstants(context);

            if (first.IsIntegralConstant() && second.IsIntegralConstant())
            {
                var a = first.GetConstantValue();
                var b = second.GetConstantValue();

                if (AsString == "+") a = (int)((ushort)a + (ushort)b);
                if (AsString == "-") a = (int)((ushort)a - (ushort)b);
                if (AsString == "*") a = (int)((ushort)a * (ushort)b);

                if (AsString == "/")
                {
                    if (b == 0) throw new CompileError(second, "Division by zero in constant expression");
                     a = (int)((ushort)a / (ushort)b);
                }

                if (AsString == "%")
                {
                    if (b == 0) throw new CompileError(second, "Division by zero in constant expression");
                    a = (int)((ushort)a % (ushort)b);
                }

                if (AsString == "-*") a = (int)((short)a * (short)b);

                if (AsString == "-/")
                {
                    if (b == 0) throw new CompileError(second, "Division by zero in constant expression");
                    a = (int)((short)a / (short)b);
                }

                if (AsString == "-%")
                {
                    if (b == 0) throw new CompileError(second, "Division by zero in constant expression");
                    a = (int)((short)a % (short)b);
                }

                if (AsString == "<<") a = (int)((ushort)a << (ushort)b);
                if (AsString == ">>") a = (int)((ushort)a >> (ushort)b);
                if (AsString == "&") a = (int)((ushort)a & (ushort)b);
                if (AsString == "|") a = (int)((ushort)a | (ushort)b);
                if (AsString == "^") a = (int)((ushort)a ^ (ushort)b);

                if (AsString == "==") a = (ushort)(a == b ? 1 : 0);
                if (AsString == "!=") a = (ushort)(a != b ? 1 : 0);
                if (AsString == ">") a = (ushort)(a > b ? 1 : 0);
                if (AsString == "<") a = (ushort)(a < b ? 1 : 0);
                if (AsString == "->") a = (ushort)(a > b ? 1 : 0);
                if (AsString == "-<") a = (ushort)(a < b ? 1 : 0);

                return new NumberLiteralNode { Value = a, WasFolded = true, ResultType = ResultType, Span = Span };
            }

            return this;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            initOps();
            this.target = target;

             var opcode = opcodes[AsString];
             if (opcode >= Assembly.Instructions.IFB && opcode <= Assembly.Instructions.IFU)
             {
                 if (!Child(0).IsIntegralConstant())
                 {
                     if ((Child(0) is VariableNameNode) &&
                         ((Child(0) as VariableNameNode).variable.type != VariableType.External &&
                         !(Child(0) as VariableNameNode).variable.isArray))
                         return;
                     firstOperandResult = parentState.FindAndUseFreeRegister();
                     Child(0).AssignRegisters(context, parentState, firstOperandResult);
                 }

                 if (!Child(1).IsIntegralConstant())
                 {
                     if ((Child(1) is VariableNameNode) &&
                         ((Child(1) as VariableNameNode).variable.type != VariableType.External &&
                         !(Child(1) as VariableNameNode).variable.isArray))
                         return;
                     secondOperandResult = parentState.FindAndUseFreeRegister();
                     Child(1).AssignRegisters(context, parentState, secondOperandResult);
                 }

                 if (!Child(0).IsIntegralConstant()) parentState.FreeRegisters(firstOperandResult);
                 if (!Child(1).IsIntegralConstant()) parentState.FreeRegisters(secondOperandResult);
             }
             else
             {
                 firstOperandResult = target;

                 if (!Child(0).IsIntegralConstant())
                     Child(0).AssignRegisters(context, parentState, firstOperandResult);

                 if (!Child(1).IsIntegralConstant())
                 {
                     if ((Child(1) is VariableNameNode) &&
                         ((Child(1) as VariableNameNode).variable.type != VariableType.External &&
                         !(Child(1) as VariableNameNode).variable.isArray))
                         return;

                     secondOperandResult = parentState.FindAndUseFreeRegister();
                     Child(1).AssignRegisters(context, parentState, secondOperandResult);
                     parentState.FreeRegisters(secondOperandResult);
                 }
             }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {

            var r = new Assembly.ExpressionNode();

            var opcode = opcodes[AsString];
            if (opcode >= Assembly.Instructions.IFB && opcode <= Assembly.Instructions.IFU)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                    Constant(0));
                if (Child(0).IsIntegralConstant())
                {
                    if (Child(1).IsIntegralConstant()) throw new CompileError("Should have been folded.");
                    var secondFetchToken = Child(1).GetFetchToken(scope);
                    if (secondFetchToken == null)
                    {
                        r.AddChild(Child(1).Emit(context, scope));
                        if (secondOperandResult == Register.STACK)
                            r.AddInstruction(opcode, Child(0).GetConstantToken(), Operand("POP"));
                        else
                            r.AddInstruction(opcode, Child(0).GetConstantToken(),
                                Operand(Scope.GetRegisterLabelSecond((int)secondOperandResult)));
                    }
                    else
                        r.AddInstruction(opcode, Child(0).GetConstantToken(), secondFetchToken);
                }
                else if (Child(1).IsIntegralConstant())
                {
                    var firstFetchToken = Child(0).GetFetchToken(scope);
                    if (firstFetchToken == null)
                    {
                        r.AddChild(Child(0).Emit(context, scope));
                        if (firstOperandResult == Register.STACK)
                        {
                            r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
                            r.AddInstruction(opcode, Operand("A"), Child(1).GetConstantToken());
                        }
                        else
                            r.AddInstruction(opcode, Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                                Child(1).GetConstantToken());
                    }
                    else
                        r.AddInstruction(opcode, firstFetchToken, Child(1).GetConstantToken());
                }
                else
                {
                    var firstFetchToken = Child(0).GetFetchToken(scope);
                    var secondFetchToken = Child(1).GetFetchToken(scope);

                    if (secondFetchToken == null) r.AddChild(Child(1).Emit(context, scope));
                    if (firstFetchToken == null) r.AddChild(Child(0).Emit(context, scope));

                    if (firstOperandResult == Register.STACK && firstFetchToken == null)
                    {
                        r.AddInstruction(Assembly.Instructions.SET, Operand("A"), Operand("POP"));
                        firstOperandResult = Register.A;
                    }

                    r.AddInstruction(opcode,
                        firstFetchToken == null ? Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)) : firstFetchToken,
                        secondFetchToken == null ? Operand(Scope.GetRegisterLabelSecond((int)firstOperandResult)) : secondFetchToken);
                }

                r.AddInstruction(Assembly.Instructions.SET,
                    target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target)),
                    Constant(1));
            }
            else
            {
                if (Child(0).IsIntegralConstant())
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                        Child(0).GetConstantToken());
                else
                    r.AddChild(Child(0).Emit(context, scope));

                DCPUC.Assembly.Operand fetchToken = null;
                if (Child(1) is VariableNameNode) fetchToken = (Child(1) as VariableNameNode).GetFetchToken(scope);

                if (Child(1) is VariableNameNode && fetchToken != null)
                {
                    if (firstOperandResult == Register.STACK)
                        r.AddInstruction(opcode, Operand("PEEK"), fetchToken);
                    else
                        r.AddInstruction(opcode, Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                            fetchToken);
                }
                else if (Child(1).IsIntegralConstant())
                {
                    if (firstOperandResult == Register.STACK)
                        r.AddInstruction(opcode, Operand("PEEK"), Child(1).GetConstantToken());
                    else
                        r.AddInstruction(opcode, Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                            Child(1).GetConstantToken());
                }
                else
                {
                    r.AddChild(Child(1).Emit(context, scope));

                    if (firstOperandResult == Register.STACK)
                    {
                        if (secondOperandResult == Register.STACK)
                        {
                            r.AddInstruction(opcode, Operand("PEEK"), Operand("POP"));
                        }
                        else
                            r.AddInstruction(opcode, Operand("PEEK"),
                                Operand(Scope.GetRegisterLabelSecond((int)secondOperandResult)));
                    }
                    else
                    {
                        r.AddInstruction(opcode,
                            Operand(Scope.GetRegisterLabelFirst((int)firstOperandResult)),
                            Operand(Scope.GetRegisterLabelSecond((int)secondOperandResult)));
                    }
                }
            }
            return r;
        }
    }
}
