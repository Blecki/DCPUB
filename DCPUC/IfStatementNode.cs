using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class IfStatementNode : BranchStatementNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.Term.Name == "IfElse")
            {
                AddChild("condition", treeNode.ChildNodes[0].ChildNodes[1].FirstChild);
                AddChild("then", treeNode.ChildNodes[0].ChildNodes[2]);
                AddChild("ELSE", treeNode.ChildNodes[2]);
            }
            else
            {
                AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
                AddChild("Block", treeNode.ChildNodes[2]);
                if (treeNode.ChildNodes.Count == 5) AddChild("Else", treeNode.ChildNodes[4]);
            }
            this.AsString = "If";
        }

        public override string TreeLabel()
        {
            return "if " + base.TreeLabel();
        }

        public override CompilableNode FoldConstants()
        {
            base.FoldConstants();
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantPass:
                    return Child(1);
                case ClauseOrder.ConstantFail:
                    return ChildNodes.Count > 2 ? Child(2) : null;
                default:
                    return this;
            }
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            base.Emit(context, scope);
            switch (clauseOrder)
            {
                case ClauseOrder.FailFirst: //Only actual valid order.
                    {
                        var thenLabel = context.GetLabel() + "THEN";
                        var endLabel = context.GetLabel() + "END";

                        context.Add("SET", "PC", thenLabel);
                        if (ChildNodes.Count == 3) EmitBlock(context, scope, Child(2));
                        context.Add("SET", "PC", endLabel);
                        context.Add(":" + thenLabel, "", "");

                        EmitBlock(context, scope, Child(1));
                        context.Add(":" + endLabel, "", "");
                    }
                    break;
                default:
                    throw new CompileError("IF !FailFirst Not implemented");
            }

        }

    }


}