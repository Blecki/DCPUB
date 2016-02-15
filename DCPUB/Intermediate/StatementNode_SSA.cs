using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUB.Ast;

namespace DCPUB.Intermediate
{
    public partial class StatementNode : IRNode 
    {
        public void __ApplySSA()
        {
            /* Step 1:      Should become ==>
                SET R0, [0x0002+J]     SET R0, [0x0002+J]
                SET R0, [0x0001+R0]    SET R1, [0x0001+R0]
                SET R1, [0x0002+J]     SET R2, [0x0002+J]
                SET R1, [0x0003+R1]    SET R3, [0x0003+R2]
                ADD R0, R1             SET R4, R1; ADD R4, R3  <- Sets are a special case. Other ops need to
                SET R2, [0x0002+J]     SET R5, [0x0002+J]           expand to two instructions.
                SET [0x0001+R2], R0    SET [0x0001+R5], R4
            */

            var operandMapping = new Dictionary<ushort, ushort>();
            var operandValues = new Dictionary<ushort, Operand>();
            var ssa_instructions = new StatementNode();
            ushort new_vr = 0;
            foreach (var child in children)
            {
                var ins = child as Instruction;
                if (ins == null) ssa_instructions.children.Add(child);
                else
                {
                    Operand second_operand = null;

                    if (ins.secondOperand != null)
                    {
                        second_operand = ins.secondOperand.Clone();
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


            /* Step 2:      Should become ==>
                SET R0, [0x0002+J]      SET R0, [0x0002+J]
                SET R1, [0x0001+R0]     SET R1, [0x0001+R0]
                SET R2, [0x0002+J]
                SET R3, [0x0003+R2]     SET R3, [0x0003+R0]
                SET R4, R1; ADD R4, R3  SET R4, R1; ADD R4, R3
                SET R5, [0x0002+J]      
                SET [0x0001+R5], R4     SET [0x0001+R0], R4
                        Duplicate values to R0 were lifted.
            */

            this.children = new List<IRNode>(ssa_instructions.children);
            ssa_instructions.children.Clear();

            for (var i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                var ins = child as Instruction;
                if (ins == null)
                    ssa_instructions.children.Add(child);
                else if (ins.instruction != Instructions.SET)
                    ssa_instructions.children.Add(child);
                else if (ins.firstOperand.semantics != OperandSemantics.None)
                {
                    if (Operand.OperandsEqual(ins.firstOperand, ins.secondOperand))
                        continue;

                    ssa_instructions.children.Add(child);
                }
                else if (ins.firstOperand.register != OperandRegister.VIRTUAL)
                    ssa_instructions.children.Add(child);
                else
                {
                    // Earlier replacements could have left us with the equivilent of 'SET A, A'
                    if (Operand.OperandsEqual(ins.firstOperand, ins.secondOperand))
                        continue;

                    var valueName = ins.firstOperand;
                    var value = ins.secondOperand;
                    var usage_count = 0;

                    var simplyReplace = ins.instruction == Instructions.SET
                        && ins.firstOperand.semantics == OperandSemantics.None
                        && ins.firstOperand.register == OperandRegister.VIRTUAL
                        && ins.secondOperand.semantics == OperandSemantics.None
                        && ins.secondOperand.register == OperandRegister.VIRTUAL;

                    bool substitutionsMade = false;
                    bool insertedLater = false;

                    for (var c = i + 1; c < children.Count; ++c)
                    {
                        var candidateForReplacement = children[c];
                        var c_ins = candidateForReplacement as Instruction;
                        if (c_ins == null) continue;
                        //if (c_ins.secondOperand == null) continue;

                        if (simplyReplace)
                        {
                            if (c_ins.secondOperand != null
                                && c_ins.secondOperand.register == OperandRegister.VIRTUAL
                                && c_ins.secondOperand.virtual_register == ins.firstOperand.virtual_register)
                                c_ins.secondOperand.virtual_register = ins.secondOperand.virtual_register;
                            if (c_ins.firstOperand.register == OperandRegister.VIRTUAL
                                && c_ins.firstOperand.virtual_register == ins.firstOperand.virtual_register)
                                c_ins.firstOperand.virtual_register = ins.secondOperand.virtual_register;
                            continue;
                        }

                        // If second operand is a duplicate of value, replace with our register.
                        if (c_ins.secondOperand != null
                            && Operand.OperandsEqual(c_ins.secondOperand, value))
                        {
                            usage_count += 1;
                            c_ins.secondOperand = valueName;
                        }
                        // If second operand is a bare reference to our register, replace with our value..
                        else if (c_ins.secondOperand != null
                            && c_ins.secondOperand.semantics == OperandSemantics.None
                            && c_ins.secondOperand.register == OperandRegister.VIRTUAL
                            && c_ins.secondOperand.virtual_register == valueName.virtual_register)
                        {
                            if (usage_count == 0)
                            {
                                c_ins.secondOperand = value;
                                substitutionsMade = true;
                            }
                        }

                        // Repeat, but for the first operand.
                        if (c_ins.firstOperand.semantics == OperandSemantics.None
                            && c_ins.firstOperand.register == OperandRegister.VIRTUAL
                            && c_ins.firstOperand.virtual_register == valueName.virtual_register
                            // But don't fuck with J
                            && !(value.semantics == OperandSemantics.None && value.register == OperandRegister.J))
                        {
                            if (usage_count == 0)
                            {
                                c_ins.firstOperand = value;
                                substitutionsMade = true;
                            }
                        }

                        // Now check to see if our register is actually ever mentioned again.
                        if (c_ins.secondOperand != null
                            && c_ins.secondOperand.register == OperandRegister.VIRTUAL
                            && c_ins.secondOperand.virtual_register == valueName.virtual_register)
                        {
                            if (usage_count == 0 && substitutionsMade)
                            {
                                insertedLater = true;
                                children.Insert(c, new Instruction
                                {
                                    instruction = Instructions.SET,
                                    firstOperand = valueName,
                                    secondOperand = value
                                });
                            }
                            usage_count += 1;
                        }

                        if (c_ins.firstOperand.register == OperandRegister.VIRTUAL &&
                        c_ins.firstOperand.virtual_register == valueName.virtual_register)
                        {
                            if (usage_count == 0 && substitutionsMade)
                            {
                                insertedLater = true;
                                children.Insert(c, new Instruction
                                {
                                    instruction = Instructions.SET,
                                    firstOperand = valueName,
                                    secondOperand = value
                                });
                            }
                            usage_count += 1;
                        }
                    }

                    if (usage_count > 0 && !insertedLater)
                        ssa_instructions.children.Add(ins);
                }
            }

            this.children = new List<IRNode>(ssa_instructions.children);
        }
    }
}
