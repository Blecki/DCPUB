using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public interface AssignableNode
    {
        Intermediate.IRNode EmitAssignment(CompileContext context, Scope scope, Intermediate.Operand from, Intermediate.Instructions opcode);
    }
}
