using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class BreakNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = "break";
        }

        public static BlockNode FindParentBlock(Scope scope)
        {
            if (scope == null) return null;
            if (scope.activeBlock == null) return FindParentBlock(scope.parent);
            if (scope.activeBlock.bypass) return FindParentBlock(scope.parent);

            //If there's only one statement in the block, it's this break statement.
            if (scope.activeBlock.ChildNodes.Count == 1) return FindParentBlock(scope.parent);
            return scope.activeBlock;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new TransientNode();

            var activeBlock = FindParentBlock(scope);

            if (activeBlock == null) context.ReportError(this, "Break not valid here.");
            else if (activeBlock.breakLabel == null) context.ReportError(this, "Break not valid here.");
            else
            {
                if (activeBlock.blockScope.parent.variablesOnStack < scope.variablesOnStack)
                    r.AddInstruction(Instructions.ADD, Operand("SP"), Constant(
                        (ushort)(scope.variablesOnStack - activeBlock.blockScope.parent.variablesOnStack)));
                r.AddInstruction(Instructions.SET, Operand("PC"), Label(activeBlock.breakLabel));
            }

            return r;
        }

    }
}
