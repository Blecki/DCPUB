using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly
{
    public class InlineStaticData : Node
    {
        public List<Operand> data = new List<Operand>();

        public override void Emit(EmissionStream stream)
        {
            var str = "DAT " + String.Join(" ", data);
            stream.WriteLine(str);
        }

        public override void SetupLabels(Dictionary<string, Label> labelTable)
        {
            foreach (var op in data)
            {
                if ((op.semantics & OperandSemantics.Label) == OperandSemantics.Label && op.label.rawLabel[0] != '\"')
                    op.label = labelTable[op.label.rawLabel];
            }
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            foreach (var op in data)
            {
                if ((op.semantics & OperandSemantics.Label) == OperandSemantics.Label)
                {
                    if (op.label.rawLabel[0] == '\"')
                        foreach (var c in op.label.rawLabel.Substring(1, op.label.rawLabel.Length - 2))
                            binary.Add(new Box<ushort> { data = (ushort)c });
                    else
                        binary.Add(op.label.position);
                }
                else
                    binary.Add(new Box<ushort> { data = op.constant });
            }
        }
    }

    public class InstructionAstNode : AstNode
    {
        public Node asmNode;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            //asmNode = ParseInstruction(treeNode);
        }

        public static Node ParseInstruction(Irony.Parsing.ParseTreeNode treeNode)
        {
            if (treeNode.Term.Name == "instruction")
            {
                var iNode = new Instruction();
                iNode.instruction = (Instructions)Enum.Parse(typeof(Instructions), treeNode.FirstChild.FindTokenAndGetText());
                iNode.firstOperand = OperandAstNode.ParseOperand(treeNode.ChildNodes[1].FirstChild);

                if (iNode.instruction > Instructions.SINGLE_OPERAND_INSTRUCTIONS)
                {
                    if (treeNode.ChildNodes[2].ChildNodes.Count > 0) throw new CompileError("Instruction only takes one argument");
                }
                else
                {
                    if (treeNode.ChildNodes[2].ChildNodes.Count == 0) throw new CompileError("Instruction takes two arguments");
                    iNode.secondOperand = OperandAstNode.ParseOperand(treeNode.ChildNodes[2].FirstChild.FirstChild);
                }

                return iNode;
            }
            else if (treeNode.Term.Name == "label")
            {
                var lNode = new LabelNode();
                lNode.label = new Label(treeNode.ChildNodes[0].FindTokenAndGetText());
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
                        dataNode.label = new Label(token);
                    }
                    else if (token[0] == '\'')
                    {
                        dataNode.semantics |= OperandSemantics.Constant;
                        dataNode.constant = (ushort)token[1];
                    }
                    else if (token.StartsWith("0x"))
                    {
                        dataNode.semantics |= OperandSemantics.Constant;
                        dataNode.constant = Hex.atoh(token.Substring(2));
                    }
                    else
                    {
                        try {
                            dataNode.constant = Convert.ToUInt16(token);
                            dataNode.semantics |= OperandSemantics.Constant;
                        } catch (Exception e)
                        {
                            dataNode.semantics |= OperandSemantics.Label;
                            dataNode.label = new Label(token);
                        }
                    }
                    dNode.data.Add(dataNode);
                }

                return dNode;

            }
            else
                throw new CompileError("Not supported");
        }

    }

    public class InstructionListAstNode : AstNode
    {
        public Node resultNode = new Node();

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                resultNode.AddChild(InstructionAstNode.ParseInstruction(child));
            }

            var labelTable = new Dictionary<String, Label>();
            foreach (var child in resultNode.children)
                if (child is LabelNode) labelTable.Add((child as LabelNode).label.rawLabel, (child as LabelNode).label);
            foreach (var child in resultNode.children)
                child.SetupLabels(labelTable);
        }
    }
}