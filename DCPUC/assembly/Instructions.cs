using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public enum Instructions
    {
        SET,

        ADD,
        SUB,
        MUL,
        MLI,
        DIV,
        DVI,
        MOD,
        MDI,

        AND,
        BOR,
        XOR,
        SHR,
        ASR,
        SHL,

        IFB,
        IFC,
        IFE,
        IFN,
        IFG,
        IFA,
        IFL,
        IFU,

        ADX,
        SBX,
        STI,
        STD,
        
        SINGLE_OPERAND_INSTRUCTIONS,
        JSR,
        INT,
        IAG,
        IAS,
        RFI,
        IAQ,
        HWN,
        HWQ,
        HWI,
    }
}
