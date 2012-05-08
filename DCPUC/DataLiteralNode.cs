using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class RawDataNode : CompilableNode
    {
        public List<ushort> data = new List<ushort>();
        public bool PartOfDataLiteral = false;

        public virtual void prepareData() { }
    }

    class DataLiteralNode : CompilableNode
    {
        public List<RawDataNode> dataNodes = new List<RawDataNode>();
        public List<ushort> data = new List<ushort>();
        public string dataLabel;
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes[0].ChildNodes)
            {
                if (child.Term.Name == "BlockLiteral")
                {
                    AddChild("block", child);
                    dataNodes.Add(ChildNodes[ChildNodes.Count - 1] as RawDataNode);
                }
                else if (child.ChildNodes.Count > 0 && child.FirstChild.Term.Name == "BlockLiteral")
                {
                    AddChild("block", child.FirstChild);
                    dataNodes.Add(ChildNodes[ChildNodes.Count - 1] as RawDataNode);
                }
                else
                {
                    var token = child.FindTokenAndGetText();
                    var dataNode = new RawDataNode();
                    dataNodes.Add(dataNode);

                    if (token[0] == '\"')
                        foreach (var c in token.Substring(1, token.Length - 2))
                            dataNode.data.Add((ushort)c);
                    else if (token[0] == '\'')
                        dataNode.data.Add((ushort)token[1]);
                    else if (token.StartsWith("0x"))
                        dataNode.data.Add(Hex.atoh(token.Substring(2)));
                    else
                        try
                        {
                            dataNode.data.Add(Convert.ToUInt16(token));
                        }
                        catch (Exception e)
                        {
                            dataNode.data.Add((ushort)Convert.ToInt16(token));
                        }
                }
            }
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            dataLabel = context.GetLabel() + "_DATA";
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            foreach (var node in dataNodes) node.PartOfDataLiteral = true;
            base.FoldConstants(context);
            foreach (var node in dataNodes)
            {
                node.prepareData();
                data.AddRange(node.data);
            }
            context.AddData(dataLabel, data);
            return this;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.ExpressionNode();
            r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelFirst((int)target), dataLabel);
            if (target == Register.STACK) scope.stackDepth += 1;
            return r;
        }
    }


}