﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class StringLiteralNode : CompilableNode
    {
        public string value;
        public Assembly.Label staticLabel;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = "";
            value = treeNode.FindTokenAndGetText();
            value = value.Substring(1, value.Length - 2);
        }

        public override string TreeLabel()
        {
            return "literal " + value;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);
            staticLabel = Assembly.Label.Make("_STRING");

            var data = new List<ushort>();
            data.Add((ushort)value.Length);
            foreach (var c in value)
                data.Add((ushort)c);
            context.AddData(staticLabel, data);
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.ExpressionNode();
            r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                Label(staticLabel));
            return r;
        }

    }

    
}