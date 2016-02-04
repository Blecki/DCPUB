using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class CompilableNode : AstNode
    {
        public string ResultType = "word";

        public virtual Assembly.Node Emit(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Emit not implemented on " + this.GetType().Name);
        }

        /// <summary>
        /// Returns an operand to fetch the value of a node. Returns null if the value cannot
        /// be fetched by a single operand. If this does not return null, the emission of 
        /// this nodes op-codes can be skipped.
        /// </summary>
        /// <returns>An operand to fetch the value of the node</returns>
        public virtual Assembly.Operand GetFetchToken() { return null; }

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

        public static Assembly.Operand Operand(String r, 
            Assembly.OperandSemantics semantics = Assembly.OperandSemantics.None,
            ushort offset = 0)
        {
            Assembly.OperandRegister opReg;
            if (!Enum.TryParse(r, out opReg))
                throw new InternalError("Unmappable operand register: " + r);
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
            if ((semantics & Assembly.OperandSemantics.Offset) == Assembly.OperandSemantics.Offset &&
                offset == 0)
                semantics &= ~Assembly.OperandSemantics.Offset;

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
