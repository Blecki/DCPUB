using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class OffsetOfNode : CompilableNode
    {
        public String typeName;
        public String memberName;
        public Struct _struct = null;
        public Intermediate.Operand CachedFetchToken;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            typeName = treeNode.ChildNodes[3].FindTokenAndGetText();
            memberName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override Intermediate.Operand GetFetchToken()
        {
            if (CachedFetchToken == null) throw new InternalError("In offsetof, fetch token was not cached");
            return CachedFetchToken;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            _struct = enclosingScope.FindType(typeName);

            if (_struct == null)
            {
                context.ReportError(this, "Could not find type " + typeName);
                CachedFetchToken = Constant(0);
            }
            else
            {
                var memberIndex = _struct.members.FindIndex(m => m.name == memberName);
                if (memberIndex < 0)
                {
                    context.ReportError(this, "Member not found : " + memberName);
                    CachedFetchToken = Constant(0);
                }
                else
                    CachedFetchToken = Constant((ushort)memberIndex);
            }

            ResultType = "word";
        }

        public override Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new TransientNode();
            r.AddInstruction(Instructions.SET, target.GetOperand(TargetUsage.Push), GetFetchToken());
            return r;
        }

    }
    
}
