﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class NumberLiteralNode : CompilableNode
    {
        public ushort Value = 0;
        public Register target;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();

            if (AsString.StartsWith("0x"))
                Value = Hex.atoh(AsString.Substring(2));
            else if (AsString.StartsWith("'"))
                Value = AsString[1];
            else
                Value = (ushort)Convert.ToInt16(AsString);
        }

        public override string TreeLabel()
        {
            return "literal (" + Hex.hex(Value) + ")" + (WasFolded ? " folded" : "") + " [into:" + target.ToString() + "]";
        }

        public override bool IsConstant()
        {
            return true;
        }

        public override bool IsIntegralConstant()
        {
            return true;
        }

        public override ushort GetConstantValue()
        {
            return Value;
        }

        public override string GetConstantToken()
        {
            return Hex.hex(GetConstantValue());
        }

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            context.Add("SET", Scope.GetRegisterLabelFirst((int)target), GetConstantToken());
            if (target == Register.STACK) scope.stackDepth += 1;
        }

        public override void Compile(CompileContext assembly, Scope scope, Register target) 
        {
            assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), Hex.hex(Value));
            if (target == Register.STACK) scope.stackDepth += 1;
        }
    }

    
}
