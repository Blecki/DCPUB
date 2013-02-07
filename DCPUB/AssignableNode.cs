using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public interface AssignableNode
    {
        bool IsAssignedTo { set; }
        Assembly.Node EmitAssignment(CompileContext context, Scope scope, Register from, Assembly.Instructions opcode);
    }
}
