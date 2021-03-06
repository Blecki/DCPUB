﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    /*
     * --- Values: (5/6 bits) ---------------------------------------------------------
 C | VALUE     | DESCRIPTION
---+-----------+----------------------------------------------------------------
 0 | 0x00-0x07 | register (A, B, C, X, Y, Z, I or J, in that order)
 0 | 0x08-0x0f | [register]
 1 | 0x10-0x17 | [register + next word]
 0 |      0x18 | (PUSH / [--SP]) if in b, or (POP / [SP++]) if in a
 0 |      0x19 | [SP] / PEEK
 1 |      0x1a | [SP + next word] / PICK n
 0 |      0x1b | SP
 0 |      0x1c | PC
 0 |      0x1d | EX
 1 |      0x1e | [next word]
 1 |      0x1f | next word (literal)
 0 | 0x20-0x3f | literal value 0xffff-0x1e (-1..30) (literal) (only for a)
 --+-----------+----------------------------------------------------------------
  
* "next word" means "[PC++]". Increases the word length of the instruction by 1.
* By using 0x18, 0x19, 0x1a as PEEK, POP/PUSH, and PICK there's a reverse stack
  starting at memory location 0xffff. Example: "SET PUSH, 10", "SET X, POP"
* Attempting to write to a literal value fails silently
     * */

    
    
    public partial class Instruction
    {
        private enum OperandUsage
        {
            A,
            B
        }

        private static Tuple<ushort, Box<ushort>> EncodeOperand(Operand op, OperandUsage usage)
        {
            if ((op.semantics & OperandSemantics.Label) == OperandSemantics.Label)
            {
                if ((op.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
                    return new Tuple<ushort, Box<ushort>>(0x1e, op.label.position);
                else
                    return new Tuple<ushort, Box<ushort>>(0x1f, op.label.position);
            }

            if ((op.semantics & OperandSemantics.Constant) == OperandSemantics.Constant)
            {
                if ((op.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
                    return new Tuple<ushort, Box<ushort>>(0x1e, new Box<ushort> { data = op.constant });
                    
                if (usage == OperandUsage.A && op.constant == 0xFFFF)
                    return new Tuple<ushort, Box<ushort>>(0x20, null);
                if (usage == OperandUsage.A && op.constant <= 30)
                    return new Tuple<ushort, Box<ushort>>((ushort)(0x21u + op.constant), null);

                return new Tuple<ushort, Box<ushort>>(0x1f, new Box<ushort> { data = op.constant });
            }

            if (op.register == OperandRegister.EX) return new Tuple<ushort,Box<ushort>>(0x1d, null);
            if (op.register == OperandRegister.PC) return new Tuple<ushort,Box<ushort>>(0x1c, null);
            if (op.register == OperandRegister.PUSH || op.register == OperandRegister.POP)
                return new Tuple<ushort,Box<ushort>>(0x18, null);
            if (op.register == OperandRegister.SP)
            {
                if ((op.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
                {
                    if ((op.semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                        return new Tuple<ushort,Box<ushort>>(0x1a, new Box<ushort>{ data = op.constant });
                    else
                        return new Tuple<ushort,Box<ushort>>(0x19, null);
                }
                else
                    return new Tuple<ushort,Box<ushort>>(0x1b, null);
            }
            if (op.register == OperandRegister.PEEK)
            {
                if (op.semantics == OperandSemantics.Dereference)
                    throw new InternalError("Generated impossible code: Can't dereference peek.");
                return new Tuple<ushort, Box<ushort>>(0x19, null);
            }

            var r = (ushort)op.register;
            if ((op.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
            {
                r += 8;
                if ((op.semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                {
                    r += 8;
                    return new Tuple<ushort,Box<ushort>>(r, new Box<ushort>{ data = op.constant });
                }
                else
                    return new Tuple<ushort,Box<ushort>>(r, null);
            }
            else
                return new Tuple<ushort,Box<ushort>>(r, null);

        }
    }
}
