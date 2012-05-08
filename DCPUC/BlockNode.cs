using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BlockNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var f in treeNode.ChildNodes)
                AddChild("Statement", f);
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            foreach (var child in ChildNodes)
                (child as CompilableNode).AssignRegisters(context,parentState, Register.DISCARD);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            foreach (var child in ChildNodes)
                r.AddChild((child as CompilableNode).Emit(context, scope));
            return r;
        }

    }
}
