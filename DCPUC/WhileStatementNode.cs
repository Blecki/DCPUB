using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class WhileStatementNode : BranchStatementNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
            AddChild("Block", treeNode.ChildNodes[2]);
            this.AsString = "While";
        }

        public override string TreeLabel()
        {
            return "while " + base.TreeLabel();
        }

        public override CompilableNode FoldConstants()
        {
            base.FoldConstants();
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

        public override void Emit(CompileContext context, Scope scope)
        {
            var topLabel = context.GetLabel() + "BEGIN_WHILE";
            context.Add(":" + topLabel, "", "");
            base.Emit(context, scope);
            switch (clauseOrder)
            {
                case ClauseOrder.FailFirst:
                    {
                        var yesLabel = context.GetLabel() + "YES";
                        var endLabel = context.GetLabel() + "END_WHILE";
                        context.Add("SET", "PC", yesLabel);
                        context.Add("SET", "PC", endLabel);
                        context.Add(":" + yesLabel, "", "");
                        EmitBlock(context, scope, Child(1));
                        context.Add("SET", "PC", topLabel);
                        context.Add(":" + endLabel, "", "");
                    }
                    break;
                default:
                    throw new CompileError("WHILE !FailFirst Not implemented");
            }
        }

    }

    
}
