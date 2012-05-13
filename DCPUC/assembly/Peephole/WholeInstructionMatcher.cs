using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC.Assembly.Peephole
{
    public class WholeInstructionMatcher : AstNode
    {
        public virtual bool Match(Instruction ins, Dictionary<string, Operand> values) { return false; }
    }

    public class WholeInstructionMatchRaw : WholeInstructionMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("instruction", treeNode.ChildNodes[0]);
            AddChild("firstOperand", treeNode.ChildNodes[1]);
            AddChild("secondOperand", treeNode.ChildNodes[2]);
        }

        public override bool Match(Instruction ins, Dictionary<string, Operand> values)
        {
            return (ChildNodes[0] as InstructionMatcher).Match(ins) &&
                (ChildNodes[1] as OperandMatcher).Match(ins.firstOperand, values) &&
                (ChildNodes[2] as OperandMatcher).Match(ins.secondOperand, values);
        }
    }

    public class WholeInstructionMatchOr : WholeInstructionMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
            AddChild("second", treeNode.ChildNodes[1]);
        }

        public override bool  Match(Instruction ins, Dictionary<string,Operand> values)
{
 	 return (ChildNodes[0] as WholeInstructionMatcher).Match(ins, values) ||
         (ChildNodes[1] as WholeInstructionMatcher).Match(ins, values);

}


    }

    public class WholeInstructionMatchNot : WholeInstructionMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
        }

        public override bool Match(Instruction ins, Dictionary<string, Operand> values)
        {
            return !(ChildNodes[0] as WholeInstructionMatcher).Match(ins, values);
        }


    }
}
  