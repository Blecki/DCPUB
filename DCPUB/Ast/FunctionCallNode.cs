﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class FunctionCallNode : CompilableNode
    {
        Model.Function function;
        String functionName;
        Model.Scope enclosingScope;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            functionName = null;//treeNode.ChildNodes[0].FindTokenAndGetText();
            AddChild("expression", treeNode.ChildNodes[0]);//.FirstChild);
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
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

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
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

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            Intermediate.IRNode r = target.target == Targets.Discard ? 
                (Intermediate.IRNode)(new StatementNode()) : new TransientNode();
            r.AddChild(new Annotation(context.GetSourceSpan(this.Span)));

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

                r.AddInstruction(Instructions.JSR, funcFetchToken);
            }
            else
            {
                r.AddInstruction(Instructions.JSR, Label(function.label));
            }

            if (ChildNodes.Count > 1)
                r.AddInstruction(Instructions.ADD, Operand("SP"), Constant((ushort)(ChildNodes.Count - 1)));

            if (target.target != Targets.Discard)
                r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), Operand("A"));

            return r;
        }

    }
}
