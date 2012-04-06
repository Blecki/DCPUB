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

    public enum Register
    {
        A = 0,
        B = 1,
        C = 2,
        X = 3,
        Y = 4,
        Z = 5,
        I = 6,
        J = 7,
        STACK = 8,
        DISCARD = 9,
    }

    public enum RegisterState
    {
        Free = 0,
        Used = 1
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
        internal RegisterState[] registers = new RegisterState[] { 0, 0, 0, 0, 0, 0, 0, 0 };

        internal Scope Push(Scope child)
        {
            child.parent = this;
            child.stackDepth = stackDepth;
            child.parentDepth = stackDepth;
            child.activeFunction = activeFunction;
            for (int i = 0; i < 8; ++i) child.registers[i] = registers[i];
            return child;
        }

        internal Variable FindVariable(string name)
        {
            foreach (var variable in variables)
                if (variable.name == name) return variable;
            if (parent != null) return parent.FindVariable(name);
            return null;
        }

        internal int FindFreeRegister()
        {
            for (int i = 0; i < 8; ++i) if (registers[i] == RegisterState.Free) return i;
            return -1;
        }

        internal string GetRegisterLabel(int r) { return ((Register)r).ToString(); }
        internal void FreeRegister(int r) { registers[r] = RegisterState.Free; }
        internal void UseRegister(int r) { registers[r] = RegisterState.Used; }
        internal bool IsRegister(Register r) { return (int)(r) <= 7; }
    }
}
