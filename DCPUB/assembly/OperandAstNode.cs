using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly
{
    public class OperandAstNode : AstNode
    {
        public Operand parsedOperand;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            //parsedOperand = ParseOperand(treeNode.FirstChild);
        }

        public static Operand ParseOperand(Irony.Parsing.ParseTreeNode root)
        {
            var r = new Operand();
            if (root.Term.Name == "deref")
            {
                r.semantics |= OperandSemantics.Dereference;
                root = root.ChildNodes[1].FirstChild;
            }

            if (root.Term.Name == "offset")
            {
                r.semantics |= OperandSemantics.Offset;
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
                try
                {
                    r.register = (OperandRegister)Enum.Parse(typeof(OperandRegister), reg);
                } catch (Exception)
                {
                    r.semantics |= OperandSemantics.Label;
                    r.label = new Label(reg);
                }
            }

            return r;
        }

    }
}