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
        //public List<Variable> parameters = new List<Variable>();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Block", treeNode.ChildNodes[3]);

            var parameters = treeNode.ChildNodes[2].ChildNodes;

            for (int i = 0; i < parameters.Count; ++i)
            {
                var variable = new Variable();
                variable.scope = localScope;
                variable.name = parameters[i].ChildNodes[0].FindTokenAndGetText();
                localScope.variables.Add(variable);

                if (i < 3)
                {
                    variable.location = (Register)i;
                    localScope.UseRegister(i);
                }
                else
                {
                    variable.location = Register.STACK;
                    variable.stackOffset = localScope.stackDepth;
                    localScope.stackDepth += 1;
                }

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
                assembly.Add("ADD", "SP", hex(localScope.stackDepth - parameterCount - 1), "Cleanup stack"); 
            assembly.Add("SET", "PC", "POP", "Return");
        }
    }
}
