using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionDeclarationNode : CompilableNode
    {
        public Function function = null;
        public List<String> parameters = new List<string>();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Block", treeNode.ChildNodes[3]);
            foreach (var parameter in treeNode.ChildNodes[2].ChildNodes)
                parameters.Add(parameter.ChildNodes[0].FindTokenAndGetText());
            function = new Function();
            function.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            function.Node = this;
            function.parameterCount = parameters.Count;
            function.localScope = new Scope();
            function.localScope.type = ScopeType.Function;
            function.localScope.activeFunction = this;
        }

        public override string TreeLabel()
        {
            return "Function " + function.name + " " + function.parameterCount;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            function.label = context.GetLabel() + function.name;
            enclosingScope.functions.Add(function);
            function.localScope.parent = enclosingScope;

            for (int i = 0; i < parameters.Count; ++i)
            {
                var variable = new Variable();
                variable.scope = function.localScope;
                variable.name = parameters[i];
                function.localScope.variables.Add(variable);

                if (i < 3)
                {
                    variable.location = (Register)i;
                    function.localScope.UseRegister(i);
                }
                else
                {
                    variable.location = Register.STACK;
                    variable.stackOffset = function.localScope.stackDepth;
                    function.localScope.stackDepth += 1;
                }
            }

            Child(0).GatherSymbols(context, function.localScope);
        }

        public override AstNode FoldConstants()
        {
            base.FoldConstants();
            return null;
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            throw new CompileError("Function was not removed by Fold pass");
        }

        public virtual void CompileFunction(CompileContext context)
        {
            function.localScope.stackDepth += 1;
            var localScope = function.localScope.Push();

            context.Add(":" + function.label, "", "");
            Child(0).Compile(context, localScope, Register.DISCARD);
            CompileReturn(context, localScope);

            context.Barrier();

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.CompileFunction(context);
        }

        internal void CompileReturn(CompileContext context, Scope localScope)
        {
            if (localScope.stackDepth - function.localScope.stackDepth > 0)
                context.Add("ADD", "SP", Hex.hex(localScope.stackDepth - function.localScope.stackDepth), "Cleanup stack"); 
            context.Add("SET", "PC", "POP", "Return");
        }
    }
}
