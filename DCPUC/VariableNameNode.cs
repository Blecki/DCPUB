﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
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

        public override int ReferencesVariable(DCPUC.Variable v)
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

        public override string GetConstantToken()
        {
            return Hex.hex(GetConstantValue());
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            var scope = enclosingScope;
            while (variable == null && scope != null)
            {
                foreach (var v in scope.variables)
                    if (v.name == variableName)
                        variable = v;
                if (variable == null) scope = scope.parent;
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
            var r = new Assembly.ExpressionNode();
            if (variable.type == VariableType.Constant)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), 
                    Constant((ushort)variable.constantValue));
            }
            else if (variable.type == VariableType.ConstantReference)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), 
                    Label(variable.staticLabel));
            }
            else if (variable.type == VariableType.Static)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), 
                    Dereference(variable.staticLabel));
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.location == Register.STACK)
                {
                    var stackOffset = scope.StackOffset(variable.stackOffset);
                    if (stackOffset > 0)
                    {
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            DereferenceOffset("SP", (ushort)stackOffset));
                    }
                    else
                        r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), 
                            Operand("PEEK"));
                }
                else if (variable.location == target)
                {
                    //context.Add(";SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location));
                }
                else
                {
                    r.AddInstruction(Assembly.Instructions.SET, 
                        Operand(Scope.GetRegisterLabelFirst((int)target)), 
                        Operand(Scope.GetRegisterLabelSecond((int)variable.location)));
                }
            }

            if (target == Register.STACK) scope.stackDepth += 1;
            return r;
        }

        public Assembly.Node EmitAssignment(CompileContext context, Scope scope, Register from, Assembly.Instructions opcode)
        {
            var r = new Assembly.Node();
            if (variable.type == VariableType.Constant || variable.type == VariableType.ConstantReference)
                throw new CompileError(this, "Can't assign to constant values");

            if (variable.type == VariableType.Local)
            {
                if (variable.location == Register.STACK)
                {
                    var stackOffset = scope.StackOffset(variable.stackOffset);
                    if (stackOffset > 0)
                        r.AddInstruction(opcode, DereferenceOffset("SP", (ushort)stackOffset), 
                            Operand(Scope.GetRegisterLabelSecond((int)from)));
                    else
                        r.AddInstruction(opcode, Operand("PEEK"), Operand(Scope.GetRegisterLabelSecond((int)from)));
                }
                else
                    r.AddInstruction(opcode, Operand(Scope.GetRegisterLabelFirst((int)variable.location)), 
                        Operand(Scope.GetRegisterLabelSecond((int)from)));
            }
            else if (variable.type == VariableType.Static)
                r.AddInstruction(opcode, Dereference(variable.staticLabel), Operand(Scope.GetRegisterLabelSecond((int)from)));
            return r;
        }

        public override Assembly.Operand GetFetchToken(Scope scope)
        {
            if (variable.type == VariableType.Constant)
                return Constant((ushort)variable.constantValue);
            else if (variable.type == VariableType.ConstantReference)
                return Label(variable.staticLabel);
            else if (variable.type == VariableType.Static)
                return DereferenceLabel(variable.staticLabel);
            else if (variable.type == VariableType.Local)
            {
                if (variable.location == Register.STACK)
                {
                    var stackOffset = scope.StackOffset(variable.stackOffset);
                    if (stackOffset > 0)
                        return DereferenceOffset("SP", (ushort)stackOffset);
                    else
                        return Operand("PEEK");
                }
                else
                    return Operand(Scope.GetRegisterLabelSecond((int)variable.location));
            }
            else
                throw new CompileError(this, "Unreachable code reached.");

        }
    }

    
}
