using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Assembly;

namespace DCPUB
{
    public class AddressOfNode : CompilableNode
    {
        public Variable variable = null;
        public Function function = null;
        public String variableName;
        public Label label = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override bool IsIntegralConstant()
        {
            return false;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            
        }

        public override Operand GetConstantToken()
        {
            if (function != null)
                return Label(function.label);
            else if (label != null)
                return Label(label.realName);
            else
                return null;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            variable = enclosingScope.FindVariable(variableName);
            if (variable == null) 
            {
                function = enclosingScope.FindFunction(variableName);
                if (function == null)
                {
                    foreach (var l in enclosingScope.activeFunction.function.labels)
                    {
                        if (l.declaredName == variableName)
                            label = l;
                    }
                    if (label == null)
                        throw new CompileError(this, "Could not find symbol " + variableName);
                }
            } 

            ResultType = "word";

            if (variable != null)
            {
                variable.addressTaken = true;
                if (variable.isArray) throw new CompileError(this, "Can't take address of array.");
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Operand GetFetchToken()
        {
            if (variable != null)
            {
                if (variable.type == VariableType.Static)
                    return Label(variable.staticLabel);
                else return null;
            }
            else if (function != null)
                return Label(function.label);
            else if (label != null)
                return Label(label.realName);
            return null;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            Node r = new Assembly.TransientNode();

            if (variable != null)
            {
                if (variable.type == VariableType.Static)
                {
                    r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst(target)), Label(variable.staticLabel));
                }
                else if (variable.type == VariableType.Local)
                {
                    r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst(target)), Operand("J"));
                    r.AddInstruction(Instructions.ADD, Operand(Scope.GetPeekLabel(target)), VariableOffset((ushort)variable.stackOffset));
                }
                else if (variable.type == VariableType.External)
                {
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst(target)), Label(new Assembly.Label("EXTERNALS")));
                    r.AddInstruction(Assembly.Instructions.ADD, Operand(Scope.GetPeekLabel(target)), Constant((ushort)variable.constantValue));
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetPeekLabel(target)), Dereference(Scope.GetPeekLabel(target)));
                }
                else
                    throw new CompileError(this, "Can't take the address of this variable.");
            }
            else if (function != null)
                r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst(target)), Label(function.label));
            else if (label != null)
                r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst(target)), Label(label.realName));

            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            Node r = new Assembly.TransientNode();

            if (variable != null)
            {
                if (variable.type == VariableType.Static)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(variable.staticLabel));
                }
                else if (variable.type == VariableType.Local)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("J"));
                    r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek), VariableOffset((ushort)variable.stackOffset));
                }
                else if (variable.type == VariableType.External)
                {
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), Label(new Assembly.Label("EXTERNALS")));
                    r.AddInstruction(Assembly.Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)variable.constantValue));
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), target.GetOperand(TargetUsage.Peek, OperandSemantics.Dereference));
                }
                else
                    throw new CompileError(this, "Can't take the address of this variable.");
            }
            else if (function != null)
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(function.label));
            else if (label != null)
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Label(label.realName));

            return r;
        }
    }

    
}
