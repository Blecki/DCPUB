using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class RootProgramNode : FunctionDeclarationNode
    {
        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            (Child(0) as BlockNode).bypass = true;

            footerLabel = Intermediate.Label.Make("main_footer");
            function = new Model.Function();
            function.localScope = enclosingScope;
            function.Node = this;
            enclosingScope.activeFunction = this;
            
            Child(0).GatherSymbols(context, function.localScope);
        }

        public override Intermediate.IRNode CompileFunction(CompileContext context)
        {
            // Only emit functions that can actually be called.
            function.MarkReachableFunctions();

            var r = new Intermediate.IRNode();
            var localScope = function.localScope.Push();

            r.AddInstruction(Instructions.SET, Operand("J"), Operand("SP"));
            var body = Child(0).Emit(context, localScope, Target.Discard);

            body.CollapseTransientNodes();
            body.PeepholeTree(context.peepholes);

            if (!context.options.emit_ir) body.AssignRegisters(null);

            r.AddChild(body);
            r.AddLabel(footerLabel);
            r.AddInstruction(Instructions.HLT); // Stop operation at end of program.
            r.AddInstruction(Instructions.SET, Operand("PC"), Label(footerLabel));

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));
            return r;
        }

    }
}
