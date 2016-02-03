using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class OffsetOfNode : CompilableNode
    {
        public String typeName;
        public String memberName;
        public Struct _struct = null;
        public bool IsAssignedTo { get; set; }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            typeName = treeNode.ChildNodes[3].FindTokenAndGetText();
            memberName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override Assembly.Operand GetFetchToken()
        {
            if (_struct == null) throw new CompileError(this, "Struct not found : " + typeName);
            var memberIndex = _struct.members.FindIndex(m => m.name == memberName);
            if (memberIndex < 0) throw new CompileError(this, "Member not found : " + memberName);
            return Constant((ushort)memberIndex);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            _struct = enclosingScope.FindType(typeName);
            if (_struct == null) throw new CompileError(this, "Could not find type " + typeName);
            ResultType = "word";
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            r.AddInstruction(Assembly.Instructions.SET, target.GetOperand(TargetUsage.Push), GetFetchToken());
            return r;
        }

    }
    
}
