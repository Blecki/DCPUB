using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class CastNode : CompilableNode
    {
        public String typeName;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            typeName = treeNode.ChildNodes[1].FindTokenAndGetText();
            AddChild("Expression", treeNode.ChildNodes[0]);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            var _struct = enclosingScope.FindType(typeName);
            if (_struct == null) context.ReportError(this, "Cast to unknown type - " + typeName);
            ResultType = typeName;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            return Child(0).Emit(context, scope, target);
        }

    }

    
}
