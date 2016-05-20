using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB.Ast
{
    public class WhileStatementNode : BranchStatementNode
    {
        private Irony.Parsing.SourceSpan headerSpan;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            AddChild("Block", treeNode.ChildNodes[2]);
            this.AsString = "While";
            headerSpan = new Irony.Parsing.SourceSpan(this.Span.Location,
                treeNode.ChildNodes[1].Span.EndPosition - this.Span.Location.Position);
            ChildNodes[1] = BlockNode.Wrap(Child(1));
            (Child(1) as BlockNode).bypass = false;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new TransientNode();
            r.AddChild(new Annotation(context.GetSourceSpan(headerSpan)));
            var topLabel = Intermediate.Label.Make("BEGIN_WHILE");
            (Child(1) as BlockNode).continueLabel = topLabel;

            r.AddLabel(topLabel);
            r.AddChild(base.Emit(context, scope, target));
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantPass:
                    {
                        var endLabel = Intermediate.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddChild(EmitBlock(context, scope, Child(1)));
                        r.AddInstruction(Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                case ClauseOrder.ConstantFail:
                    {
                        var endLabel = Intermediate.Label.Make("END_WHILE");
                        r.AddLabel(endLabel);
                    }
                    break;
                case ClauseOrder.FailFirst:
                    {
                        var yesLabel = Intermediate.Label.Make("YES");
                        var endLabel = Intermediate.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddInstruction(Instructions.SET, Operand("PC"), Label(yesLabel));
                        r.AddInstruction(Instructions.SET, Operand("PC"), Label(endLabel));
                        r.AddLabel(yesLabel);
                        r.AddChild(EmitBlock(context, scope, Child(1)));
                        r.AddInstruction(Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                default:
                    throw new InternalError("WHILE !FailFirst Not implemented");
            }
            return r;
        }

    }

    
}
