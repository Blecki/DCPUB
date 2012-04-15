using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class BlockLiteralNode : CompilableNode
    {
        public String dataLabel;
        public int dataSize;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();

            if (AsString.StartsWith("0x"))
                dataSize = Hex.atoh(AsString.Substring(2));
            else
                dataSize = Convert.ToUInt16(AsString);
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            dataLabel = context.GetLabel() + "_DATA";
            context.AddData(dataLabel, MakeData());
        }

        public List<ushort> MakeData() 
        { 
            var r = new List<ushort>(); 
            for (int i = 0; i < dataSize; ++i) r.Add(0); 
            return r; 
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target)
        {
            throw new CompileError("Should never reach this.");
        }
    }

    class DataLiteralNode : CompilableNode
    {
        public List<ushort> data = new List<ushort>();
        public string dataLabel;
        Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes[1].ChildNodes)
            {

                var token = child.FindTokenAndGetText();

                if (child.Term.Name == "BlockLiteral" ||
                    (child.ChildNodes.Count > 0 && child.FirstChild.Term.Name == "BlockLiteral"))
                {
                    int d = 0;
                    if (token.StartsWith("0x")) d = Hex.atoh(token.Substring(2));
                    else d = Convert.ToUInt16(token);
                    for (int i = 0; i < d; ++i) data.Add(0);
                }
                else if (token[0] == '\"')
                    foreach (var c in token.Substring(1, token.Length - 2))
                        data.Add((ushort)c);
                else if (token[0] == '\'')
                    data.Add((ushort)token[1]);
                else if (token.StartsWith("0x"))
                    data.Add(Hex.atoh(token.Substring(2)));
                else
                    data.Add(Convert.ToUInt16(token));
            }

            
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            dataLabel = context.GetLabel() + "_DATA";
            context.AddData(dataLabel, data);
        }

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            //if (data.Count == 1)
            //    assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), hex(data[0]));
            //else
            //{
                context.Add("SET", Scope.GetRegisterLabelFirst((int)target), dataLabel);
            //    context.AddData(dataLabel, data);
            //}
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }


}