using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public partial class CompilableNode : AstNode
    {
        public static Intermediate.Operand Operand(String r,
            Intermediate.OperandSemantics semantics = Intermediate.OperandSemantics.None,
            ushort offset = 0)
        {
            Intermediate.OperandRegister opReg;
            if (!Enum.TryParse(r, out opReg))
                throw new InternalError("Unmappable operand register: " + r);
            return new Intermediate.Operand { register = opReg, semantics = semantics, constant = offset };
        }

        public static Intermediate.Operand Operand(Model.Register r)
        {
            return Operand(r.ToString());
        }

        public static Intermediate.Operand Dereference(String r) { return Operand(r, Intermediate.OperandSemantics.Dereference); }

        public static Intermediate.Operand DereferenceLabel(Intermediate.Label l) 
        { 
            return new Intermediate.Operand{
                label = l,
                semantics = Intermediate.OperandSemantics.Dereference | Intermediate.OperandSemantics.Label
            };
        }
        
        public static Intermediate.Operand DereferenceOffset(String s, ushort offset) 
        {
            var r = Operand(s, Intermediate.OperandSemantics.Dereference | Intermediate.OperandSemantics.Offset);
            r.constant = offset;
            return r; 
        }

        public static Intermediate.Operand Constant(ushort value)
        {
            return new Intermediate.Operand { semantics = Intermediate.OperandSemantics.Constant, constant = value };
        }

        public static Intermediate.Operand Label(Intermediate.Label value)
        {
            return new Intermediate.Operand { semantics = Intermediate.OperandSemantics.Label, label = value };
        }

        public static Intermediate.Operand DereferenceVariableOffset(ushort offset)
        {
            var r = Operand("J", Intermediate.OperandSemantics.Dereference | Intermediate.OperandSemantics.Offset | 
                Intermediate.OperandSemantics.VariableOffset);
            r.constant = offset;
            return r;
        }

        public static Intermediate.Operand VariableOffset(ushort offset)
        {
            return new Intermediate.Operand {
                semantics = Intermediate.OperandSemantics.Constant | Intermediate.OperandSemantics.VariableOffset,
                constant = offset 
            };
        }

        public static Intermediate.Operand Virtual(int id, 
            Intermediate.OperandSemantics semantics = Intermediate.OperandSemantics.None,
            ushort offset = 0)
        {
            if ((semantics & Intermediate.OperandSemantics.Offset) == Intermediate.OperandSemantics.Offset &&
                offset == 0)
                semantics &= ~Intermediate.OperandSemantics.Offset;

            return new Intermediate.Operand
            {
                register = Intermediate.OperandRegister.VIRTUAL,
                virtual_register = (ushort)id,
                semantics = semantics,
                constant = offset
            };
        }
    }
}
