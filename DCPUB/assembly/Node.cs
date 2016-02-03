using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Assembly
{
    public class Node
    {
        public List<Node> children = new List<Node>();

        public void AddChild(Node child)
        {
            children.Add(child);
        }

        public void AddInstruction(Instructions instruction, Operand firstOperand, Operand secondOperand = null)
        {
            AddChild(Instruction.Make(instruction, firstOperand, secondOperand));
        }

        public void AddLabel(Assembly.Label label)
        {
            AddChild(new LabelNode { label = label });
        }

        public virtual void Emit(EmissionStream stream) 
        {
            stream.indentDepth += 1;
            foreach (var child in children) child.Emit(stream);
            stream.indentDepth -= 1;
        }

        public virtual void EmitIR(EmissionStream stream)
        {
            stream.WriteLine("[generic node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream);
            stream.indentDepth -= 1;
            stream.WriteLine("[/generic node]");
        }

        public virtual List<Node> CollapseTree(Peephole.Peepholes peepholes) 
        {
            var result = new List<Node>();
            foreach (var child in children) result.AddRange(child.CollapseTree(peepholes));
            children = result;
            return new List<Node>(new Node[]{this}); 
        }

        public virtual int InstructionCount()
        {
            return children.Sum((node) => { return node.InstructionCount(); });
        }

        public virtual void EmitBinary(List<Box<ushort>> binary)
        {
            foreach (var child in children) child.EmitBinary(binary);
        }

        public virtual void SetupLabels(Dictionary<string, Label> labelTable)
        { }

        public virtual void AssignRegisters(Dictionary<ushort, VirtualRegisterRecord> mapping)
        {
            foreach (var child in children) child.AssignRegisters(mapping);
        }

        public virtual void MarkRegisters(bool[] bank)
        {
            foreach (var child in children) child.MarkRegisters(bank);
        }

        public virtual void AdjustVariableOffsets(int delta)
        {
            foreach (var child in children) child.AdjustVariableOffsets(delta);
        }
    }

    public class VirtualRegisterRecord
    {
        public int first_instruction = 0;
        public int last_instruction = 0;
        public OperandRegister assignedRegister = OperandRegister.A;
    }

    public class StatementNode : Node 
    {
        public override List<Node> CollapseTree(Peephole.Peepholes peepholes)
        {
            var r = base.CollapseTree(peepholes);
            if (peepholes != null) peepholes.ProcessAssembly(children);
            return r;
        }

        public override void EmitIR(EmissionStream stream)
        {
            stream.WriteLine("[statement node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream);
            stream.indentDepth -= 1;
            stream.WriteLine("[/statement node]");
        }

        public static void MarkVirtualRegisterUsage(Dictionary<ushort, VirtualRegisterRecord> mapping, int i, Operand operand)
        {
            if (operand.register == OperandRegister.VIRTUAL)
            {
                if (!mapping.ContainsKey(operand.virtual_register))
                    mapping.Add(operand.virtual_register, new VirtualRegisterRecord { first_instruction = i });
                mapping[operand.virtual_register].last_instruction = i;
            }
        }

        public static void AssignPhysicalRegisterToOperand(Dictionary<ushort, VirtualRegisterRecord> mapping,
            bool[] usedRegisters, int i, Operand operand)
        {
            if (operand.register == OperandRegister.VIRTUAL)
            {
                if (!mapping.ContainsKey(operand.virtual_register)) throw new CompileError("Virtual register not marked.");
                if (mapping[operand.virtual_register].assignedRegister == OperandRegister.A)
                {
                    //Assign a register to this virtual register.
                    int reg = -1;
                    for (int r = 0; r < 6; ++r)
                        if (usedRegisters[r] == false) reg = r;
                    if (reg == -1) throw new CompileError("Register spill.");
                    mapping[operand.virtual_register].assignedRegister = (OperandRegister)(reg + 1);
                    usedRegisters[reg] = true;
                }
                operand.register = mapping[operand.virtual_register].assignedRegister;
                if (mapping[operand.virtual_register].last_instruction == i)
                    usedRegisters[(int)(mapping[operand.virtual_register].assignedRegister) - 1] = false;
            }
        }

        public override void AssignRegisters(Dictionary<ushort, VirtualRegisterRecord> mapping)
        {
            mapping = new Dictionary<ushort, VirtualRegisterRecord>();
                       
            // Find the first and last mentions of each register.
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] is Instruction)
                {
                    var instruction = children[i] as Instruction;
                    MarkVirtualRegisterUsage(mapping, i * 2, instruction.firstOperand);
                    if (instruction.secondOperand != null) MarkVirtualRegisterUsage(mapping, (i * 2) + 1, instruction.secondOperand);
                }
            }

            bool[] usedRegisters = new bool[6] { false, false, false, false, false, false };
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] is Instruction)
                {

                    var instruction = children[i] as Instruction;

                    // Assigning registers in reverse allows the from register to un-mark it's real register.
                    // Then the destination register can recycle it in the same instruction.
                    // TODO: Make this change after some validation that it won't break things.
                    AssignPhysicalRegisterToOperand(mapping, usedRegisters, i * 2, instruction.firstOperand);

                    if (instruction.secondOperand != null) AssignPhysicalRegisterToOperand(mapping, usedRegisters, (i * 2) + 1, instruction.secondOperand);

                }
            }


            //var newMap = new Dictionary<ushort, VirtualRegisterRecord>();
            //foreach (var child in children) child.AssignRegisters(newMap);
        }
    }

    public class TransientNode : Node 
    {
        public override List<Node> CollapseTree(Peephole.Peepholes peepholes)
        {
            base.CollapseTree(peepholes);
            return children;
        }

        public override void EmitIR(EmissionStream stream)
        {
            stream.WriteLine("[transient node]");
            stream.indentDepth += 1;
            foreach (var child in children) child.EmitIR(stream);
            stream.indentDepth -= 1;
            stream.WriteLine("[/transient node]");

        }
    }
}
