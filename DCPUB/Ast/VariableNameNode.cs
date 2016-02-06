﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

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

        public bool TryGatherSymbols(CompileContext Context, Scope EnclosingScope)
        {
            var scope = EnclosingScope;
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

            return variable != null;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            if (!TryGatherSymbols(context, enclosingScope))
                context.ReportError(this, "Could not find variable " + variableName);

        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            if (variable != null) ResultType = variable.typeSpecifier;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new TransientNode();

            if (variable == null)
            {
                context.ReportError(this, "Variable name was not resolved.");
                return r;
            }

            if (variable.type == VariableType.Constant)
            {
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), GetFetchToken());
            }
            else if (variable.type == VariableType.ConstantLabel)
            {
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push),
                    Label(variable.staticLabel));
            }
            else if (variable.type == VariableType.External)
            {
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push),
                    Label(new Intermediate.Label("EXTERNALS")));
                r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek), Constant((ushort)variable.constantValue));
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Peek), target.GetOperand(TargetUsage.Peek, Intermediate.OperandSemantics.Dereference));
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Peek), target.GetOperand(TargetUsage.Peek, Intermediate.OperandSemantics.Dereference));
            }
            else if (variable.type == VariableType.Static)
            {
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push),
                    variable.isArray ? Label(variable.staticLabel) : DereferenceLabel(variable.staticLabel));
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.isArray)
                {
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("J"));
                    r.AddInstruction(Instructions.ADD, target.GetOperand(TargetUsage.Peek),
                        VariableOffset((ushort)variable.stackOffset));
                }
                else
                    r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push),
                        DereferenceVariableOffset((ushort)(variable.stackOffset)));
            }

            return r;
        }

        public Intermediate.IRNode EmitAssignment(CompileContext context, Scope scope, Intermediate.Operand from, Instructions opcode)
        {
            var r = new TransientNode();
            if (variable.isArray)
            {
                context.ReportError(this, "Can't assign to arrays.");
                return r;
            }
            if (variable.type == VariableType.Constant || variable.type == VariableType.External 
                || variable.type == VariableType.ConstantLabel)
            {
                context.ReportError(this, "Can't assign to constant values.");
                return r;
            }

            if (variable.type == VariableType.Local)
                    r.AddInstruction(opcode, DereferenceVariableOffset((ushort)variable.stackOffset), from);
            else if (variable.type == VariableType.Static)
                r.AddInstruction(opcode, DereferenceLabel(variable.staticLabel), from);
            return r;
        }

        public override Intermediate.Operand GetFetchToken()
        {
            if (variable == null) return null;

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
            }
            else
                throw new InternalError("Unreachable code reached.");

        }
    }

    
}
