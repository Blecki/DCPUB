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
        int size = 1;
        bool hasInitialValue = false;
        bool isArray = false;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes[4].FirstChild.ChildNodes.Count > 0)
            {
                AddChild("Value", treeNode.ChildNodes[4].FirstChild.LastChild.FirstChild);
                hasInitialValue = true;
            }
            else
            {
                var newNode = new NumberLiteralNode();
                newNode.Value = 0;
                ChildNodes.Add(newNode);
            }

            if (treeNode.ChildNodes[3].FirstChild.ChildNodes.Count > 0)
            {
                AddChild("Size", treeNode.ChildNodes[3].FirstChild.FirstChild);
                isArray = true;
            }

            declLabel = treeNode.ChildNodes[0].FindTokenAndGetText();
            variable = new Variable();
            variable.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            variable.typeSpecifier = treeNode.ChildNodes[2].FindTokenAndGetText();
            variable.assignedBy = this;
            if (variable.typeSpecifier == null) variable.typeSpecifier = "word";
        }

        public override string TreeLabel()
        {
            return declLabel + " " + variable.name + (variable.typeSpecifier != null ? ":" + variable.typeSpecifier : "") 
                + " [loc:" + variable.location.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);

            enclosingScope.variables.Add(variable);
            variable.scope = enclosingScope;
            
            if (declLabel == "local")
            {
                variable.type = VariableType.Local;
            }
            else if (declLabel == "static")
            {
                variable.type = VariableType.Static;
                variable.location = Register.STATIC;
                variable.staticLabel = Assembly.Label.Make("_STATIC_" + variable.name);
            }
            else if (declLabel == "constant")
            {
                variable.type = VariableType.Constant;
                variable.location = Register.CONST;
            }
            else if (declLabel == "external")
            {
                if (enclosingScope.type != ScopeType.Global)
                    throw new CompileError("Externals can only be declared at global scope.");
                variable.type = VariableType.External;
            }
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (!Scope.IsBuiltIn(variable.typeSpecifier))
            {
                variable.structType = enclosingScope.FindType(variable.typeSpecifier);
                if (variable.structType == null)
                    throw new CompileError(this, "Could not find type " + variable.typeSpecifier);
            }
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            if (isArray)
            {
                if (declLabel == "constant")
                    throw new CompileError("Can't have constant array.");
                if (declLabel == "external")
                    throw new CompileError("Can't have external array.");
            
                if (!Child(1).IsIntegralConstant()) throw new CompileError("Array sizes must be a compile time constant.");
                size = Child(1).GetConstantValue();

                if (hasInitialValue && !(Child(0) is ArrayInitializationNode))
                    throw new CompileError("Can't initialize an array this way.");
                if (hasInitialValue && (Child(0) as ArrayInitializationNode).rawData.Length != size)
                    throw new CompileError("Array initialization size mismatch");

                if (declLabel == "static")
                {
                    if (hasInitialValue)
                        context.AddData(variable.staticLabel, new List<ushort>((Child(0) as ArrayInitializationNode).rawData));
                    else
                    {
                        var data = new ushort[size];
                        for (int i = 0; i < size; ++i) data[i] = 0;
                        context.AddData(variable.staticLabel, new List<ushort>(data));
                    }
                }
            }
            else if (declLabel == "static")
            {
                if (hasInitialValue)
                {
                    if (!Child(0).IsIntegralConstant())
                        throw new CompileError(this, "Statics must be initialized to a constant value.");
                    context.AddData(variable.staticLabel, (ushort)Child(0).GetConstantValue());
                }
                else
                    context.AddData(variable.staticLabel, 0);
            }
            else if (declLabel == "constant")
            {
                if (!hasInitialValue) throw new CompileError("Constants must have a value.");
                if (!Child(0).IsIntegralConstant()) throw new CompileError("Constants must be initialized to a constant value.");
                variable.constantValue = Child(0).GetConstantValue();
            }
            else if (declLabel == "external")
            {
                variable.constantValue = context.externalCount;
                context.externalCount += 1;
                if (hasInitialValue) throw new CompileError("Can't initialize externals.");
            }
            return this;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (variable.type == VariableType.Local)
            {
                variable.location = Register.STACK;
                Child(0).AssignRegisters(context, parentState, variable.location);
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            if (variable.type == VariableType.Local)
            {
                variable.stackOffset = -(scope.variablesOnStack + size);
                if (hasInitialValue) r.AddChild(Child(0).Emit(context, scope));
                else r.AddInstruction(Assembly.Instructions.SUB, Operand("SP"), Constant((ushort)size));
                scope.variablesOnStack += size;
            }
            return r;
        }
    }
}
