﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class VariableNameNode : CompilableNode, AssignableNode
    {
        public Variable variable = null;
        public String variableName;
        public bool IsAssignedTo { get; set; }

        public VariableNameNode()
        {
            IsAssignedTo = false;
        }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return "varref " + variableName + " [into:" + target.ToString() + "]";
        }

        public override int ReferencesVariable(DCPUB.Variable v)
        {
            if (v == variable) return 1;
            return 0;
        }

        public override bool IsIntegralConstant()
        {
            if (variable.type == VariableType.Constant)
                return true;
            return false;
        }

        public override int GetConstantValue()
        {
            return variable.constantValue;
        }

        public override Assembly.Operand GetConstantToken()
        {
            if (variable.type == VariableType.ConstantLabel)
                return Label(variable.staticLabel);
            else if (variable.type == VariableType.Constant)
                return Constant((ushort)GetConstantValue());
            else
                return null;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            var scope = enclosingScope;
            bool ignoreLocals = false;
            while (variable == null && scope != null)
            {
                foreach (var v in scope.variables)
                    if (v.name == variableName)
                    {
                        if (v.type == VariableType.Local && ignoreLocals) variable = null;
                        else variable = v;
                    }
                if (variable == null)
                {
                    if (scope.type == ScopeType.Function) ignoreLocals = true;
                    scope = scope.parent;
                }
            }

            if (variable == null)
                throw new CompileError(this, "Could not find variable " + variableName);

        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            if (variable != null) ResultType = variable.typeSpecifier;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.TransientNode();
            if (variable.type == VariableType.Constant)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                    GetConstantToken());
            }
            else if (variable.type == VariableType.ConstantLabel)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                    Label(variable.staticLabel));
            }
            else if (variable.type == VariableType.External)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                    Label(new Assembly.Label("EXTERNALS")));
                r.AddInstruction(Assembly.Instructions.ADD,
                    (target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target))),
                    Constant((ushort)variable.constantValue));
                r.AddInstruction(Assembly.Instructions.SET,
                    (target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target))),
                    (target == Register.STACK ? Dereference("PEEK") : Dereference(Scope.GetRegisterLabelSecond((int)target))));
                r.AddInstruction(Assembly.Instructions.SET,
                    (target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target))),
                    (target == Register.STACK ? Dereference("PEEK") : Dereference(Scope.GetRegisterLabelSecond((int)target))));
            }
            else if (variable.type == VariableType.Static)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                    variable.isArray ? Label(variable.staticLabel) : DereferenceLabel(variable.staticLabel));
            }
            else if (variable.type == VariableType.Local)
            {
                    if (variable.isArray)
                    {
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            Operand("J"));
                        r.AddInstruction(Assembly.Instructions.ADD, 
                            target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target)),
                            VariableOffset((ushort)variable.stackOffset));
                    }
                    else
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            DereferenceVariableOffset((ushort)(variable.stackOffset)));
            }

            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            if (variable.type == VariableType.Constant)
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                    GetConstantToken());
            }
            else if (variable.type == VariableType.ConstantLabel)
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                    Label(variable.staticLabel));
            }
            else if (variable.type == VariableType.External)
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                    Label(new Assembly.Label("EXTERNALS")));
                r.AddInstruction(Assembly.Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)variable.constantValue));
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Peek), target.GetOperand(TargetUsage.Peek, Assembly.OperandSemantics.Dereference));
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Peek), target.GetOperand(TargetUsage.Peek, Assembly.OperandSemantics.Dereference));
            }
            else if (variable.type == VariableType.Static)
            {
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                    variable.isArray ? Label(variable.staticLabel) : DereferenceLabel(variable.staticLabel));
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.isArray)
                {
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("J"));
                    r.AddInstruction(Assembly.Instructions.ADD, target.GetOperand(TargetUsage.Peek),
                        VariableOffset((ushort)variable.stackOffset));
                }
                else
                    r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                        DereferenceVariableOffset((ushort)(variable.stackOffset)));
            }

            return r;
        }

        public Assembly.Node EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode)
        {
            var r = new Assembly.TransientNode();
            if (variable.isArray) throw new CompileError("Can't assign to arrays.");
            if (variable.type == VariableType.Constant || variable.type == VariableType.External 
                || variable.type == VariableType.ConstantLabel)
                throw new CompileError(this, "Can't assign to constant values");

            if (variable.type == VariableType.Local)
                    r.AddInstruction(opcode, DereferenceVariableOffset((ushort)variable.stackOffset), from);
            else if (variable.type == VariableType.Static)
                r.AddInstruction(opcode, DereferenceLabel(variable.staticLabel), from);
            return r;
        }

        public Assembly.Node EmitAssignment2(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode)
        {
            var r = new Assembly.TransientNode();
            if (variable.isArray) throw new CompileError("Can't assign to arrays.");
            if (variable.type == VariableType.Constant || variable.type == VariableType.External
                || variable.type == VariableType.ConstantLabel)
                throw new CompileError(this, "Can't assign to constant values");

            if (variable.type == VariableType.Local)
                r.AddInstruction(opcode, DereferenceVariableOffset((ushort)variable.stackOffset), from);
            else if (variable.type == VariableType.Static)
                r.AddInstruction(opcode, DereferenceLabel(variable.staticLabel), from);
            return r;
        }

        public override Assembly.Operand GetFetchToken()
        {
            if (variable.type == VariableType.Static)
            {
                if (variable.isArray) return Label(variable.staticLabel);
                else return DereferenceLabel(variable.staticLabel);
            }
            else if (variable.type == VariableType.ConstantLabel)
            {
                return Label(variable.staticLabel);
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.isArray) return null;
                return DereferenceVariableOffset((ushort)(variable.stackOffset));
            }
            else if (variable.type == VariableType.Constant)
                return Constant((ushort)variable.constantValue);
            else if (variable.type == VariableType.External)
            {
                return null;
                //throw new CompileError(this, "Attempt to get fetch token from external variable.");
            }
            else
                throw new CompileError(this, "Unreachable code reached.");

        }
    }

    
}
