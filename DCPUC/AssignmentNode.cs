using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

        public Register rvalueTargetRegister = Register.STACK;
        public Register lvalueTargetRegister = Register.STACK;
        public Variable assignTo = null;
        public Boolean dereferenceLvalue = false;
        public String @operator;
        
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("LValue", treeNode.ChildNodes[0].ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);
            @operator = treeNode.ChildNodes[1].FindTokenAndGetText();
            ResultType = "void";

            if (opcodes == null)
            {
                opcodes = new Dictionary<string, string>();
                opcodes.Add("=", "SET");
                opcodes.Add("+=", "ADD");
                opcodes.Add("-=", "SUB");
                opcodes.Add("*=", "MUL");
                opcodes.Add("/=", "DIV");

                opcodes.Add("+=signed", "ADD");
                opcodes.Add("-=signed", "SUB");
                opcodes.Add("*=signed", "MLI");
                opcodes.Add("/=signed", "DVI");

                opcodes.Add("%=", "MOD");
                opcodes.Add("<<=", "SHL");
                opcodes.Add(">>=", "SHR");
                opcodes.Add("&=", "AND");
                opcodes.Add("|=", "BOR");
                opcodes.Add("^=", "XOR");
            }
        }

        public override string TreeLabel()
        {
            return @operator + " " + (dereferenceLvalue ? "* " : "") + "[l:" + lvalueTargetRegister.ToString() + " r:" + rvalueTargetRegister.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            Child(0).GatherSymbols(context, enclosingScope);
            if (Child(0) is VariableNameNode)
            {
                assignTo = (Child(0) as VariableNameNode).variable;
                if (assignTo.type == VariableType.Constant || assignTo.type == VariableType.ConstantReference)
                    throw new CompileError("Can't assign to constants");


            }
            else if (Child(0) is DereferenceNode) 
            {
                dereferenceLvalue = true;
            }
            else
                throw new CompileError("Illegal assignment");
            Child(1).GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            Child(1).ResolveTypes(context, enclosingScope);
            if (Child(0).ResultType != Child(1).ResultType)
                context.AddWarning(Span, CompileContext.TypeWarning(Child(1).ResultType, Child(0).ResultType));
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (target != Register.DISCARD)
                throw new CompileError("Assignment should always target discard");

            if (assignTo != null)
            {
                lvalueTargetRegister = assignTo.location;
                if (@operator == "=")
                {
                    if (assignTo.type != VariableType.Local)
                    {
                        rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                        Child(1).AssignRegisters(context, parentState, rvalueTargetRegister);
                        parentState.FreeMaybeRegister(rvalueTargetRegister);
                    }
                    else
                    {
                        if (assignTo.location != Register.STACK)
                            rvalueTargetRegister = assignTo.location;
                        else
                            rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                        Child(1).AssignRegisters(context, parentState, rvalueTargetRegister);
                        if (rvalueTargetRegister != assignTo.location) parentState.FreeMaybeRegister(rvalueTargetRegister);
                    }
                }
                else
                {
                    rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                    Child(1).AssignRegisters(context, parentState, rvalueTargetRegister);
                    parentState.FreeMaybeRegister(rvalueTargetRegister);
                }
            }
            else 
            {
                rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                Child(1).AssignRegisters(context, parentState, rvalueTargetRegister);
                lvalueTargetRegister = parentState.FindAndUseFreeRegister();
                Child(0).AssignRegisters(context, parentState, lvalueTargetRegister);
                parentState.FreeRegisters(lvalueTargetRegister, rvalueTargetRegister);
            }
        }
        
        public override void Emit(CompileContext context, Scope scope)
        {
            Child(1).Emit(context, scope);

            if (assignTo == null)
            {
                //Compound assignment to dereferences are always unsigned operations.

                Child(0).Child(0).Emit(context, scope); //Skip deref node

                context.Add(opcodes[@operator],
                    (dereferenceLvalue ? "[" : "") + Scope.GetRegisterLabelSecond((int)lvalueTargetRegister) + (dereferenceLvalue ? "]" : ""),
                     Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));

                if (lvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            else if (assignTo.type == VariableType.Local)
            {
                var opcode = assignTo.typeSpecifier == "signed" ? opcodes[@operator + "signed"] : opcodes[@operator];
               if (assignTo.location != rvalueTargetRegister)
               {
                   if (assignTo.location == Register.STACK)
                   {
                       var stackOffset = scope.StackOffset(assignTo.stackOffset);
                       if (stackOffset > 0)
                       {
                           context.Add(opcode, "[" + Hex.hex(stackOffset)  + "+SP]",
                               Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                       }
                       else
                           context.Add(opcode, "PEEK", Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                   }
                   else
                       context.Add(opcode, Scope.GetRegisterLabelFirst((int)assignTo.location),
                           Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));

               }
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            else if (assignTo.type == VariableType.Static)
            {
                var opcode = assignTo.typeSpecifier == "signed" ? opcodes[@operator + "signed"] : opcodes[@operator];
                context.Add(opcode, "[" + assignTo.staticLabel + "]", Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            
           
        }
    }

    
}
