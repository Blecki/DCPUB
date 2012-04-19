using System;
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
            return "call " + functionName + " [into:" + target.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            this.enclosingScope = enclosingScope;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;

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


            var startingRegisterState = parentState.SaveRegisterState();

            for (int i = 0; i < 3 && i < function.parameterCount; ++i)
                Child(i).AssignRegisters(context, parentState, (Register)i);
            for (int i = 3; i < function.parameterCount; ++i)
                Child(i).AssignRegisters(context, parentState, Register.STACK);
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (scope.activeFunction.usedRegisters.registers[i] == RegisterState.Used)
                {
                    context.Add("SET", "PUSH", Scope.GetRegisterLabelSecond((int)i), "Saving register");
                    scope.stackDepth += 1;
                    if (scope.activeFunction.function.parameterCount > i)
                    {
                        scope.activeFunction.function.localScope.variables[i].location = Register.STACK;
                        scope.activeFunction.function.localScope.variables[i].stackOffset = scope.stackDepth - 1;
                    }
                }
                if (ChildNodes.Count > i)
                    Child(i).Emit(context, scope); //Should already be targetting i.
            }

            for (int i = 3; i < ChildNodes.Count; ++i)
            {
                Child(i).Emit(context, scope);
                scope.stackDepth += 1;
            }

            context.Add("JSR", function.label, "", "Calling function");

            if (ChildNodes.Count > 3) //Need to remove parameters from stack
            {
                context.Add("ADD", "SP", Hex.hex(ChildNodes.Count - 3), "Remove parameters");
                scope.stackDepth -= (ChildNodes.Count - 3);
            }

            var saveA = scope.activeFunction.usedRegisters.registers[0] == RegisterState.Used;
            if (saveA && target != Register.DISCARD) context.Add("SET", Scope.TempRegister, "A");

            for (int i = 2; i >= 0; --i)
                if (scope.activeFunction.usedRegisters.registers[i] == RegisterState.Used)
                {
                    context.Add("SET", Scope.GetRegisterLabelFirst(i), "POP");
                    scope.stackDepth -= 1;
                    if (scope.activeFunction.function.parameterCount > i)
                        scope.activeFunction.function.localScope.variables[i].location = (Register)i;
                }

            if (target == Register.A && !saveA) return;
            else if (Scope.IsRegister(target)) context.Add("SET", Scope.GetRegisterLabelFirst((int)target), saveA ? Scope.TempRegister : "A");
            else if (target == Register.STACK)
            {
                context.Add("SET", "PUSH", saveA ? Scope.TempRegister : "A", "Put return value on stack");
                scope.stackDepth += 1;
            }
        }

        

        
    }
}
