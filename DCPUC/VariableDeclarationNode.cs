using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class VariableDeclarationNode : CompilableNode
    {
        string declLabel = "";
        Variable variable = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Value", treeNode.ChildNodes[4].FirstChild);
            declLabel = treeNode.ChildNodes[0].FindTokenAndGetText();
            variable = new Variable();
            variable.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            variable.typeSpecifier = treeNode.ChildNodes[2].FindTokenAndGetText();
            variable.assignedBy = this;
            if (variable.typeSpecifier == null) variable.typeSpecifier = "unsigned";
        }

        public override string TreeLabel()
        {
            return declLabel + " " + variable.name + (variable.typeSpecifier != null ? ":" + variable.typeSpecifier : "") 
                + " [loc:" + variable.location.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            Child(0).GatherSymbols(context, enclosingScope);

            enclosingScope.variables.Add(variable);
            variable.scope = enclosingScope;
            
            if (declLabel == "var")
            {
                variable.type = VariableType.Local;

            }
            else if (declLabel == "static")
            {
                variable.type = VariableType.Static;
                variable.location = Register.STATIC;
                if (Child(0) is DataLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, (Child(0) as DataLiteralNode).dataLabel);
                }
                else if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, (Child(0) as BlockLiteralNode).dataLabel);
                }
                else if (Child(0).IsIntegralConstant()) //Other expressions should fold if they are constant.
                {
                    variable.staticLabel = context.GetLabel() + "_STATIC_" + variable.name;
                    context.AddData(variable.staticLabel, (ushort)Child(0).GetConstantValue());
                }
                else
                    throw new CompileError(this, "Statics must be initialized to a constant value.");
            }
            else if (declLabel == "const")
            {
                variable.location = Register.CONST;

                if (Child(0) is DataLiteralNode)
                {
                    variable.staticLabel = (Child(0) as DataLiteralNode).dataLabel;
                    variable.type = VariableType.ConstantReference;
                }
                else if (Child(0) is BlockLiteralNode)
                {
                    variable.staticLabel = (Child(0) as DataLiteralNode).dataLabel;
                    variable.type = VariableType.ConstantReference;
                }
                else if (Child(0) is NumberLiteralNode) //Other expressions should fold if they are constant.
                {
                    variable.constantValue = (Child(0) as NumberLiteralNode).Value;
                    variable.type = VariableType.Constant;
                }
                else
                    throw new CompileError(this, "Consts must be initialized to a constant value.");
            }
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (Child(0).ResultType != variable.typeSpecifier)
                context.AddWarning(Span, "Conversion of " + Child(0).ResultType + " to " + variable.typeSpecifier + ". Possible loss of data.");
            if (!Scope.IsBuiltIn(variable.typeSpecifier))
            {
                variable.structType = enclosingScope.FindType(variable.typeSpecifier);
                if (variable.structType == null)
                    throw new CompileError(this, "Could not find type " + variable.typeSpecifier);
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (variable.type == VariableType.Local)
            {
                variable.location = Register.STACK;
                /*
                variable.location = parentState.FindAndUseFreeRegister();
                if (variable.location == Register.I)
                {
                    variable.location = Register.STACK;
                    parentState.FreeMaybeRegister(Register.I);
                }*/
                Child(0).AssignRegisters(context, parentState, variable.location);
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            variable.stackOffset = scope.stackDepth;
            
            if (variable.type == VariableType.Local)
            {
                if (Child(0) is BlockLiteralNode)
                {
                    var size = (Child(0) as BlockLiteralNode).dataSize;
                    scope.stackDepth += size;
                    r.AddInstruction(Assembly.Instructions.SUB, Operand("SP"), Constant((ushort)size));
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)variable.location)), 
                        Operand("SP"));
                    variable.stackOffset = scope.stackDepth;
                    if (variable.location == Register.STACK) scope.stackDepth += 1;
                }
                else
                    r.AddChild(Child(0).Emit(context, scope));
            }
            return r;
        }
    }
}
