using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;
using DCPUB.Assembly;

namespace DCPUB.Ast.Assembly
{
    public class InstructionAstNode : AstNode
    {
        public IRNode asmNode;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            //asmNode = ParseInstruction(treeNode);
        }

        public static IRNode ParseInstruction(Irony.Parsing.ParseTreeNode treeNode)
        {
            if (treeNode.Term.Name == "instruction")
            {
                var iNode = new Instruction();
                iNode.instruction = (Instructions)Enum.Parse(typeof(Instructions), treeNode.FirstChild.FindTokenAndGetText());
                iNode.firstOperand = OperandAstNode.ParseOperand(treeNode.ChildNodes[1].FirstChild);

                if (iNode.instruction <= Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                {
                    if (treeNode.ChildNodes.Count >= 3)
                        iNode.secondOperand = OperandAstNode.ParseOperand(treeNode.ChildNodes[2].FirstChild.FirstChild);
                }

                return iNode;
            }
            else if (treeNode.Term.Name == "label")
            {
                var lNode = new Intermediate.LabelNode();
                lNode.label = new Intermediate.Label(treeNode.ChildNodes[0].FindTokenAndGetText());
                return lNode;
            }
            else if (treeNode.Term.Name == "dat")
            {
                var dNode = new InlineStaticData();

                for (int i = 0; i < treeNode.ChildNodes[1].ChildNodes.Count; ++i)
                {
                    var token = treeNode.ChildNodes[1].ChildNodes[i].FindTokenAndGetText();
                    var dataNode = new Operand();
                    if (token[0] == '\"')
                    {
                        dataNode.semantics |= OperandSemantics.Label;
                        dataNode.label = new Intermediate.Label(token);
                    }
                    else if (token[0] == '\'')
                    {
                        dataNode.semantics |= OperandSemantics.Constant;
                        dataNode.constant = (ushort)token[1];
                    }
                    else if (token.StartsWith("0x"))
                    {
                        dataNode.semantics |= OperandSemantics.Constant;
                        dataNode.constant = Convert.ToUInt16(token.Substring(2), 16);
                    }
                    else
                    {
                        try {
                            dataNode.constant = Convert.ToUInt16(token);
                            dataNode.semantics |= OperandSemantics.Constant;
                        } catch (Exception)
                        {
                            dataNode.semantics |= OperandSemantics.Label;
                            dataNode.label = new Intermediate.Label(token);
                        }
                    }
                    dNode.data.Add(dataNode);
                }

                return dNode;

            }
            else
                throw new InternalError("Error parsing inline ASM");
        }

    }
}