﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class SizeofNode : CompilableNode
    {
        public String typeName;
        public Struct _struct = null;
        public bool IsAssignedTo { get; set; }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            typeName = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override string TreeLabel()
        {
            return "sizeof " + typeName + " [" + _struct.size + "] [into:" + target.ToString() + "]";
        }

        public override bool IsIntegralConstant()
        {
            return true;
        }

        public override Assembly.Operand GetConstantToken()
        {
            return Constant((ushort)_struct.size);
        }

        public override int GetConstantValue()
        {
            if (_struct.size == 0) throw new CompileError(this, "Struct size not yet determined");
            return _struct.size;
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            _struct = enclosingScope.FindType(typeName);
            if (_struct == null) throw new CompileError("Could not find type " + typeName);
            ResultType = "word";
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            this.target = target;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.ExpressionNode();
            if (_struct.size == 0) throw new CompileError(this, "Struct size not yet determined");
            r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelFirst((int)target)),
                Constant((ushort)_struct.size));
            return r;
        }

    }
    
}