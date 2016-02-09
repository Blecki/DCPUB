﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Model
{
    public class Struct
    {
        public String name;
        public List<Member> members = new List<Member>();
        public Ast.StructDeclarationNode Node;
        public int size;
    }
}