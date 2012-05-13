using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class TernarySelectionNode : BranchStatementNode
    {
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

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = base.Emit(context, scope);
            r = new Assembly.ExpressionNode { children = r.children };
            switch (clauseOrder)
            {
                case ClauseOrder.FailFirst: //Only actual valid order.
                    {
                        var thenLabel = context.GetLabel() + "THEN";
                        var endLabel = context.GetLabel() + "END";

                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(thenLabel));
                        if (ChildNodes.Count == 3) 
                            r.AddChild(new Assembly.ExpressionNode { children = EmitBlock(context, scope, Child(2), false).children });
                        r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(endLabel));
                        r.AddLabel(thenLabel);

                        r.AddChild(new Assembly.ExpressionNode { children = EmitBlock(context, scope, Child(1), false).children });
                        r.AddLabel(endLabel);
                    }
                    break;
                default:
                    throw new CompileError("IF !FailFirst Not implemented");
            }
            return r;

        }

    }


}