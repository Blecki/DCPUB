using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.Intermediate
{
    public enum Instructions
    {
        SET = 0x01,
        ADD = 0x02,
        SUB = 0x03,
        MUL = 0x04,
        MLI = 0x05,
        DIV = 0x06,
        DVI = 0x07,
        MOD = 0x08,
        MDI = 0x09,
        AND = 0x0a,
        BOR = 0x0b,
        XOR = 0x0c,
        SHR = 0x0d,
        ASR = 0x0e,
        SHL = 0x0f,

        IFB = 0x10,
        IFC = 0x11,
        IFE = 0x12,
        IFN = 0x13,
        IFG = 0x14,
        IFA = 0x15,
        IFL = 0x16,
        IFU = 0x17,

        ADX = 0x1a,
        SBX = 0x1b,

        STI = 0x1e,
        STD = 0x1f,

        SINGLE_OPERAND_INSTRUCTIONS = 0x100,

        JSR = 0x101,
        HLT = 0x102, //Non spec

        INT = 0x108,
        IAG = 0x109,
        IAS = 0x10a,
        RFI = 0x10b,
        IAQ = 0x10c,

        HWN = 0x110,
        HWQ = 0x111,
        HWI = 0x112,


    }

    public enum OperandsModified
    {
        A,
        B,
        Both
    }

    public static class InstructionExtension
    {
        public static OperandsModified GetOperandsModified(this Instructions Ins)
        {
            if (Ins <= Instructions.SHL) return OperandsModified.A;
            return OperandsModified.Both;
        }

        public static int GetOperandCount(this Instructions Ins)
        {
            if (Ins >= Instructions.SINGLE_OPERAND_INSTRUCTIONS) return 1;
            return 2;
        }
    }
}
