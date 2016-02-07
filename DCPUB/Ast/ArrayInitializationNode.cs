using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class ArrayInitializationNode : CompilableNode
    {
        internal List<Intermediate.Operand> RawData;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var item in treeNode.ChildNodes[0].ChildNodes)
                AddChild("child", item);
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            RawData = new List<Intermediate.Operand>();

            for (int i = 0; i < ChildNodes.Count; ++i)
            {
                var _c = Child(i);
                if (_c == null) throw new InternalError("Failed sanity check: Array items not nodes?");
                _c.ResolveTypes(context, enclosingScope);
                var itemFetchToken = _c.GetFetchToken();
                if (itemFetchToken == null)
                {
                    RawData.Add(Constant(0));
                    context.ReportError(_c, "Array elements must be compile time constants.");
                }
                else if (itemFetchToken.IsIntegralConstant() || itemFetchToken.semantics == Intermediate.OperandSemantics.Label)
                    RawData.Add(itemFetchToken);
                else
                {
                    RawData.Add(Constant(0));
                    context.ReportError(_c, "Array elements must be compile time constants.");
                }
            }
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddChild(new Annotation("Array Initialization"));
            for (int i = 0; i < ChildNodes.Count; ++i)
                r.AddInstruction(Instructions.SET, Operand("PUSH"), Child(i).GetFetchToken());
            return r;
        }
    }
}
