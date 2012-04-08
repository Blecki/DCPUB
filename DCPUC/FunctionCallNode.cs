using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionCallNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = treeNode.ChildNodes[0].FindTokenAndGetText();
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        private static FunctionDeclarationNode findFunction(AstNode node, string name)
        {
            foreach (var child in node.ChildNodes)
                if (child is FunctionDeclarationNode && (child as FunctionDeclarationNode).AsString == name)
                    return child as FunctionDeclarationNode;
            if (node.Parent != null) return findFunction(node.Parent, name);
            return null;
        }

        private static void PushRegister(Assembly assembly, Scope scope, Register r)
        {
            assembly.Add("SET", "PUSH", Scope.GetRegisterLabelSecond((int)r), "Saving register");
            scope.FreeRegister(0);
            scope.stackDepth += 1;
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var func = findFunction(this, AsString);
            if (func == null) throw new CompileError("Can't find function - " + AsString);
            if (func.parameterCount != ChildNodes.Count) throw new CompileError("Incorrect number of arguments - " + AsString);
            func.references += 1;

            //Marshall registers
            var startingRegisterState = scope.SaveRegisterState();

            for (int i = 0; i < 3; ++i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    PushRegister(assembly, scope, (Register)i);
                    if (scope.activeFunction != null && scope.activeFunction.parameterCount > i)
                    {
                        scope.activeFunction.localScope.variables[i].location = Register.STACK;
                        scope.activeFunction.localScope.variables[i].stackOffset = scope.stackDepth - 1;
                    }
                }
                if (func.parameterCount > i)
                {
                    scope.UseRegister(i);
                    (ChildNodes[i] as CompilableNode).Compile(assembly, scope, (Register)i);
                }
            }

            for (int i = 3; i < 7; ++i)
                if (startingRegisterState[i] == RegisterState.Used)
                    PushRegister(assembly, scope, (Register)i);

            if (func.parameterCount > 3)
                for (int i = 3; i < func.parameterCount; ++i)
                    (ChildNodes[i] as CompilableNode).Compile(assembly, scope, Register.STACK);

            assembly.Add("JSR", func.label, "", "Calling function");

            if (func.parameterCount > 3) //Need to remove parameters from stack
            {
                assembly.Add("ADD", "SP", hex(func.parameterCount - 3), "Remove parameters");
                scope.stackDepth -= (func.parameterCount - 3);
            }

            if (scope.activeFunction != null)
                for (int i = 0; i < 3 && i < scope.activeFunction.parameterCount; ++i)
                    scope.activeFunction.localScope.variables[i].location = (Register)i;

            var saveA = startingRegisterState[0] == RegisterState.Used;
            if (saveA && target != Register.DISCARD) assembly.Add("SET", Scope.TempRegister, "A", "Save return value from being overwritten by stored register");

            for (int i = 6; i >= 0; --i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    assembly.Add("SET", Scope.GetRegisterLabelFirst(i), "POP", "Restoring register");
                    scope.UseRegister(i);
                    scope.stackDepth -= 1;
                }
            }

            if (target == Register.A && !saveA) return;
            else if (Scope.IsRegister(target)) assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), saveA ? Scope.TempRegister : "A");
            else if (target == Register.STACK)
            {
                assembly.Add("SET", "PUSH", saveA ? Scope.TempRegister : "A", "Put return value on stack");
                scope.stackDepth += 1;
            }
        }

        

        
    }
}
