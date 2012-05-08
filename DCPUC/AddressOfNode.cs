using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUC.Assembly;

namespace DCPUC
{
    public class AddressOfNode : CompilableNode
    {
        public Variable variable = null;
        public Function function = null;
        public String variableName;
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            variableName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return "addof " + variableName + " [into:" + target.ToString() + "]";
        }

        public override bool IsIntegralConstant()
        {
            return false;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
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
            {
                scope = enclosingScope;
                while (function == null && scope != null)
                {
                    foreach (var v in scope.functions)
                        if (v.name == variableName)
                            function = v;
                    if (function == null) scope = scope.parent;
                }

                if (function == null) throw new CompileError(this, "Could not find symbol " + variableName);
            } 

            ResultType = "unsigned";

            if (variable != null)
            {
                variable.addressTaken = true;
                if (variable.type == VariableType.Constant) throw new CompileError("Integral constants have no address.");
                if (variable.type == VariableType.ConstantReference) throw new CompileError("Can't take address of constant reference.");
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            Node r = new Assembly.ExpressionNode();

            if (variable != null)
            {
                if (variable.type == VariableType.Constant)
                {
                    //context.Add("SET", Scope.GetRegisterLabelFirst((int)target), Hex.hex(variable.constantValue));
                }
                else if (variable.type == VariableType.ConstantReference)
                {
                    //context.Add("SET", Scope.GetRegisterLabelFirst((int)target), variable.staticLabel);
                }
                else if (variable.type == VariableType.Static)
                {
                    r.AddInstruction(Instructions.SET, Scope.GetRegisterLabelFirst((int)target), variable.staticLabel);
                }
                else if (variable.type == VariableType.Local)
                {
                    if (variable.location == Register.STACK)
                    {
                        var stackOffset = scope.StackOffset(variable.stackOffset);
                        r.AddInstruction(Instructions.SET, Scope.GetRegisterLabelFirst((int)target), "SP");
                        if (stackOffset != 0)
                        {
                            if (target == Register.STACK)
                                r.AddInstruction(Instructions.ADD, "PEEK", Hex.hex(stackOffset));
                            else
                                r.AddInstruction(Instructions.ADD, Scope.GetRegisterLabelFirst((int)target), Hex.hex(stackOffset));
                        }
                    }
                    else
                        throw new CompileError("Variable should be on stack");
                }
            }
            else if (function != null)
            {
                r.AddInstruction(Instructions.SET, Scope.GetRegisterLabelFirst((int)target), function.label);
            }

            if (target == Register.STACK) scope.stackDepth += 1;
            return r;
        }
    }

    
}
