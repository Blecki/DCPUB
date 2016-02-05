using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class FunctionCallNode : CompilableNode
    {
        Function function;
        String functionName;
        Scope enclosingScope;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            functionName = null;//treeNode.ChildNodes[0].FindTokenAndGetText();
            AddChild("expression", treeNode.ChildNodes[0]);//.FirstChild);
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            if (Child(0) is VariableNameNode)
            {
                if (!(Child(0) as VariableNameNode).TryGatherSymbols(context, enclosingScope))
                    functionName = (Child(0) as VariableNameNode).variableName;
            }
            else
                Child(0).GatherSymbols(context, enclosingScope);

            for (var i = 1; i < ChildNodes.Count; ++i) Child(i).GatherSymbols(context, enclosingScope);

            this.enclosingScope = enclosingScope;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);

            if (functionName != null)
            {
                var func_scope = enclosingScope;
                while (function == null && func_scope != null)
                {
                    foreach (var v in func_scope.functions)
                        if (v.name == functionName)
                            function = v;
                    if (function == null) func_scope = func_scope.parent;
                }

                if (function == null)
                {
                    context.ReportError(this, "Could not find function " + functionName);
                    ResultType = "word";
                    return;
                }
                else if (function.parameterCount != ChildNodes.Count - 1)
                {
                    context.ReportError(this, "Incorrect number of arguments to function");
                    ResultType = "word";
                    return;
                }
                
                enclosingScope.activeFunction.function.Calls.Add(function);
                ResultType = function.returnType;
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            Assembly.Node r = target.target == Targets.Discard ? 
                (Assembly.Node)(new Assembly.StatementNode()) : new Assembly.TransientNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));

            for (int i = ChildNodes.Count - 1; i >= 1; --i)
                r.AddChild(Child(i).Emit(context, scope, Target.Stack));

            if (function == null)
            {
                var funcFetchToken = Child(0).GetFetchToken();
                if (funcFetchToken == null)
                {
                    var funcTarget = Target.Register(context.AllocateRegister());
                    r.AddChild(Child(0).Emit(context, scope, funcTarget));
                    funcFetchToken = Virtual(funcTarget.virtualId);
                }

                r.AddInstruction(Assembly.Instructions.JSR, funcFetchToken);
            }
            else
            {
                r.AddInstruction(Assembly.Instructions.JSR, Label(function.label));
            }

            if (ChildNodes.Count > 1)
                r.AddInstruction(Assembly.Instructions.ADD, Operand("SP"), Constant((ushort)(ChildNodes.Count - 1)));

            if (target.target != Targets.Discard)
                r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("A"));

            return r;
        }

    }
}
