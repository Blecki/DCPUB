using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class LabelNode : CompilableNode
    {
        public Model.Label label = new Model.Label();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            label.declaredName = treeNode.FirstChild.FindTokenAndGetText();
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            label.realName = Intermediate.Label.Make("_" + label.declaredName);
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            enclosingScope.activeFunction.function.labels.Add(label);
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddLabel(label.realName);
            return r;
        }

    }
}
