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

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var func = findFunction(this, AsString);
            if (func == null) throw new CompileError("Can't find function - " + AsString);
            if (func.parameterCount != ChildNodes.Count) throw new CompileError("Incorrect number of arguments - " + AsString);
            //Marshall registers
            var startingRegisterState = scope.SaveRegisterState();
            for (int i = 1; i < 8; ++i)
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    assembly.Add("SET", "PUSH", Scope.GetRegisterLabelSecond(i), "Saving register");
                    scope.FreeRegister(i);
                    scope.stackDepth += 1;
                }
            foreach (var child in ChildNodes)
                (child as CompilableNode).Compile(assembly, scope, Register.STACK);
            assembly.Add("JSR", func.label, "", "Calling function");
            scope.stackDepth -= func.parameterCount; //Pushed parameters onto the stack, REMEMBER?
            //unmarshall registers
            for (int i = 7; i > 0; --i)
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    assembly.Add("SET", Scope.GetRegisterLabelFirst(i), "POP", "Restoring register");
                    scope.UseRegister(i);
                    scope.stackDepth -= 1;
                }
            if (target == Register.A) return;
            else if (Scope.IsRegister(target)) assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "A");
            else if (target == Register.STACK)
            {
                assembly.Add("SET", "PUSH", "A", "Put return value on stack");
                scope.stackDepth += 1;
            }
        }

        
    }
}
