﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB
{
    public interface AssignableNode
    {
        Assembly.Node EmitAssignment(CompileContext context, Scope scope, Assembly.Operand from, Assembly.Instructions opcode);
    }
}
