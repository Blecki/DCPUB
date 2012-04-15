using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        public Register rvalueTargetRegister = Register.STACK;
        public Register lvalueTargetRegister = Register.STACK;
        public Variable assignTo = null;
        public Boolean dereferenceLvalue = false;
        
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("LValue", treeNode.ChildNodes[0].ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);
        }

        public override string TreeLabel()
        {
            return "= " + (dereferenceLvalue ? "* " : "") + "[l:" + lvalueTargetRegister.ToString() + " r:" + rvalueTargetRegister.ToString() + "]";
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

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            if (target != Register.DISCARD)
                throw new CompileError("Assignment should always target discard");

            if (assignTo != null)
            {
                lvalueTargetRegister = assignTo.location;
                if (assignTo.type != VariableType.Local)
                {
                    rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                    Child(1).AssignRegisters(parentState, rvalueTargetRegister);
                    parentState.FreeMaybeRegister(rvalueTargetRegister);
                }
                else
                {
                    if (assignTo.location != Register.STACK)
                        rvalueTargetRegister = assignTo.location;
                    else
                        rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                    Child(1).AssignRegisters(parentState, rvalueTargetRegister);
                    parentState.FreeMaybeRegister(rvalueTargetRegister);
                }
            }
            else 
            {
                rvalueTargetRegister = parentState.FindAndUseFreeRegister();
                Child(1).AssignRegisters(parentState, rvalueTargetRegister);
                lvalueTargetRegister = parentState.FindAndUseFreeRegister();
                Child(0).AssignRegisters(parentState, lvalueTargetRegister);
                parentState.FreeRegisters(lvalueTargetRegister, rvalueTargetRegister);
            }
        }
        
        public override void Emit(CompileContext context, Scope scope)
        {
            if (assignTo == null)
            {
                Child(1).Emit(context, scope);
                Child(0).Child(0).Emit(context, scope); //Skip deref node

                context.Add("SET",
                    (dereferenceLvalue ? "[" : "") + Scope.GetRegisterLabelSecond((int)lvalueTargetRegister) + (dereferenceLvalue ? "]" : ""),
                     Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));

                if (lvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            else if (assignTo.type == VariableType.Local)
            {
               Child(1).Emit(context, scope);
               if (assignTo.location != rvalueTargetRegister)
               {
                   if (assignTo.location == Register.STACK)
                   {
                       var stackOffset = scope.StackOffset(assignTo.stackOffset);
                       if (stackOffset > 0)
                       {
                           context.Add("SET", Scope.TempRegister, "SP");
                           context.Add("SET", "[" + Hex.hex(stackOffset) + "+" + Scope.TempRegister + "]",
                               Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                       }
                       else
                           context.Add("SET", "PEEK", Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                   }
                   else
                       context.Add("SET", Scope.GetRegisterLabelFirst((int)assignTo.location),
                           Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));

               }
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            else if (assignTo.type == VariableType.Static)
            {
                Child(1).Emit(context, scope);
                context.Add("SET", "[" + assignTo.staticLabel + "]", Scope.GetRegisterLabelSecond((int)rvalueTargetRegister));
                if (rvalueTargetRegister == Register.STACK) scope.stackDepth -= 1;
            }
            
           
        }
    }

    
}
