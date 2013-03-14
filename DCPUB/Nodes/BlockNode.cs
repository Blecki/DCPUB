using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class BlockNode : CompilableNode
    {
        public Assembly.Label breakLabel = null;
        public Assembly.Label continueLabel = null;
        public Scope blockScope;
        public bool bypass = true;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var f in treeNode.ChildNodes)
                AddChild("Statement", f);
        }

        public static Irony.Interpreter.Ast.AstNode Wrap(CompilableNode node)
        {
            if (node is BlockNode)
            {
                (node as BlockNode).bypass = false;
                return node;
            }

            var r = new BlockNode();
            r.ChildNodes.Add(node);
            r.bypass = false;
            return r;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            if (bypass) base.GatherSymbols(context, enclosingScope);
            else
            {
                blockScope = enclosingScope.Push();
                base.GatherSymbols(context, blockScope);
            }
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, bypass ? enclosingScope : blockScope);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();

            //r.AddChild(new Assembly.Annotation("Entering blocknode emit"));
            if (bypass)
            {
                //r.AddChild(new Assembly.Annotation("bypassed"));

                foreach (var child in ChildNodes)
                    r.AddChild((child as CompilableNode).Emit(context, scope, Target.Discard));
            }
            else
            {
                var localScope = scope.Push(blockScope);
                localScope.activeBlock = this;
                foreach (var child in ChildNodes)
                    r.AddChild((child as CompilableNode).Emit(context, localScope, Target.Discard));
                //if (breakLabel != null) r.AddLabel(breakLabel);
                if (blockScope.variablesOnStack - scope.variablesOnStack > 0)
                    r.AddInstruction(Assembly.Instructions.ADD, Operand("SP"), Constant((ushort)(blockScope.variablesOnStack - scope.variablesOnStack)));
            }
            //r.AddChild(new Assembly.Annotation("Leaving blocknode emit"));

            return r;
        }
    }
}
