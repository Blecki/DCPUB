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
        public Label label = null;

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

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            Node r = new Assembly.ExpressionNode();

            if (variable != null)
            {
                if (variable.type == VariableType.Static)
                {
                    r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), Label(variable.staticLabel));
                }
                else if (variable.type == VariableType.Local)
                {
                    if (variable.location == Register.STACK)
                    {
                        r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                            Operand("J"));
                        if (target != Register.STACK)
                            r.AddInstruction(Instructions.ADD, Operand(Scope.GetRegisterLabelFirst((int)target)),
                                Constant((ushort)variable.stackOffset));
                        else
                            r.AddInstruction(Instructions.ADD, Operand("PEEK"), Constant((ushort)variable.stackOffset));
                    }
                    else
                        throw new CompileError("Variable should be on stack");
                }
                else if (variable.type == VariableType.External)
                {
                    r.AddInstruction(Assembly.Instructions.SET,
                        Operand(Scope.GetRegisterLabelFirst((int)target)),
                        Label(new Assembly.Label("EXTERNALS")));
                    r.AddInstruction(Assembly.Instructions.ADD,
                        (target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target))),
                        Constant((ushort)variable.constantValue));
                    r.AddInstruction(Assembly.Instructions.SET,
                        (target == Register.STACK ? Operand("PEEK") : Operand(Scope.GetRegisterLabelFirst((int)target))),
                        (target == Register.STACK ? Dereference("PEEK") : Dereference(Scope.GetRegisterLabelSecond((int)target))));
                }
                else
                    throw new CompileError(this, "Can't take the address of this variable.");
            }
            else if (function != null)
            {
                r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), Label(function.label));
            }
            else if (label != null)
            {
                r.AddInstruction(Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), Label(label.realName));
            }

            return r;
        }
    }

    
}
