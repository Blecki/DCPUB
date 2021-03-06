﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Model
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
        public Intermediate.Label staticLabel;
        public int constantValue;
        public string typeSpecifier = "word";
        public bool addressTaken = false;
        public Struct structType = null;
        public VariableType type;
        public bool isArray = false;

        public Ast.CompilableNode assignedBy = null;
    }

    
}
