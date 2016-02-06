using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Intermediate.Peephole
{
    public class InstructionMatcher : AstNode
    {
        public virtual bool Match(Instruction ins) { return false; }
    }

    public class InstructionMatchRaw : InstructionMatcher
    {
        public string rawValue;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            rawValue = treeNode.FindTokenAndGetText();
        }

        public override bool Match(Instruction ins)
        {
            return ins.instruction.ToString() == rawValue;
        }
    }

    public class InstructionMatchOr : InstructionMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
            AddChild("second", treeNode.ChildNodes[1]);
        }

        public override bool Match(Instruction ins)
        {
            return ((ChildNodes[0] as InstructionMatcher).Match(ins)) ||
                ((ChildNodes[1] as InstructionMatcher).Match(ins));
        }
    }

    public class InstructionMatcherNot : InstructionMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
        }

        public override bool Match(Instruction ins)
        {
            return !((ChildNodes[0] as InstructionMatcher).Match(ins));
        }
    }
}
  