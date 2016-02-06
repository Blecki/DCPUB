using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly.Peephole
{
    public class ReplacementOperand : AstNode
    {
        public virtual Operand Generate(Dictionary<string, Operand> values) { return null; }
    }

    public class ReplacementRaw : ReplacementOperand
    {
        public String rawValue;
        public Operand parsedOperand;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            rawValue = treeNode.FindTokenAndGetText();
            rawValue = rawValue.Substring(1, rawValue.Length - 2);

            var opParse = Peepholes.operandParser.Parse(rawValue);
            if (opParse.HasErrors()) parsedOperand = Operand.fromString(rawValue);
            else
            {
                parsedOperand = OperandAstNode.ParseOperand(opParse.Root.FirstChild);
            }
        }

        public override Operand Generate(Dictionary<string, Operand> values)
        {
            return parsedOperand.Clone();
            //return Operand.fromString(rawValue);
        }
    }

    public class ReplacementValue : ReplacementOperand
    {
        public String valueName;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            valueName = treeNode.FindTokenAndGetText();
        }

        public override Operand Generate(Dictionary<string, Operand> values)
        {
            if (!values.ContainsKey(valueName)) throw new Exception("Unknown value");
            return values[valueName].Clone();
        }
    }

    public class ReplacementDereference : ReplacementOperand
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("child", treeNode.ChildNodes[1]);
        }

        public override Operand Generate(Dictionary<string, Operand> values)
        {
            var r = (ChildNodes[0] as ReplacementOperand).Generate(values);
            r.semantics |= OperandSemantics.Dereference;
            return r;
        }
    }

    public class ReplacementInstruction : AstNode
    {
        public Instructions instruction;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            instruction = (Instructions)Enum.Parse(typeof(Instructions), treeNode.ChildNodes[0].FindTokenAndGetText());
            AddChild("firstOperand", treeNode.ChildNodes[1]);
            AddChild("secondOperand", treeNode.ChildNodes[2]);
        }

        public IRNode Generate(Dictionary<string, Operand> values)
        {
            return Instruction.Make(instruction, (ChildNodes[0] as ReplacementOperand).Generate(values),
                (ChildNodes[1] as ReplacementOperand).Generate(values));
        }
    }

    public class ReplacementSet : AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
                AddChild("Instruction", child);
        }

        public List<IRNode> Generate(Dictionary<string, Operand> values)
        {
            var r = new List<IRNode>();
            foreach (var child in ChildNodes)
                r.Add((child as ReplacementInstruction).Generate(values));
            return r;
        }
    }


    
}
