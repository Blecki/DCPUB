﻿using System;
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
        ConstantReference
    }

    public class Variable
    {
        public String name;
        public Scope scope;
        public int stackOffset;
        public Register location;
        public string staticLabel;
        public int constantValue;
        public string typeSpecifier = "unsigned";
        public bool addressTaken = false;
        public Struct structType = null;
        public VariableType type;

        public CompilableNode assignedBy = null;
    }

    
}