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

        public override Assembly.Node CompileFunction(CompileContext context)
        {
            var r = new Assembly.Node();
            var localScope = function.localScope.Push();

            r.AddChild(Child(0).Emit(context, localScope));
            r.AddLabel(footerLabel);
            r.AddInstruction(Assembly.Instructions.SUB, Operand("PC"), Constant(1));

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));
            return r;
        }

    }
}
