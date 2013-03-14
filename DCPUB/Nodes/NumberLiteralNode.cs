using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class NumberLiteralNode : CompilableNode
    {
        public int Value = 0;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();

            if (AsString.StartsWith("0x"))
            {
                Value = Convert.ToUInt16(AsString.Substring(2), 16);
                ResultType = "word";
            }
            else if (AsString.StartsWith("0b"))
            {
                if (AsString.Length > 18) throw new CompileError(this, "Binary literals cannot be longer than 16 bits");
                Value = Convert.ToUInt16(AsString.Substring(2), 2);
                ResultType = "word";
            }
            else if (AsString.StartsWith("'"))
            {
                if (AsString.StartsWith("'\\"))
                {
                    if (AsString[2] == 'n') Value = '\n';
                    else Value = AsString[2];
                }
                else
                    Value = AsString[1];
                ResultType = "word";
            }
            else
            {
                Value = Convert.ToInt16(AsString);
                ResultType = "word";
            }
        }

        public override Assembly.Operand GetFetchToken()
        {
            return Constant((ushort)Value);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Instruction
            {
                instruction = Assembly.Instructions.SET,
                firstOperand = target.GetOperand(TargetUsage.Push),
                secondOperand = Constant((ushort)Value)
            };
        }

    }

    
}
