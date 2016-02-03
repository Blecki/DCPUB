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
        PEEK = 13,
        VIRTUAL = 14,
    }

    [Flags]
    public enum OperandSemantics
    {
        None = 0,
        Dereference = 1,
        Offset = 2,
        Constant = 4,
        Label = 8,
        VariableOffset = 16,
    }

    public class Operand
    {
        public OperandRegister register = OperandRegister.A;
        public OperandSemantics semantics = OperandSemantics.None;
        public ushort constant;
        public Label label;
        public ushort virtual_register;

        public override string ToString()
        {
            var s = "";
            if ((semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference) s += "[";
            if ((semantics & OperandSemantics.Label) == OperandSemantics.Label) s += label;
            else if ((semantics & OperandSemantics.Constant) == OperandSemantics.Constant)
                s += string.Format("0x{0:X4}", constant);
            else
            {
                if ((semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                    s += string.Format("0x{0:X4}", constant) + "+";
                s += register == OperandRegister.VIRTUAL ? string.Format("VR{0}", virtual_register) : register.ToString();
            }
            if ((semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference) s += "]";
            return s;
        }

        public void MarkRegisters(bool[] bank)
        {
            if ((semantics & OperandSemantics.Label) == OperandSemantics.Label) return;
            if ((semantics & OperandSemantics.Constant) == OperandSemantics.Constant) return;
            if (register <= OperandRegister.J) bank[(int)register] = true;
        }

        public void AdjustVariableOffsets(int delta)
        {
            if ((semantics & OperandSemantics.VariableOffset) == OperandSemantics.VariableOffset)
                if (constant > 0x8000) constant = (ushort)(constant + delta);
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
                label = label,
                virtual_register = virtual_register
            };
        }

        public bool IsIntegralConstant()
        {
            return (semantics & OperandSemantics.Constant) == OperandSemantics.Constant;
        }

        public void AssignRegisters(Dictionary<ushort, OperandRegister> mapping)
        {
            if (register == OperandRegister.VIRTUAL)
            {
                if (mapping.ContainsKey(virtual_register)) register = mapping[virtual_register];
                else
                {
                    //Find a register not referenced in mapping.
                    var reg = OperandRegister.A;
                    for (int i = 1; i < (int)OperandRegister.J; ++i)
                        if (!mapping.ContainsValue((OperandRegister)i)) reg = (OperandRegister)i;

                    if (reg != OperandRegister.A)
                    {
                        mapping.Add(virtual_register, reg);
                        register = reg;
                    }
                    else
                    {
                        //throw new CompileError("Ran out of registers. Expression too complicated.");
                    }
                }
            }
        }
    }
}
