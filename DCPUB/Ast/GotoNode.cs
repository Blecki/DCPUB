using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

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

        public override Assembly.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            Label destination = null;
            foreach (var _label in scope.activeFunction.function.labels)
                if (_label.declaredName == label) destination = _label;
            if (destination == null) context.ReportError(this, "Unknown label - " + label);
            var r = new Assembly.StatementNode();
            if (destination != null)
                r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(destination.realName));
            return r;
        }

    }
}
