using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public enum VariableType
    {
        Local,
        Static,
        Constant,
        ConstantLabel,
        External
    }

    public class Variable
    {
        public String name;
        public Scope scope;
        public int stackOffset;
        public Register location;
        public Assembly.Label staticLabel;
        public int constantValue;
        public string typeSpecifier = "unsigned";
        public bool addressTaken = false;
        public Struct structType = null;
        public VariableType type;

        public CompilableNode assignedBy = null;
    }

    
}
