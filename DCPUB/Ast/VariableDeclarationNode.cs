using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class VariableDeclarationNode : CompilableNode
    {
        string declLabel = "";
        Model.Variable variable = null;
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
            variable = new Model.Variable();
            variable.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            variable.typeSpecifier = treeNode.ChildNodes[2].FindTokenAndGetText();
            variable.assignedBy = this;
            variable.isArray = isArray;
            if (variable.typeSpecifier == null) variable.typeSpecifier = "word";
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            
            enclosingScope.variables.Add(variable);
            variable.scope = enclosingScope;
            
            if (declLabel == "local")
            {
                variable.type = Model.VariableType.Local;
            }
            else if (declLabel == "static")
            {
                variable.type = Model.VariableType.Static;
                variable.staticLabel = Intermediate.Label.Make("_STATIC_" + variable.name);
            }
            else if (declLabel == "external")
            {
                if (context.options.externals == false)
                {
                    context.ReportError(this, "Compile with -externals to support externals.");
                    variable.type = Model.VariableType.Local;
                }
                if (enclosingScope.type != Model.ScopeType.Global)
                {
                    context.ReportError(this, "Externals can only be declared at global scope.");
                    variable.type = Model.VariableType.Local;
                }
                variable.type = Model.VariableType.External;
            }
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);

            if (!Model.Scope.IsBuiltIn(variable.typeSpecifier))
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
                            var data = new List<Intermediate.Operand>();
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
                        else if ((valueToken.semantics & Intermediate.OperandSemantics.Label) == Intermediate.OperandSemantics.Label)
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

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddChild(new Annotation(context.GetSourceSpan(this.Span)));

            if (variable.type == Model.VariableType.Local)
            {
                variable.stackOffset = -(scope.variablesOnStack + size);
                if (hasInitialValue) r.AddChild(Child(0).Emit(context, scope, Target.Stack));
                else r.AddInstruction(Instructions.SUB, Operand("SP"), Constant((ushort)size));
                scope.variablesOnStack += size;
            }
            return r;
        }
    }
}
