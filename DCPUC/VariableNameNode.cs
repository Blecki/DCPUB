using System;
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
        Register target;
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
                r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), Hex.hex(variable.constantValue));
            }
            else if (variable.type == VariableType.ConstantReference)
            {
                r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), variable.staticLabel);
            }
            else if (variable.type == VariableType.Static)
            {
                r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), "[" + variable.staticLabel + "]");
            }
            else if (variable.type == VariableType.Local)
            {
                if (variable.location == Register.STACK)
                {
                    var stackOffset = scope.StackOffset(variable.stackOffset);
                    if (stackOffset > 0)
                    {
                        r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target),
                            "[" + Hex.hex(stackOffset) + "+SP]");
                    }
                    else
                        r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), "PEEK");
                }
                else if (variable.location == target)
                {
                    //context.Add(";SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location));
                }
                else
                {
                    r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location));
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
                        r.AddInstruction(opcode, "[SP+" + Hex.hex(stackOffset) + "]", Scope.GetRegisterLabelSecond((int)from));
                    else
                        r.AddInstruction(opcode, "PEEK", Scope.GetRegisterLabelSecond((int)from));
                }
                else
                    r.AddInstruction(opcode, Scope.GetRegisterLabelFirst((int)variable.location), Scope.GetRegisterLabelSecond((int)from));
            }
            else if (variable.type == VariableType.Static)
                r.AddInstruction(opcode, "[" + variable.staticLabel + "]", Scope.GetRegisterLabelSecond((int)from));
            return r;
        }
    }

    
}
