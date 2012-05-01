using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class BlockLiteralNode : RawDataNode
    {
        public String dataLabel;
        public int dataSize;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("size", treeNode.ChildNodes[0]);
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            dataLabel = context.GetLabel() + "_DATA";
            Child(0).GatherSymbols(context, enclosingScope);
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            if (!Child(0).IsIntegralConstant()) throw new CompileError("Block must have a constant size.");
            dataSize = Child(0).GetConstantValue();
            if (dataSize <= 0) throw new CompileError("Block size must be > 0");
 	        //if (!PartOfDataLiteral) context.AddData(dataLabel, MakeData());
            return this;
        }

        public override void prepareData()
        {
            data = MakeData();
        }

        public List<ushort> MakeData() 
        { 
            var r = new List<ushort>(); 
            for (int i = 0; i < dataSize; ++i) r.Add(0); 
            return r; 
        }

    }


}