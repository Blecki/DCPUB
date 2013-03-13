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
            if (_struct == null) throw new CompileError(this, "Unknown type.");
            ResultType = typeName;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            Child(0).ResultType = ResultType;
            return Child(0);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError(this, "Cast was not folded.");
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            return Child(0).Emit2(context, scope, target);
        }

    }

    
}
