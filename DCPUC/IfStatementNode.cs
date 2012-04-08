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

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            var hasElseBlock = ChildNodes.Count == 3;
            var elseBranchLabel = hasElseBlock ? Scope.GetLabel() + "ELSE" : "";
            var endLabel = Scope.GetLabel() + "END";

            if (ChildNodes[0] is ComparisonNode) //Emit more effecient code for plain comparisons
            {
                var rightConstant = (ChildNodes[0].ChildNodes[1] as CompilableNode).IsConstant();
                var leftConstant = (ChildNodes[0].ChildNodes[0] as CompilableNode).IsConstant();
                string left, right;
                int rightTarget = (int)Register.STACK;
                int leftTarget = (int)Register.STACK;

                if (rightConstant)
                    right = hex((ChildNodes[0].ChildNodes[1] as CompilableNode).GetConstantValue());
                else
                {
                    rightTarget = scope.FindAndUseFreeRegister();
                    (ChildNodes[0].ChildNodes[1] as CompilableNode).Compile(assembly, scope, (Register)rightTarget);
                    right = Scope.GetRegisterLabelSecond(rightTarget);
                }

                if (leftConstant)
                    left = hex((ChildNodes[0].ChildNodes[0] as CompilableNode).GetConstantValue());
                else
                {
                    leftTarget = scope.FindAndUseFreeRegister();
                    (ChildNodes[0].ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)leftTarget);
                    left = Scope.GetRegisterLabelSecond(leftTarget);
                }

                scope.FreeMaybeRegister(leftTarget);
                scope.FreeMaybeRegister(rightTarget);

                var condType = ChildNodes[0].AsString;
                assembly.Add((condType == "==" ? "IFN" : "IFE"), left, right, "Plain conditional");
            }
            else
            {
                var condTarget = scope.FindAndUseFreeRegister();
                (ChildNodes[0] as CompilableNode).Compile(assembly, scope, (Register)condTarget);
                assembly.Add("IFE", Scope.GetRegisterLabelSecond(condTarget), "0x0", "If from expression");
                scope.FreeMaybeRegister(condTarget);
                scope.stackDepth -= 1;
            }

            assembly.Add("SET", "PC", (hasElseBlock ? elseBranchLabel : endLabel), "Jump to else clause or end");
            var blockScope = BeginBlock(scope);
            assembly.Barrier();
            (ChildNodes[1] as CompilableNode).Compile(assembly, blockScope, Register.DISCARD);
            assembly.Barrier();
            EndBlock(assembly, blockScope);
            if (hasElseBlock)
            {
                assembly.Add("SET", "PC", endLabel);
                assembly.Add(":" + elseBranchLabel, "", "");
                var elseScope = BeginBlock(scope);
                assembly.Barrier();
                (ChildNodes[2] as CompilableNode).Compile(assembly, elseScope, Register.DISCARD);
                assembly.Barrier();
                EndBlock(assembly, elseScope);
            }
            assembly.Add(":" + endLabel, "", "");
        }

    }

    
}
