﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionCallNode : CompilableNode
    {
        Function function;
        String functionName;
        Register target;
        Scope enclosingScope;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            functionName = treeNode.ChildNodes[0].FindTokenAndGetText();
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        public override string TreeLabel()
        {
            return "call " + functionName + " " + ResultType + " [into:" + target.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            this.enclosingScope = enclosingScope;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            var func_scope = enclosingScope;
            while (function == null && func_scope != null)
            {
                foreach (var v in func_scope.functions)
                    if (v.name == functionName)
                        function = v;
                if (function == null) func_scope = func_scope.parent;
            }

            if (function == null) throw new CompileError("Could not find function " + functionName);
            if (function.parameterCount != ChildNodes.Count) throw new CompileError("Incorrect number of arguments to function");
            for (int i = 0; i < function.parameterCount; ++i)
            {
                if (function.localScope.variables[i].typeSpecifier != Child(i).ResultType)
                    context.AddWarning(Span, CompileContext.TypeWarning(Child(i).ResultType, function.localScope.variables[i].typeSpecifier));
            }

            ResultType = function.returnType;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;

            

            var startingRegisterState = parentState.SaveRegisterState();

            for (int i = 0; i < 3 && i < function.parameterCount; ++i)
                Child(i).AssignRegisters(context, parentState, (Register)i);
            for (int i = 3; i < function.parameterCount; ++i)
                Child(i).AssignRegisters(context, parentState, Register.STACK);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            if (target == Register.DISCARD) r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            for (int i = 0; i < 3; ++i)
            {
                if (scope.activeFunction.usedRegisters.registers[i] == RegisterState.Used)
                {
                    r.AddInstruction(Assembly.Instructions.SET, "PUSH", Scope.GetRegisterLabelSecond((int)i));
                    scope.stackDepth += 1;
                    if (scope.activeFunction.function.parameterCount > i)
                    {
                        scope.activeFunction.function.localScope.variables[i].location = Register.STACK;
                        scope.activeFunction.function.localScope.variables[i].stackOffset = scope.stackDepth - 1;
                    }
                }
                if (ChildNodes.Count > i)
                    r.AddChild(Child(i).Emit(context, scope));
            }

            for (int i = ChildNodes.Count - 1; i >= 3; --i)
            {
                r.AddChild(Child(i).Emit(context, scope));
                scope.stackDepth += 1;
            }

            r.AddInstruction(Assembly.Instructions.JSR, function.label);

            if (ChildNodes.Count > 3) //Need to remove parameters from stack
            {
                r.AddInstruction(Assembly.Instructions.ADD, "SP", Hex.hex(ChildNodes.Count - 3));
                scope.stackDepth -= (ChildNodes.Count - 3);
            }

            var saveA = scope.activeFunction.usedRegisters.registers[0] == RegisterState.Used;
            if (saveA && target != Register.DISCARD) r.AddInstruction(Assembly.Instructions.SET, Scope.TempRegister, "A");

            for (int i = 2; i >= 0; --i)
                if (scope.activeFunction.usedRegisters.registers[i] == RegisterState.Used)
                {
                    r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst(i), "POP");
                    scope.stackDepth -= 1;
                    if (scope.activeFunction.function.parameterCount > i)
                        scope.activeFunction.function.localScope.variables[i].location = (Register)i;
                }

            if (target == Register.A && !saveA) return r;
            else if (Scope.IsRegister(target)) r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), saveA ? Scope.TempRegister : "A");
            else if (target == Register.STACK)
            {
                r.AddInstruction(Assembly.Instructions.SET, "PUSH", saveA ? Scope.TempRegister : "A");
                scope.stackDepth += 1;
            }
            return r;
        }

        

        
    }
}
