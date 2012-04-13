using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class RootProgramNode : FunctionDeclarationNode
    {
        public override string TreeLabel()
        {
            return "Root";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            function = new Function();
            function.localScope = enclosingScope;
            function.Node = this;
            enclosingScope.activeFunction = this;
            
            Child(0).GatherSymbols(context, function.localScope);
        }

        public override void CompileFunction(CompileContext context)
        {
            var localScope = function.localScope.Push();

            Child(0).Compile(context, localScope, Register.DISCARD);
            CompileReturn(context, localScope);

            context.Barrier();

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.CompileFunction(context);
        }

        internal void CompileReturn(CompileContext context, Scope localScope)
        {
            context.Add("BRK", "", "");
        }
    }
}
