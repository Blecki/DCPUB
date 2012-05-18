﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC.Assembly.Peephole
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
                var r = new Operand();
                var root = opParse.Root.FirstChild;
                if (root.Term.Name == "deref")
                {
                    r.semantics |= OperandSemantics.Dereference;
                    root = root.ChildNodes[1];
                }

                if (root.Term.Name == "offset")
                {
                    r.semantics |= OperandSemantics.Constant;
                    string constant = "";
                    string reg = "";
                    if (root.FirstChild.Term.Name == "integer")
                    {
                        constant = root.FirstChild.FindTokenAndGetText();
                        reg = root.LastChild.FindTokenAndGetText();
                    }
                    else
                    {
                        constant = root.LastChild.FindTokenAndGetText();
                        reg = root.FirstChild.FindTokenAndGetText();
                    }
                    r.register = (OperandRegister)Enum.Parse(typeof(OperandRegister), reg);
                    if (constant.StartsWith("0x")) r.constant = Hex.atoh(constant.Substring(2));
                    else r.constant = Convert.ToUInt16(constant);
                }
                else if (root.Term.Name == "integer")
                {
                    r.semantics |= OperandSemantics.Constant;
                    var constant = root.FindTokenAndGetText();
                    if (constant.StartsWith("0x")) r.constant = Hex.atoh(constant.Substring(2));
                    else r.constant = Convert.ToUInt16(constant);
                }
                else
                {
                    var reg = root.FindTokenAndGetText();
                    r.register = (OperandRegister)Enum.Parse(typeof(OperandRegister), reg);
                }

                parsedOperand = r;
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

        public Node Generate(Dictionary<string, Operand> values)
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

        public List<Node> Generate(Dictionary<string, Operand> values)
        {
            var r = new List<Node>();
            foreach (var child in ChildNodes)
                r.Add((child as ReplacementInstruction).Generate(values));
            return r;
        }
    }


    
}
