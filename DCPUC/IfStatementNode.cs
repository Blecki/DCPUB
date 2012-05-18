using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class IfStatementNode : BranchStatementNode
    {
        private Irony.Parsing.SourceSpan headerSpan;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.Term.Name == "IfElse")
            {
                AddChild("condition", treeNode.ChildNodes[0].ChildNodes[1].FirstChild);
                AddChild("then", treeNode.ChildNodes[0].ChildNodes[2]);
                AddChild("ELSE", treeNode.ChildNodes[2]);
                headerSpan = new Irony.Parsing.SourceSpan(this.Span.Location,
                    treeNode.ChildNodes[0].ChildNodes[1].FirstChild.Span.EndPosition - this.Span.Location.Position);
            }
            else
            {
                AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
                AddChild("Block", treeNode.ChildNodes[2]);
                if (treeNode.ChildNodes.Count == 5) AddChild("Else", treeNode.ChildNodes[4]);
                headerSpan = new Irony.Parsing.SourceSpan(this.Span.Location,
                    treeNode.ChildNodes[1].FirstChild.Span.EndPosition - this.Span.Location.Position);
            }
            this.AsString = "If";

        }

        public override string TreeLabel()
        {
            return "if " + base.TreeLabel();
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

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = base.Emit(context, scope);
            r.children.Insert(0, new Assembly.Annotation(context.GetSourceSpan(headerSpan)));
            switch (clauseOrder)
            {
                case ClauseOrder.FailFirst: //Only actual valid order.
                    {
                        var thenClauseAssembly = EmitBlock(context, scope, Child(1));
                        Assembly.Node elseClauseAssembly = ChildNodes.Count == 3 ? EmitBlock(context, scope, Child(2)) : null;

                        thenClauseAssembly.CollapseTree();
                        if (elseClauseAssembly != null) elseClauseAssembly.CollapseTree();

                        if (thenClauseAssembly.InstructionCount() == 0 &&
                            (elseClauseAssembly == null || elseClauseAssembly.InstructionCount() == 0))
                        {
                            r.children.RemoveRange(1, r.children.Count - 1);
                        }
                        else if (thenClauseAssembly.InstructionCount() == 1 && 
                            (elseClauseAssembly == null || elseClauseAssembly.InstructionCount() == 0))
                        {
                            r.AddChild(thenClauseAssembly);
                        }
                        else
                        {
                            var thenLabel = Assembly.Label.Make("THEN");
                            var endLabel = Assembly.Label.Make("END");

                            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(thenLabel));
                            if (elseClauseAssembly != null) r.AddChild(elseClauseAssembly);
                            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(endLabel));
                            r.AddLabel(thenLabel);

                            r.AddChild(thenClauseAssembly);
                            r.AddLabel(endLabel);
                        }
                    }
                    break;
                default:
                    throw new CompileError("IF !FailFirst Not implemented");
            }
            return r;

        }

    }


}