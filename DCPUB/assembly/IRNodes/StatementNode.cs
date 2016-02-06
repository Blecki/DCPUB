using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public class VirtualRegisterRecord
    {
        public int first_instruction = 0;
        public int last_instruction = 0;
        public OperandRegister assignedRegister = OperandRegister.A;
    }

    public class StatementNode : IRNode 
    {
        public override void PeepholeTree(Peephole.Peepholes peepholes)
        {
            if (peepholes != null) peepholes.ProcessAssembly(children);
        }

        public override void EmitIR(EmissionStream stream)
        {
            stream.WriteLine("[statement node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream);
            stream.indentDepth -= 1;
            stream.WriteLine("[/statement node]");
        }

        /// <summary>
        /// Record the lifetime of any virtual register used by the operand.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="i"></param>
        /// <param name="operand"></param>
        private static void MarkVirtualRegisterLifetime(Dictionary<ushort, VirtualRegisterRecord> mapping, int i, Operand operand)
        {
            if (operand.register == OperandRegister.VIRTUAL)
            {
                if (!mapping.ContainsKey(operand.virtual_register))
                    mapping.Add(operand.virtual_register, new VirtualRegisterRecord { first_instruction = i });
                mapping[operand.virtual_register].last_instruction = i;
            }
        }

        /// <summary>
        /// Assign a real register to the operand, if it uses a virtual register/
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="usedRegisters"></param>
        /// <param name="i"></param>
        /// <param name="operand"></param>
        private static void AssignRealRegisterToOperand(Dictionary<ushort, VirtualRegisterRecord> mapping,
            bool[] usedRegisters, int i, Operand operand)
        {
            if (operand.register == OperandRegister.VIRTUAL)
            {
                if (!mapping.ContainsKey(operand.virtual_register)) throw new InternalError("Virtual register not marked.");
                if (mapping[operand.virtual_register].assignedRegister == OperandRegister.A)
                {
                    //Assign a register to this virtual register.
                    int reg = -1;
                    for (int r = 0; r < 6; ++r)
                        if (usedRegisters[r] == false) reg = r;
                    if (reg == -1) throw new InternalError("Register spill.");
                    mapping[operand.virtual_register].assignedRegister = (OperandRegister)(reg + 1);
                    usedRegisters[reg] = true;
                }
                operand.register = mapping[operand.virtual_register].assignedRegister;
                if (mapping[operand.virtual_register].last_instruction == i)
                    usedRegisters[(int)(mapping[operand.virtual_register].assignedRegister) - 1] = false;
            }
        }

        /// <summary>
        /// Convert virtual registers to real registers.
        /// </summary>
        /// <param name="mapping"></param>
        public override void AssignRegisters(Dictionary<ushort, VirtualRegisterRecord> mapping)
        {
            if (children.Count(c => !(c is Instruction) && !(c is Annotation)) != 0)
                throw new InternalError("Only instructions and annotations should be children of statements.");
            
            mapping = new Dictionary<ushort, VirtualRegisterRecord>();

            // Find the first and last mentions of each register.
            for (int i = 0; i < children.Count; ++i)
            {               
                if (children[i] is Instruction)
                {
                    var instruction = children[i] as Instruction;
                    MarkVirtualRegisterLifetime(mapping, i * 2, instruction.firstOperand);
                    if (instruction.secondOperand != null) MarkVirtualRegisterLifetime(mapping, (i * 2) + 1, instruction.secondOperand);
                }
            }

            // Six flags, because we cannot assign A or J to a virtual register.
            bool[] usedRegisters = new bool[6] { false, false, false, false, false, false };

            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] is Instruction)
                {
                    var instruction = children[i] as Instruction;

                    // Assigning registers in reverse allows the from register to un-mark it's real register.
                    // Then the destination register can recycle it in the same instruction.
                    // TODO: Make this change after some validation that it won't break things.
                    AssignRealRegisterToOperand(mapping, usedRegisters, i * 2, instruction.firstOperand);

                    if (instruction.secondOperand != null) AssignRealRegisterToOperand(mapping, usedRegisters, (i * 2) + 1, instruction.secondOperand);

                }
            }
        }
    }
}
