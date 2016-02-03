using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly.Peephole
{
    public class OperandMatcher : AstNode
    {
        public virtual bool Match(Operand op, Dictionary<String, Operand> values) { return false; }
    }

    public class OperandMatchRaw : OperandMatcher
    {
        public string rawValue;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            rawValue = treeNode.FindTokenAndGetText();
            rawValue = rawValue.Substring(1, rawValue.Length - 2);
        }

        public override bool Match(Operand op, Dictionary<String, Operand> values)
        {
            return op.ToString() == rawValue;
        }
    }

    public class OperandMatchValue : OperandMatcher
    {
        public string valueName;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            valueName = treeNode.FindTokenAndGetText();
        }

        public override bool Match(Operand op, Dictionary<string, Operand> values)
        {
            if (values.ContainsKey(valueName))
            {
                var matchWith = values[valueName];
                if (matchWith.semantics != op.semantics) return false;
                if (matchWith.register != op.register) return false;
                if (matchWith.constant != op.constant) return false;
                if (matchWith.label != op.label) return false;
                if (matchWith.virtual_register != op.virtual_register) return false;
                return true;
            }
            else
            {
                values.Add(valueName, op);
                return true;
            }
        }
    }

    public class OperandOr : OperandMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
            AddChild("second", treeNode.ChildNodes[1]);
        }

        public override bool Match(Operand op, Dictionary<string, Operand> values)
        {
            return ((ChildNodes[0] as OperandMatcher).Match(op, values)) ||
                ((ChildNodes[1] as OperandMatcher).Match(op, values));
        }
    }

    public class OperandAnd : OperandMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
            AddChild("second", treeNode.ChildNodes[1]);
        }

        public override bool Match(Operand op, Dictionary<string, Operand> values)
        {
            return ((ChildNodes[0] as OperandMatcher).Match(op, values)) &&
                ((ChildNodes[1] as OperandMatcher).Match(op, values));
        }
    }

    public class OperandNot : OperandMatcher
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("first", treeNode.ChildNodes[0]);
        }

        public override bool Match(Operand op, Dictionary<string, Operand> values)
        {
            return !((ChildNodes[0] as OperandMatcher).Match(op, values));
        }
    }
}
  