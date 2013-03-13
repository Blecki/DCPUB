using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class WhileStatementNode : BranchStatementNode
    {
        private Irony.Parsing.SourceSpan headerSpan;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
            AddChild("Block", treeNode.ChildNodes[2]);
            this.AsString = "While";
            headerSpan = new Irony.Parsing.SourceSpan(this.Span.Location,
                treeNode.ChildNodes[1].FirstChild.Span.EndPosition - this.Span.Location.Position);
            ChildNodes[1] = BlockNode.Wrap(Child(1));
            (Child(1) as BlockNode).bypass = false;
        }

        public override string TreeLabel()
        {
            return "while " + base.TreeLabel();
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantFail:
                    return null;
                default:
                    return this;
            }
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(headerSpan)));
            var topLabel = Assembly.Label.Make("BEGIN_WHILE");
            (Child(1) as BlockNode).continueLabel = topLabel;
            
            r.AddLabel(topLabel);
            r.AddChild(base.Emit(context, scope));
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantPass:
                    {
                        var endLabel = Assembly.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddChild(EmitBlock(context, scope, Child(1)));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                case ClauseOrder.FailFirst:
                    {
                        var yesLabel = Assembly.Label.Make("YES");
                        var endLabel = Assembly.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(yesLabel));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(endLabel));
                        r.AddLabel(yesLabel);
                        r.AddChild(EmitBlock(context, scope, Child(1)));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                default:
                    throw new CompileError("WHILE !FailFirst Not implemented");
            }
            return r;
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            var r = new Assembly.TransientNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(headerSpan)));
            var topLabel = Assembly.Label.Make("BEGIN_WHILE");
            (Child(1) as BlockNode).continueLabel = topLabel;

            r.AddLabel(topLabel);
            r.AddChild(base.Emit2(context, scope, target));
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantPass:
                    {
                        var endLabel = Assembly.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddChild(EmitBlock2(context, scope, Child(1)));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                case ClauseOrder.ConstantFail:
                    {
                        var endLabel = Assembly.Label.Make("END_WHILE");
                        r.AddLabel(endLabel);
                    }
                    break;
                case ClauseOrder.FailFirst:
                    {
                        var yesLabel = Assembly.Label.Make("YES");
                        var endLabel = Assembly.Label.Make("END_WHILE");
                        (Child(1) as BlockNode).breakLabel = endLabel;
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(yesLabel));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(endLabel));
                        r.AddLabel(yesLabel);
                        r.AddChild(EmitBlock2(context, scope, Child(1)));
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(topLabel));
                        r.AddLabel(endLabel);
                    }
                    break;
                default:
                    throw new CompileError("WHILE !FailFirst Not implemented");
            }
            return r;
        }

    }

    
}
