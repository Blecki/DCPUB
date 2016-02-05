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
                {
                    context.ReportError(this, "Compile with -externals to support externals.");
                    variable.type = VariableType.Local;
                }
                if (enclosingScope.type != ScopeType.Global)
                {
                    context.ReportError(this, "Externals can only be declared at global scope.");
                    variable.type = VariableType.Local;
                }
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
                {
                    context.ReportError(this, "Could not find type " + variable.typeSpecifier);
                    variable.typeSpecifier = "word";
                }
            }

            if (isArray)
            {
                if (declLabel == "external") context.ReportError(this, "Can't have external array.");

                var sizeToken = Child(1).GetFetchToken();
                if (!sizeToken.IsIntegralConstant())
                {
                    context.ReportError(this, "Array sizes must be a compile time constant.");
                    size = 1;
                }
                else
                    size = sizeToken.constant;

                if (hasInitialValue && !(Child(0) is ArrayInitializationNode))
                    context.ReportError(this, "Can't initialize an array this way.");
                else if (hasInitialValue && (Child(0) as ArrayInitializationNode).RawData.Count != size)
                    context.ReportError(this, "Array initialization size mismatch");

                if (declLabel == "static")
                {
                    if (hasInitialValue)
                        context.AddData(variable.staticLabel, (Child(0) as ArrayInitializationNode).RawData);
                    else
                    {
                            var data = new List<Assembly.Operand>();
                            for (int i = 0; i < size; ++i) data.Add(Constant(0));
                        context.AddData(variable.staticLabel, data);
                    }
                }
            }
            else if (declLabel == "static")
            {
                if (hasInitialValue)
                {
                    var valueToken = Child(0).GetFetchToken();
                    if (valueToken == null)
                        context.ReportError(this, "Static variables must be initialized with a static value.");
                    else
                    {
                        if (valueToken.IsIntegralConstant())
                            context.AddData(variable.staticLabel, valueToken.constant);
                        else if ((valueToken.semantics & Assembly.OperandSemantics.Label) == Assembly.OperandSemantics.Label)
                            context.AddData(variable.staticLabel, valueToken.label);
                        else
                            context.ReportError(this, "Static variables must be initialized with a static value.");
                    }
                }
                else
                    context.AddData(variable.staticLabel, 0);
            }
            else if (declLabel == "external")
            {
                variable.constantValue = context.externalCount;
                context.externalCount += 1;
                if (hasInitialValue) context.ReportError(this, "Can't initialize externals.");
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
