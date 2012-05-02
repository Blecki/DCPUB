using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class TernarySelectionNode : BranchStatementNode
    {
        public Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("condition", treeNode.ChildNodes[0].FirstChild);
            AddChild("then", treeNode.ChildNodes[1]);
            AddChild("ELSE", treeNode.ChildNodes[2]);
            
            this.AsString = "?:";
        }

        public override string TreeLabel()
        {
            return "?: " + base.TreeLabel();
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            ResultType = Child(1).ResultType;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            switch (clauseOrder)
            {
                case ClauseOrder.ConstantPass:
                    {
                        Child(1).WasFolded = true;
                        return Child(1);
                    }
                case ClauseOrder.ConstantFail:
                    {
                        if (ChildNodes.Count > 2)
                        {
                            Child(2).WasFolded = true;
                            return Child(2);
                        }
                        return null;
                    }
                default:
                    return this;
            }
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
            base.AssignRegisters(context, parentState, target);
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
                        if (ChildNodes.Count == 3) EmitBlock(context, scope, Child(2), false);
                        context.Add("SET", "PC", endLabel);
                        context.Add(":" + thenLabel, "", "");

                        EmitBlock(context, scope, Child(1), false);
                        context.Add(":" + endLabel, "", "");
                    }
                    break;
                default:
                    throw new CompileError("IF !FailFirst Not implemented");
            }

        }

    }


}