using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class RootProgramNode : FunctionDeclarationNode
    {
        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            (Child(0) as BlockNode).bypass = true;

            footerLabel = Assembly.Label.Make("main_footer");
            function = new Function();
            function.localScope = enclosingScope;
            function.Node = this;
            enclosingScope.activeFunction = this;
            
            Child(0).GatherSymbols(context, function.localScope);
        }

        public override Assembly.Node CompileFunction(CompileContext context)
        {
            // Only emit functions that can actually be called.
            function.MarkReachableFunctions();

            var r = new Assembly.Node();
            var localScope = function.localScope.Push();

            r.AddInstruction(Assembly.Instructions.SET, Operand("J"), Operand("SP"));
            var body = Child(0).Emit(context, localScope, Target.Discard);
            body.CollapseTree(context.peepholes);
            if (!context.options.emit_ir) AssignVirtualRegisters(body);
            r.AddChild(body);
            r.AddLabel(footerLabel);
            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(footerLabel));

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));
            return r;
        }

    }
}
