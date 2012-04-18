using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

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
        public bool emitBrackets = true;
        public int constantValue;
        public string typeSpecifier = "unsigned";

        public VariableType type;
    }

    public class Function
    {
        public String name;
        public FunctionDeclarationNode Node;
        public Scope localScope;
        public String label;
        public int parameterCount = 0;
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
        STATIC = 10,
        CONST = 11,
    }

    public enum RegisterState
    {
        Free = 0,
        Used = 1
    }

    public enum ScopeType
    {
        Global,
        Function,
        Branch
    }

    public class Scope
    {
        internal const string TempRegister = "J";

        internal ScopeType type = ScopeType.Global;
        internal Scope parent = null;
        internal int parentDepth = 0;
        public List<Variable> variables = new List<Variable>();
        public List<Function> functions = new List<Function>();
        public int stackDepth = 0;
        public List<FunctionDeclarationNode> pendingFunctions = new List<FunctionDeclarationNode>();
        public FunctionDeclarationNode activeFunction = null;
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Free, 0, 0, 0, 0, 0, 0, RegisterState.Used };

        internal int StackOffset(int of)
        {
            return stackDepth - of - 1;
        }

        internal Scope Push(Scope child)
        {
            child.parent = this;
            child.stackDepth = stackDepth;
            child.parentDepth = stackDepth;
            child.activeFunction = activeFunction;
            for (int i = 0; i < 8; ++i) child.registers[i] = registers[i];
            return child;
        }

        internal Scope Push()
        {
            var child = new Scope();
            return Push(child);
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
            for (int i = 3; i < 8; ++i) if (registers[i] == RegisterState.Free) return i;
            for (int i = 0; i < 3; ++i) if (registers[i] == RegisterState.Free) return i;
            return (int)Register.STACK;
        }

        internal static string GetRegisterLabelFirst(int r) { if (r == (int)Register.STACK) return "PUSH"; else return ((Register)r).ToString(); }
        internal static string GetRegisterLabelSecond(int r) { if (r == (int)Register.STACK) return "POP"; else return ((Register)r).ToString(); }
        internal void FreeRegister(int r) { registers[r] = RegisterState.Free; }
        public void UseRegister(int r) { registers[r] = RegisterState.Used; }
        internal static bool IsRegister(Register r) { return (int)(r) <= 7; }

        internal RegisterState[] SaveRegisterState()
        {
            var r = new RegisterState[8];
            for (int i = 0; i < 8; ++i) r[i] = registers[i];
            return r;
        }

        internal void RestoreRegisterState(RegisterState[] state)
        {
            registers = state;
        }

        internal int FindAndUseFreeRegister()
        {
            var r = FindFreeRegister();
            if (IsRegister((Register)r)) UseRegister(r);
            return r;
        }

        internal void FreeMaybeRegister(int r) { if (IsRegister((Register)r)) FreeRegister(r); }

    }
}
