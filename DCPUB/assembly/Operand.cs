using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public enum OperandRegister
    {
        A = 0,
        B = 1,
        C = 2,
        X = 3,
        Y = 4,
        Z = 5,
        I = 6,
        J = 7,
        PC = 8,
        SP = 9,
        EX = 10,
        PUSH = 11,
        POP = 12,
        PEEK = 13
    }

    [Flags]
    public enum OperandSemantics
    {
        None = 0,
        Dereference = 1,
        Offset = 2,
        Constant = 4,
        Label = 8,
    }

    public class Operand
    {
        public OperandRegister register = OperandRegister.A;
        public OperandSemantics semantics = OperandSemantics.None;
        public ushort constant;
        public Label label;

        public override string ToString()
        {
            var s = "";
            if ((semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference) s += "[";
            if ((semantics & OperandSemantics.Label) == OperandSemantics.Label) s += label;
            else if ((semantics & OperandSemantics.Constant) == OperandSemantics.Constant)
                s += string.Format("0x{0:X}", constant);
            else if ((semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                s += string.Format("0x{0:X}", constant) + "+" + register.ToString();
            else s += register.ToString();
            if ((semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference) s += "]";
            return s;
        }

        public static Operand fromString(String s)
        {
            return new Operand { semantics = OperandSemantics.Label, label = new Label(s) };
        }

        public Operand Clone()
        {
            return new Operand
            {
                register = register,
                semantics = semantics,
                constant = constant,
                label = label
            };
        }
    }
}
