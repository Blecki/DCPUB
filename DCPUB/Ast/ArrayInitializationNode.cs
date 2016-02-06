using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class ArrayInitializationNode : CompilableNode
    {
        internal List<Assembly.Operand> RawData;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var item in treeNode.ChildNodes[0].ChildNodes)
                AddChild("child", item);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            RawData = new List<Assembly.Operand>();

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
                else if (itemFetchToken.IsIntegralConstant() || itemFetchToken.semantics == Assembly.OperandSemantics.Label)
                    RawData.Add(itemFetchToken);
                else
                {
                    RawData.Add(Constant(0));
                    context.ReportError(_c, "Array elements must be compile time constants.");
                }
            }
        }

        public override Assembly.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation("Array Initialization"));
            for (int i = 0; i < ChildNodes.Count; ++i)
                r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Child(i).GetFetchToken());
            return r;
        }
    }
}
