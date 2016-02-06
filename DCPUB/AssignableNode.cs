using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public interface AssignableNode
    {
        Assembly.IRNode EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode);
    }
}
