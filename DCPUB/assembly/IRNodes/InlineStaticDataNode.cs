using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Assembly
{
    public class InlineStaticData : IRNode
    {
        public List<Operand> data = new List<Operand>();

        public override void Emit(EmissionStream stream)
        {
            var str = "DAT " + String.Join(" ", data);
            stream.WriteLine(str);
        }

        public override void EmitIR(EmissionStream stream)
        {
            var str = "DAT " + String.Join(" ", data);
            stream.WriteLine(str);
        }

        public override void SetupLabels(Dictionary<string, Label> labelTable)
        {
            foreach (var op in data)
            {
                if ((op.semantics & OperandSemantics.Label) == OperandSemantics.Label && op.label.rawLabel[0] != '\"')
                    op.label = labelTable[op.label.rawLabel];
            }
        }

        public override void EmitBinary(List<Box<ushort>> binary)
        {
            foreach (var op in data)
            {
                if ((op.semantics & OperandSemantics.Label) == OperandSemantics.Label)
                {
                    if (op.label.rawLabel[0] == '\"')
                        foreach (var c in op.label.rawLabel.Substring(1, op.label.rawLabel.Length - 2))
                            binary.Add(new Box<ushort> { data = (ushort)c });
                    else
                        binary.Add(op.label.position);
                }
                else
                    binary.Add(new Box<ushort> { data = op.constant });
            }
        }
    }
}