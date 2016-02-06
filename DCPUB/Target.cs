using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public enum TargetUsage
    {
        Pop,
        Push,
        Peek
    }

    public enum Targets
    {
        Stack,
        Register,
        Discard,
        Raw,
    }

    public class Target
    {
        public Targets target = Targets.Stack;
        public int virtualId = 0;

        public static Target Stack { get { return new Target { target = Targets.Stack }; } }
        public static Target Discard { get { return new Target { target = Targets.Discard }; } }
        public static Target Register(int id) { return new Target { target = Targets.Register, virtualId = id }; }
        public static Target Raw(Register r) { return new Target { target = Targets.Raw, virtualId = (int)r }; }

        public Assembly.Operand GetOperand(TargetUsage usage,
            Assembly.OperandSemantics semantics = Assembly.OperandSemantics.None, 
            ushort offset = 0)
        {
            if (target == Targets.Stack)
            {
                switch (usage)
                {
                    case TargetUsage.Peek: return CompilableNode.Operand("PEEK", semantics, offset);
                    case TargetUsage.Push: return CompilableNode.Operand("PUSH", semantics, offset);
                    case TargetUsage.Pop: return CompilableNode.Operand("POP", semantics, offset);
                }
            }
            else if (target == Targets.Discard)
                throw new InternalError("Unable to get operand from target with semantic 'discard'.");
            else if (target == Targets.Raw)
                return new Assembly.Operand
                {
                    register = (Assembly.OperandRegister)virtualId,
                    semantics = semantics,
                    constant = offset
                };
            else
                return CompilableNode.Virtual(virtualId, semantics, offset);
            throw new InternalError("Unknown error while getting operand from target.");
        }
    }
}
