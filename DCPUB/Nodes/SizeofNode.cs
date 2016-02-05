using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class SizeofNode : CompilableNode
    {
        public String typeName;
        public Struct _struct = null;
        public Assembly.Operand CachedFetchToken;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            typeName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override Assembly.Operand GetFetchToken()
        {
            if (CachedFetchToken == null) throw new InternalError("In sizeof, fetch token was not cached");
            return CachedFetchToken;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {              
            _struct = enclosingScope.FindType(typeName);
            if (_struct == null)
            {
                CachedFetchToken = Constant(0);
                context.ReportError(this, "Could not find type " + typeName);
            }
            else if (_struct.size == 0)
            {
                throw new InternalError("Struct size must be determined before types are resolved.");
            }
            else
                CachedFetchToken = Constant((ushort)_struct.size);

             ResultType = "word";
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            if (_struct.size == 0) throw new InternalError("Struct size not yet determined");
            r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push),
                Constant((ushort)_struct.size));
            return r;
        }

    }
    
}
