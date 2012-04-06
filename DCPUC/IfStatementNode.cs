using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class IfStatementNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            AddChild("Block", treeNode.ChildNodes[2]);
            if (treeNode.ChildNodes.Count == 5) AddChild("Else", treeNode.ChildNodes[4]);
            this.AsString = "If";
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope);
            var hasElseBlock = ChildNodes.Count == 3;
            var elseBranchLabel = hasElseBlock ? Scope.GetLabel() + "ELSE" : "";
            var endLabel = Scope.GetLabel() + "END";
            assembly.Add("IFE POP, 0x0");
            assembly.Add("SET PC, " + (hasElseBlock ? elseBranchLabel : endLabel));
            scope.stackDepth -= 1;
            var blockScope = BeginBlock(scope);
            (ChildNodes[1] as CompilableNode).Compile(assembly, blockScope);
            EndBlock(assembly, blockScope);
            if (hasElseBlock)
            {
                assembly.Add("SET PC, " + endLabel);
                assembly.Add(":" + elseBranchLabel);
                var elseScope = BeginBlock(scope);
                (ChildNodes[2] as CompilableNode).Compile(assembly, elseScope);
                EndBlock(assembly, elseScope);
            }
            assembly.Add(":" + endLabel);
        }

    }

    
}
