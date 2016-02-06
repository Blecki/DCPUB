using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class GotoNode : CompilableNode
    {
        public String label;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            label = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            Label destination = null;
            foreach (var _label in scope.activeFunction.function.labels)
                if (_label.declaredName == label) destination = _label;
            if (destination == null) context.ReportError(this, "Unknown label - " + label);
            var r = new StatementNode();
            if (destination != null)
                r.AddInstruction(Instructions.SET, Operand("PC"), Label(destination.realName));
            return r;
        }

    }
}
