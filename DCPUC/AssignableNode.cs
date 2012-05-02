using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public interface AssignableNode
    {
        bool IsAssignedTo { set; }
        void EmitAssignment(CompileContext context, Scope scope, Register from, String opcode);
    }
}
