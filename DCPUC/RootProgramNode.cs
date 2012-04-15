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
            footerLabel = context.GetLabel() + "main_footer";
            function = new Function();
            function.localScope = enclosingScope;
            function.Node = this;
            enclosingScope.activeFunction = this;
            
            Child(0).GatherSymbols(context, function.localScope);
        }

        public override void CompileFunction(CompileContext context)
        {
            var localScope = function.localScope.Push();

            Child(0).Emit(context, localScope);
            context.Add(":" + footerLabel, "", "");
            context.Add("BRK", "", "");

            context.Barrier();

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.CompileFunction(context);
        }

    }
}
