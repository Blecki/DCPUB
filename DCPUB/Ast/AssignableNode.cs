﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Ast
{
    public interface AssignableNode
    {
        Intermediate.IRNode EmitAssignment(CompileContext context, Model.Scope scope, Intermediate.Operand from, Intermediate.Instructions opcode);
    }
}
