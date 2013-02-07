using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class ArrayInitializationNode : CompilableNode
    {
        internal ushort[] rawData;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var item in treeNode.ChildNodes[0].ChildNodes)
                AddChild("child", item);
        }

        public override string TreeLabel()
        {
            return "array initialization";
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            rawData = new ushort[ChildNodes.Count];

            for (int i = 0; i < ChildNodes.Count; ++i)
            {
                var _c = Child(i);
                if (_c == null) throw new CompileError("Failed sanity check: Array items not nodes?");
                if (!_c.IsIntegralConstant()) throw new CompileError("Array elements must be compile time constants.");
                rawData[i] = (ushort)_c.GetConstantValue();
            }
            return this;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation("Array Initialization"));
            for (int i = rawData.Length - 1; i >= 0; --i)
                r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Constant(rawData[i]));
            return r;
        }
    }
}
