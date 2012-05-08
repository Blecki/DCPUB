using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
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
                case ClauseOrder.ConstantPass:
                    return Child(1);
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
            var topLabel = context.GetLabel() + "BEGIN_WHILE";
            r.AddLabel(topLabel);
            r.AddChild(base.Emit(context, scope));
            switch (clauseOrder)
            {
                case ClauseOrder.FailFirst:
                    {
                        var yesLabel = context.GetLabel() + "YES";
                        var endLabel = context.GetLabel() + "END_WHILE";
                        r.AddInstruction(Assembly.Instructions.SET, "PC", yesLabel);
                        r.AddInstruction(Assembly.Instructions.SET, "PC", endLabel);
                        r.AddLabel(yesLabel);
                        r.AddChild(EmitBlock(context, scope, Child(1)));
                        r.AddInstruction(Assembly.Instructions.SET, "PC", topLabel);
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
