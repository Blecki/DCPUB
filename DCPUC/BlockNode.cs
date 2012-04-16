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

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            foreach (var child in ChildNodes)
                (child as CompilableNode).AssignRegisters(parentState, Register.DISCARD);
        }

        public override void Emit(CompileContext assembly, Scope scope)
        {
            foreach (var child in ChildNodes)
            {
                assembly.Barrier();
                //assembly.AddSource(child.Span);
                (child as CompilableNode).Emit(assembly, scope);
            }


        }

    }
}
