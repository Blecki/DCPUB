using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class CompilableNode : AstNode
    {
        public bool WasFolded = false;
        public string ResultType = "word";
        public Register target = Register.DISCARD;

        public virtual Assembly.Node Emit(CompileContext context, Scope scope) { return null; }
        public virtual int GetConstantValue() { return 0; }
        public virtual Assembly.Operand GetConstantToken() { return null; }
        public virtual bool IsIntegralConstant() { return false; }
        public virtual string TreeLabel() { return AsString; }

        public virtual Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Emit2 not implemented on " + this.GetType().Name);
        }

        /// <summary>
        /// Returns an operand to fetch the value of a node. Returns null if the value cannot
        /// be fetched by a single operand. If this does not return null, the emission of 
        /// this nodes op-codes can be skipped.
        /// </summary>
        /// <returns>An operand to fetch the value of the node</returns>
        public virtual Assembly.Operand GetFetchToken() { return null; }

        public virtual int ReferencesVariable(Variable v)
        {
            var r = 0;
            foreach (CompilableNode child in ChildNodes)
                r += child.ReferencesVariable(v);
            return r;
        }

        public virtual void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            foreach (CompilableNode child in ChildNodes)
                child.ResolveTypes(context, enclosingScope);
        }

        public CompilableNode Child(int n) { return ChildNodes[n] as CompilableNode; }

        public virtual void GatherSymbols(CompileContext context, Scope enclosingScope) 
        {
            foreach (CompilableNode child in ChildNodes)
                child.GatherSymbols(context, enclosingScope);
        }

        public virtual CompilableNode FoldConstants(CompileContext context)
        {
            var childrenCopy = new AstNodeList();
            foreach (CompilableNode child in ChildNodes)
            {
                var nChild = child.FoldConstants(context);
                if (nChild != null) childrenCopy.Add(nChild);
            }
            ChildNodes.Clear();
            ChildNodes.AddRange(childrenCopy);
            return this;
        }

        public virtual void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            foreach (CompilableNode child in ChildNodes) child.AssignRegisters(context, parentState, target);
        }

        public static Assembly.Operand Operand(String r, 
            Assembly.OperandSemantics semantics = Assembly.OperandSemantics.None,
            ushort offset = 0)
        {
            Assembly.OperandRegister opReg;
            if (!Enum.TryParse(r, out opReg)) 
                throw new CompileError("Unmappable operand register: " + r);
            return new Assembly.Operand { register = opReg, semantics = semantics, constant = offset };
        }

        public static Assembly.Operand Dereference(String r) { return Operand(r, Assembly.OperandSemantics.Dereference); }

        public static Assembly.Operand DereferenceLabel(Assembly.Label l) 
        { 
            return new Assembly.Operand{
                label = l,
                semantics = Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Label
            };
        }
        
        public static Assembly.Operand DereferenceOffset(String s, ushort offset) 
        {
            var r = Operand(s, Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Offset);
            r.constant = offset;
            return r; 
        }

        public static Assembly.Operand Constant(ushort value)
        {
            return new Assembly.Operand { semantics = Assembly.OperandSemantics.Constant, constant = value };
        }

        public static Assembly.Operand Label(Assembly.Label value)
        {
            return new Assembly.Operand { semantics = Assembly.OperandSemantics.Label, label = value };
        }

        public static Assembly.Operand DereferenceVariableOffset(ushort offset)
        {
            var r = Operand("J", Assembly.OperandSemantics.Dereference | Assembly.OperandSemantics.Offset | 
                Assembly.OperandSemantics.VariableOffset);
            r.constant = offset;
            return r;
        }

        public static Assembly.Operand VariableOffset(ushort offset)
        {
            return new Assembly.Operand {
                semantics = Assembly.OperandSemantics.Constant | Assembly.OperandSemantics.VariableOffset,
                constant = offset 
            };
        }

        public static Assembly.Operand Virtual(int id, 
            Assembly.OperandSemantics semantics = Assembly.OperandSemantics.None,
            ushort offset = 0)
        {
            return new Assembly.Operand
            {
                register = Assembly.OperandRegister.VIRTUAL,
                virtual_register = (ushort)id,
                semantics = semantics,
                constant = offset
            };
        }
    }
}
