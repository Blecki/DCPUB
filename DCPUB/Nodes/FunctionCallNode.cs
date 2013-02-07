﻿using System;
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
        RegisterState[] activeRegisters = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            functionName = null;//treeNode.ChildNodes[0].FindTokenAndGetText();
            AddChild("expression", treeNode.ChildNodes[0]);//.FirstChild);
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        public override string TreeLabel()
        {
            return "call " + functionName + " " + ResultType + " [into:" + target.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            if (Child(0) is VariableNameNode)
            {
                try
                {
                    Child(0).GatherSymbols(context, enclosingScope);
                }
                catch (CompileError e)
                {
                    functionName = (Child(0) as VariableNameNode).variableName;
                }
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

                if (function == null) throw new CompileError("Could not find function " + functionName);
                if (function.parameterCount != ChildNodes.Count - 1) throw new CompileError("Incorrect number of arguments to function");

                ResultType = function.returnType;
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;

            activeRegisters = parentState.SaveRegisterState();

            if (function == null) Child(0).AssignRegisters(context, parentState, Register.STACK);

            for (int i = 1; i < ChildNodes.Count; ++i)
                Child(i).AssignRegisters(context, parentState, Register.STACK);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            Assembly.Node r = null;
            if (target == Register.DISCARD)
            {
                r = new Assembly.StatementNode();
                r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            }
            else
                r = new Assembly.ExpressionNode();

            for (int i = ChildNodes.Count - 1; i >= 1; --i)
                r.AddChild(Child(i).Emit(context, scope));
            
            if (function == null)
            {
                if (Child(0).IsIntegralConstant())
                    r.AddInstruction(Assembly.Instructions.JSR, Constant((ushort)Child(0).GetConstantValue()));
                else
                {
                    var fetchToken = Child(0).GetFetchToken(scope);
                    if (fetchToken != null)
                        r.AddInstruction(Assembly.Instructions.JSR, fetchToken);
                    else
                    {
                        r.AddChild(Child(0).Emit(context, scope));
                        r.AddInstruction(Assembly.Instructions.JSR, Operand("POP"));
                    }
                }
            }
            else
            {
                r.AddInstruction(Assembly.Instructions.JSR, Label(function.label));
            }

             r.AddInstruction(Assembly.Instructions.ADD, Operand("SP"), Constant((ushort)(ChildNodes.Count - 1)));

             if (target != Register.A && target != Register.DISCARD)
                 r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)), Operand("A"));

            return r;
        }
    }
}