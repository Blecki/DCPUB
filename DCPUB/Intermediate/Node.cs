﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    public class IRNode
    {
        public List<IRNode> children = new List<IRNode>();

        public void AddChild(IRNode child)
        {
            children.Add(child);
        }

        public void AddInstruction(Instructions instruction, Operand firstOperand = null, Operand secondOperand = null)
        {
            AddChild(Instruction.Make(instruction, firstOperand, secondOperand));
        }

        public void AddLabel(Intermediate.Label label)
        {
            AddChild(new LabelNode { label = label });
        }

        public virtual void Emit(EmissionStream stream) 
        {
            stream.indentDepth += 1;
            foreach (var child in children) child.Emit(stream);
            stream.indentDepth -= 1;
        }

        public virtual void EmitIR(EmissionStream stream, bool Tidy)
        {
            if (!Tidy) stream.WriteLine("[generic node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream, Tidy);
            stream.indentDepth -= 1;
            if (!Tidy) stream.WriteLine("[/generic node]");
        }

        public virtual void PeepholeTree(Peephole.Peepholes peepholes) 
        {
            foreach (var child in children) child.PeepholeTree(peepholes);
        }

        public virtual int InstructionCount()
        {
            return children.Sum((node) => { return node.InstructionCount(); });
        }

        public virtual void EmitBinary(List<Box<ushort>> binary)
        {
            foreach (var child in children) child.EmitBinary(binary);
        }

        public virtual void SetupLabels(Dictionary<string, Label> labelTable)
        { }

        public virtual void AssignRegisters(Dictionary<ushort, VirtualRegisterRecord> mapping)
        {
            foreach (var child in children) child.AssignRegisters(mapping);
        }

        
        public virtual void MarkUsedRealRegisters(bool[] bank)
        {
            foreach (var child in children) child.MarkUsedRealRegisters(bank);
        }

        /// <summary>
        /// Apply a delta to all variable offsets in the code tree.
        /// </summary>
        /// <param name="delta"></param>
        public virtual void CorrectVariableOffsets(int delta)
        {
            foreach (var child in children) child.CorrectVariableOffsets(delta);
        }

        internal void CollapseTransientNodes()
        {
            foreach (var child in children) if (child != null) child.CollapseTransientNodes();
            children = children.SelectMany(n =>
            {
                if (n == null) return new List<IRNode>();
                else if (n is TransientNode) return (n as TransientNode).children;
                else return (new IRNode[] { n }).ToList();
            }).ToList();
        }

        public void MergeConsecutiveStatements()
        {
            foreach (var child in children) child.MergeConsecutiveStatements();

            if (children.Count <= 1) return;

            for (var index = 0; index < children.Count - 1;)
            {
                var first = children[index] as StatementNode;
                var second = children[index + 1] as StatementNode;

                if (first != null && second != null)
                {
                    first.children.AddRange(second.children);
                    children.RemoveAt(index + 1);
                }
                else
                    index += 1;
            }
        }

        public virtual void ApplySSA()
        {
            foreach (var child in children) child.ApplySSA();
        }

        internal virtual void ErrorCheck(CompileContext Context, Ast.CompilableNode Ast)
        {
            foreach (var child in children) child.ErrorCheck(Context, Ast);
        }
    }

    public class TransientNode : IRNode 
    {
        public override void EmitIR(EmissionStream stream, bool Tidy)
        {
            if (!Tidy) stream.WriteLine("[transient node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream, Tidy);
            stream.indentDepth -= 1;
            if (!Tidy) stream.WriteLine("[/transient node]");

        }
    }
}
