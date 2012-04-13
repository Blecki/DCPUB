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

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            var clauseOrder = CompileConditional(assembly, scope, ChildNodes[0] as CompilableNode);

            if (clauseOrder == ClauseOrder.ConstantPass)
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
            else if (clauseOrder == ClauseOrder.ConstantFail)
            {
                if (ChildNodes.Count == 3) CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
            }
            else if (clauseOrder == ClauseOrder.PassFirst)
            {
                var elseLabel = assembly.GetLabel() + "ELSE";
                var endLabel = assembly.GetLabel() + "END";

                assembly.Add("SET", "PC", elseLabel);
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);

                if (ChildNodes.Count == 3)
                {
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                    CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
                }

                assembly.Add(":" + endLabel, "", "");
            }
            else if (clauseOrder == ClauseOrder.FailFirst)
            {
                var elseLabel = assembly.GetLabel() + "ELSE";
                var endLabel = assembly.GetLabel() + "END";
                if (ChildNodes.Count == 3)
                {
                    assembly.Add("SET", "PC", elseLabel);
                    CompileBlock(assembly, scope, ChildNodes[2] as CompilableNode);
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                }
                else
                {
                    assembly.Add("SET", "PC", elseLabel);
                    assembly.Add("SET", "PC", endLabel);
                    assembly.Add(":" + elseLabel, "", "");
                }

                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                assembly.Add(":" + endLabel, "", "");
            }

        }

    }


}