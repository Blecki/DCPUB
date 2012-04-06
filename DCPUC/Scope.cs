using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class Variable
    {
        public String name;
        public Scope scope;
        public int stackOffset;
    }

    public class Scope
    {
        private static int nextLabelID = 0;
        public static String GetLabel()
        {
            return "LABEL" + nextLabelID++;
        }


        internal Scope parent = null;
        internal int parentDepth = 0;
        internal List<Variable> variables = new List<Variable>();
        internal int stackDepth = 0;
        public List<FunctionDeclarationNode> pendingFunctions = new List<FunctionDeclarationNode>();
        internal FunctionDeclarationNode activeFunction = null;

        internal Scope Push(Scope child)
        {
            child.parent = this;
            child.stackDepth = stackDepth;
            child.parentDepth = stackDepth;
            child.activeFunction = activeFunction;
            return child;
        }

        internal Variable FindVariable(string name)
        {
            foreach (var variable in variables)
                if (variable.name == name) return variable;
            if (parent != null) return parent.FindVariable(name);
            return null;
        }
    }
}
