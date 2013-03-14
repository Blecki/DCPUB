using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
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
            variable.isArray = isArray;
            if (variable.typeSpecifier == null) variable.typeSpecifier = "word";
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
                variable.staticLabel = Assembly.Label.Make("_STATIC_" + variable.name);
            }
            else if (declLabel == "external")
            {
                if (context.options.externals == false)
                    throw new CompileError(this, "Compile with -externals to support externals.");
                if (enclosingScope.type != ScopeType.Global)
                    throw new CompileError(this, "Externals can only be declared at global scope.");
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

            if (isArray)
            {
                if (declLabel == "external") throw new CompileError("Can't have external array.");

                var sizeToken = Child(1).GetFetchToken();
                if (!sizeToken.IsIntegralConstant()) throw new CompileError(this, "Array sizes must be a compile time constant.");
                size = sizeToken.constant;

                if (hasInitialValue && !(Child(0) is ArrayInitializationNode)) throw new CompileError("Can't initialize an array this way.");
                if (hasInitialValue && (Child(0) as ArrayInitializationNode).rawData.Length != size)
                    throw new CompileError("Array initialization size mismatch");

                if (declLabel == "static")
                {
                    if (hasInitialValue) context.AddData(variable.staticLabel, new List<ushort>((Child(0) as ArrayInitializationNode).rawData));
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
                    var valueToken = Child(0).GetFetchToken();
                    if (valueToken.IsIntegralConstant())
                        context.AddData(variable.staticLabel, valueToken.constant);
                    else if ((valueToken.semantics & Assembly.OperandSemantics.Label) == Assembly.OperandSemantics.Label)
                        context.AddData(variable.staticLabel, valueToken.label);
                    else
                        throw new CompileError(this, "Static variables must be initialized with a static value.");
                }
                else
                    context.AddData(variable.staticLabel, 0);
            }
            else if (declLabel == "external")
            {
                variable.constantValue = context.externalCount;
                context.externalCount += 1;
                if (hasInitialValue) throw new CompileError("Can't initialize externals.");
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            if (variable.type == VariableType.Local)
            {
                variable.stackOffset = -(scope.variablesOnStack + size);
                if (hasInitialValue) r.AddChild(Child(0).Emit(context, scope, Target.Stack));
                else r.AddInstruction(Assembly.Instructions.SUB, Operand("SP"), Constant((ushort)size));
                scope.variablesOnStack += size;
            }
            return r;
        }
    }
}
