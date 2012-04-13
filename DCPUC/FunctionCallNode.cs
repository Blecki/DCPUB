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

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            functionName = treeNode.ChildNodes[0].FindTokenAndGetText();
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            
            foreach (CompilableNode child in ChildNodes) child.GatherSymbols(context, enclosingScope);
        }

        private static void PushRegister(CompileContext context, Scope scope, Register r)
        {
            context.Add("SET", "PUSH", Scope.GetRegisterLabelSecond((int)r), "Saving register");
            scope.FreeRegister(0);
            scope.stackDepth += 1;
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            //Lookup function : Waiting until compile times allows us to dispense with function prototypes.
            var func_scope = scope;
            while (function == null && func_scope != null)
            {
                foreach (var v in func_scope.functions)
                    if (v.name == functionName)
                        function = v;
                if (function == null) func_scope = func_scope.parent;
            }

            if (function == null) throw new CompileError("Could not find function " + functionName);
            if (function.parameterCount != ChildNodes.Count) throw new CompileError("Incorrect number of arguments to function");
            
            //Marshall registers : New calling convention (when implemented) would require only marshalling A, B, C.
            var startingRegisterState = scope.SaveRegisterState();

            for (int i = 0; i < 3; ++i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    PushRegister(context, scope, (Register)i);
                    if (scope.activeFunction != null && scope.activeFunction.function.parameterCount > i)
                    {
                        scope.activeFunction.function.localScope.variables[i].location = Register.STACK;
                        scope.activeFunction.function.localScope.variables[i].stackOffset = scope.stackDepth - 1;
                    }
                }
                if (function.parameterCount > i)
                {
                    scope.UseRegister(i);
                    (ChildNodes[i] as CompilableNode).Compile(context, scope, (Register)i);
                }
            }

            for (int i = 3; i < 7; ++i)
                if (startingRegisterState[i] == RegisterState.Used)
                    PushRegister(context, scope, (Register)i);

            if (function.parameterCount > 3)
                for (int i = 3; i < function.parameterCount; ++i)
                    (ChildNodes[i] as CompilableNode).Compile(context, scope, Register.STACK);

            context.Add("JSR", function.label, "", "Calling function");

            if (function.parameterCount > 3) //Need to remove parameters from stack
            {
                context.Add("ADD", "SP", Hex.hex(function.parameterCount - 3), "Remove parameters");
                scope.stackDepth -= (function.parameterCount - 3);
            }

            if (scope.activeFunction != null)
                for (int i = 0; i < 3 && i < scope.activeFunction.function.parameterCount; ++i)
                    scope.activeFunction.function.localScope.variables[i].location = (Register)i;

            var saveA = startingRegisterState[0] == RegisterState.Used;
            if (saveA && target != Register.DISCARD) context.Add("SET", Scope.TempRegister, "A", "Save return value from being overwritten by stored register");

            for (int i = 6; i >= 0; --i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    context.Add("SET", Scope.GetRegisterLabelFirst(i), "POP", "Restoring register");
                    scope.UseRegister(i);
                    scope.stackDepth -= 1;
                }
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
