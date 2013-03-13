using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public interface AssignableNode
    {
        bool IsAssignedTo { set; }
        Assembly.Node EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode);
        Assembly.Node EmitAssignment2(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode);
    }
}
