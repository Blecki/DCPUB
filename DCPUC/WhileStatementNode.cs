using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class WhileStatementNode : BranchStatementNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1].FirstChild);
            AddChild("Block", treeNode.ChildNodes[2]);
            this.AsString = "While";
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var topLabel = Scope.GetLabel() + "BEGIN_WHILE";
            assembly.Add(":" + topLabel, "", "");

            var clauseOrder = CompileConditional(assembly, scope, ChildNodes[0] as CompilableNode);

            if (clauseOrder == ClauseOrder.ConstantPass)
            {
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                assembly.Add("SET", "PC", topLabel);
            }
            else if (clauseOrder == ClauseOrder.ConstantFail)
            {
            }
            else if (clauseOrder == ClauseOrder.PassFirst)
            {
                var endLabel = Scope.GetLabel() + "END";
                assembly.Add("SET", "PC", endLabel);
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                assembly.Add("SET", "PC", topLabel);
                assembly.Add(":" + endLabel, "", "");
            }
            else if (clauseOrder == ClauseOrder.FailFirst)
            {
                var elseLabel = Scope.GetLabel() + "ELSE";
                var endLabel = Scope.GetLabel() + "END";
                assembly.Add("SET", "PC", elseLabel);
                assembly.Add("SET", "PC", endLabel);
                assembly.Add(":" + elseLabel, "", "");
                CompileBlock(assembly, scope, ChildNodes[1] as CompilableNode);
                assembly.Add("SET", "PC", topLabel);
                assembly.Add(":" + endLabel, "", "");
            }

        }

    }

    
}
