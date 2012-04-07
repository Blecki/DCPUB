using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionDeclarationNode : CompilableNode
    {
        public Scope localScope = new Scope();
        public String label;
        public int parameterCount = 0;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Block", treeNode.ChildNodes[3]);

            foreach (var parameter in treeNode.ChildNodes[2].ChildNodes)
            {
                var variable = new Variable();
                variable.scope = localScope;
                variable.name = parameter.ChildNodes[0].FindTokenAndGetText();
                variable.stackOffset = localScope.stackDepth;
                localScope.variables.Add(variable);
                localScope.stackDepth += 1;
                parameterCount += 1;
                
            }

            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
            label = Scope.GetLabel() + "_" + AsString;
            localScope.activeFunction = this;
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            scope.pendingFunctions.Add(this);
        }

        public void CompileFunction(Assembly assembly)
        {
            assembly.Add(":" + label, "", "");
            assembly.Barrier();
            localScope.stackDepth += 1; //account for return address
            (ChildNodes[0] as CompilableNode).Compile(assembly, localScope, Register.DISCARD);
            CompileReturn(assembly);
            assembly.Barrier();
            //Should leave the return value, if any, in A.
            foreach (var function in localScope.pendingFunctions)
                function.CompileFunction(assembly);
        }

        internal void CompileReturn(Assembly assembly)
        {
            if (localScope.stackDepth - parameterCount > 1)
                assembly.Add("ADD", "SP", hex(localScope.stackDepth - parameterCount - 1));
            assembly.Add("SET", "B", "POP", "Get return value");
            if (parameterCount > 0) assembly.Add("ADD", "SP", hex(parameterCount), "Remove parameters");
            assembly.Add("SET", "PC", "B", "Return");
        }
    }
}
