using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class LabelNode : CompilableNode
    {
        public Label label = new Label();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            label.declaredName = treeNode.FirstChild.FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            label.realName = Assembly.Label.Make("_" + label.declaredName);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            enclosingScope.activeFunction.function.labels.Add(label);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.StatementNode();
            r.AddLabel(label.realName);
            return r;
        }

    }
}
