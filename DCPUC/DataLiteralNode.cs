using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class BlockLiteralNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();
        }

        public override bool IsConstant()
        {
            return true;
        }

        public override ushort GetConstantValue()
        {
            if (AsString.StartsWith("0x"))
                return atoh(AsString.Substring(2));
            else
                return Convert.ToUInt16(AsString);
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            throw new CompileError("Should never reach this.");
        }
    }

    class DataLiteralNode : CompilableNode
    {
        List<ushort> data = new List<ushort>();
        string dataLabel;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {

                var token = child.FindTokenAndGetText();

                if (child.Term.Name == "BlockLiteral" ||
                    (child.ChildNodes.Count > 0 && child.FirstChild.Term.Name == "BlockLiteral"))
                {
                    int d = 0;
                    if (token.StartsWith("0x")) d = atoh(token.Substring(2));
                    else d = Convert.ToUInt16(token);
                    for (int i = 0; i < d; ++i) data.Add(0);
                }
                else if (token[0] == '\"')
                    foreach (var c in token.Substring(1, token.Length - 2))
                        data.Add((ushort)c);
                else if (token.StartsWith("0x"))
                    data.Add(atoh(token.Substring(2)));
                else
                    data.Add(Convert.ToUInt16(token));
            }

            if (data.Count > 1) dataLabel = Scope.GetLabel() + "_DATA";
        }

        public override bool IsConstant()
        {
            return data.Count == 1;
        }

        public override ushort GetConstantValue()
        {
            return data[0];
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            if (data.Count == 1)
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex(data[0]));
            else
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), dataLabel);
                Scope.AddData(dataLabel, data);
            }
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }


}