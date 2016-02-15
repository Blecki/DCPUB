/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Intermediate;

namespace DCPUB.SSA
{
    public class SSA
    {
        /// <summary>
        /// Convert a list of ordinary instructions to SSA form. All virtual registers must be assigned
        /// only once.
        /// </summary>
        /// <param name="Instructions"></param>
        /// <returns></returns>
        public static List<SSAInstruction> BuildSSAInstruction(List<Instruction> Instructions)
        {
            var ssa_instructions = new List<SSAInstruction>();
            var virtual_register_mapping = new Dictionary<ushort, SSABareVirtualValue>();
            ushort new_vr_index = 0;
            
            foreach (var ins in Instructions)
            {
                SSAValue first_operand = null;
                SSAValue second_operand = null;

                if (ins.secondOperand != null)
                {
                    if ((ins.secondOperand.semantics & OperandSemantics.Constant) == OperandSemantics.Constant)
                        second_operand = new SSAConstantValue(ins.secondOperand.constant);
                    else if (ins.secondOperand.register != OperandRegister.VIRTUAL)
                        second_operand = new SSAVariableValue(ins.secondOperand);
                    else
                    {
                        if (!virtual_register_mapping.ContainsKey(ins.secondOperand.virtual_register))
                            throw new InternalError("Virtual register used to load before being used to store.");

                        if ((ins.secondOperand.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
                        {
                            if ((ins.secondOperand.semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                                second_operand = new SSADereferenceOffsetVirtualValue(virtual_register_mapping[ins.secondOperand.virtual_register], ins.secondOperand.constant);
                            else
                                second_operand = new SSADereferenceOffsetVirtualValue(virtual_register_mapping[ins.secondOperand.virtual_register], 0);
                        }
                        else
                            second_operand = virtual_register_mapping[ins.secondOperand.virtual_register];
                    }
                }

                if (ins.firstOperand != null)
                {
                    if ((ins.firstOperand.semantics & OperandSemantics.Constant) == OperandSemantics.Constant)
                        first_operand = new SSAConstantValue(ins.firstOperand.constant);
                    else if (ins.firstOperand.register != OperandRegister.VIRTUAL)
                        first_operand = new SSAVariableValue(ins.firstOperand);
                    else
                    {
                        var operands_modified = ins.instruction.GetOperandsModified();
                        if (ins.instruction != Intermediate.Instructions.SET 
                            && operands_modified == OperandsModified.A)
                        {
                            // Need to insert extra instruction to preserve register value.
                            var new_virtual_register = new_vr_index++;
                            var vr_value = new SSABareVirtualValue(new_virtual_register);
                            ssa_instructions.Add(new SSAInstruction(Intermediate.Instructions.SET, vr_value, TranslateOperand(ins.firstOperand, virtual_register_mapping)));
                            virtual_register_mapping.Upsert(ins.firstOperand.virtual_register, vr_value);
                        }
                        // Okay, SET or != Modify A.


                        // Pull this into a 'translate operand' function.
                        if ((ins.secondOperand.semantics & OperandSemantics.Dereference) == OperandSemantics.Dereference)
                        {
                            if ((ins.secondOperand.semantics & OperandSemantics.Offset) == OperandSemantics.Offset)
                                second_operand = new SSADereferenceOffsetVirtualValue(virtual_register_mapping[ins.secondOperand.virtual_register], ins.secondOperand.constant);
                            else
                                second_operand = new SSADereferenceOffsetVirtualValue(virtual_register_mapping[ins.secondOperand.virtual_register], 0);
                        }
                        else
                            second_operand = virtual_register_mapping[ins.secondOperand.virtual_register];
                    }
                }

                if (second_operand.register == OperandRegister.VIRTUAL)
                        {
                            if (!operandMapping.ContainsKey(second_operand.virtual_register))
                                throw new InternalError("Virtual register used as source before being encountered as destination");
                            second_operand.virtual_register = operandMapping[second_operand.virtual_register];
                        }
                    }

                    var operands_modified = ins.instruction.GetOperandsModified();

                    var first_operand = ins.firstOperand.Clone();
                    var ori_first_operand = first_operand.Clone();
                    if (first_operand.register == OperandRegister.VIRTUAL)
                    {
                        if (operandMapping.ContainsKey(first_operand.virtual_register))
                        {
                            first_operand.virtual_register = operandMapping[first_operand.virtual_register];
                            ori_first_operand.virtual_register = first_operand.virtual_register;
                        }

                        if (operands_modified == OperandsModified.A
                            && (first_operand.semantics & OperandSemantics.Dereference) != OperandSemantics.Dereference)
                        {
                            var new_register = new_vr++;
                            operandMapping.Upsert(ins.firstOperand.virtual_register, new_register);
                            first_operand.virtual_register = new_register;
                        }
                    }


                    if (ins.instruction == Instructions.SET)
                    {
                        ssa_instructions.AddInstruction(Instructions.SET, first_operand, second_operand);
                        if ((first_operand.semantics & OperandSemantics.Dereference) != OperandSemantics.Dereference)
                            operandValues.Upsert(first_operand.virtual_register, second_operand);
                    }
                    else if (operands_modified == OperandsModified.A)
                    {
                        if (first_operand.register == OperandRegister.VIRTUAL)
                        {
                            ssa_instructions.AddInstruction(Instructions.SET, CompilableNode.Virtual(first_operand.virtual_register), ori_first_operand);
                            ssa_instructions.AddInstruction(ins.instruction, CompilableNode.Virtual(first_operand.virtual_register), second_operand);
                        }
                        else
                            ssa_instructions.AddInstruction(ins.instruction, first_operand, second_operand);
                    }
                    else
                    {
                        ssa_instructions.AddInstruction(ins.instruction, first_operand, second_operand);
                    }
                }
            }


        }
    }
}
*/
