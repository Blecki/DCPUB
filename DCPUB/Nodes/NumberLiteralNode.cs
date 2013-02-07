﻿using System;
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
                Value = Hex.atoh(AsString.Substring(2));
                ResultType = "word";
            }
            else if (AsString.StartsWith("0b"))
            {
                if (AsString.Length != 18) throw new CompileError(this, "Binary literals must be 16 bits");
                Value = Hex.atob(AsString.Substring(2));
                ResultType = "word";
            }
            else if (AsString.StartsWith("'"))
            {
                Value = AsString[1];
                ResultType = "word";
            }
            else
            {
                Value = Convert.ToInt16(AsString);
                ResultType = "word";
            }
        }

        public override string TreeLabel()
        {
            return "literal " + ResultType + " (" + Hex.hex(Value) + ")" + (WasFolded ? " folded" : "") + " [into:" + target.ToString() + "]";
        }

        public override bool IsIntegralConstant()
        {
            return true;
        }

        public override int GetConstantValue()
        {
            return Value;
        }

        public override Assembly.Operand GetConstantToken()
        {
            return Constant((ushort)GetConstantValue());
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.ExpressionNode();
            r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                GetConstantToken());
            return r;
        }

    }

    
}