using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class ComparisonNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return AsString;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            if (Child(0).IsIntegralConstant() && Child(1).IsIntegralConstant())
            {
                var firstValue = Child(0).GetConstantValue();
                var secondValue = Child(1).GetConstantValue();
                if (AsString == "==") return new NumberLiteralNode
                {
                    Value = (firstValue == secondValue ? (ushort)1 : (ushort)0),
                    WasFolded = true
                };
                if (AsString == "!=") return new NumberLiteralNode
                {
                    Value = (firstValue != secondValue ? (ushort)1 : (ushort)0),
                    WasFolded = true
                };
                if (AsString == ">") return new NumberLiteralNode
                {
                    Value = (firstValue > secondValue ? (ushort)1 : (ushort)0),
                    WasFolded = true
                };
                if (AsString == "<") return new NumberLiteralNode
                {
                    Value = (firstValue < secondValue ? (ushort)1 : (ushort)0),
                    WasFolded = true
                };
            }
            return this;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            throw new CompileError("Branch node should have handled this.");
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Branch node should have handled this.");
        }

    }

    
}
