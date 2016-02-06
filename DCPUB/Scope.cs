using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class Label
    {
        public String declaredName;
        public Assembly.Label realName;
    }

    public class Function
    {
        public String name;
        public FunctionDeclarationNode Node;
        public Scope localScope;
        public Assembly.Label label;
        public string LabelName;
        public int parameterCount = 0;
        public String returnType = "void";
        public List<Label> labels = new List<Label>();

        public bool reached = false;
        public List<Function> Calls = new List<Function>();
        public List<Function> SubordinateFunctions = new List<Function>();

        public void MarkReachableFunctions()
        {
            if (reached) return;
            reached = true;
            foreach (var child in Calls) child.MarkReachableFunctions();
        }
    }

    public class Member
    {
        public String name;
        public String typeSpecifier;
        public Struct referencedStruct;
        public int offset;
        public int size;
        public bool isArray;
    }

    public class Struct
    {
        public String name;
        public List<Member> members = new List<Member>();
        public StructDeclarationNode Node;
        public int size;
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
        internal ScopeType type = ScopeType.Global;
        internal Scope parent = null;
        internal int parentDepth = 0;
        public List<Variable> variables = new List<Variable>();
        public List<Function> functions = new List<Function>();
        public List<Struct> structs = new List<Struct>();
        public int variablesOnStack = 0;
        public List<FunctionDeclarationNode> pendingFunctions = new List<FunctionDeclarationNode>();
        public FunctionDeclarationNode activeFunction = null;
        public BlockNode activeBlock = null;
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Free, 0, 0, 0, 0, 0, 0, RegisterState.Used };

        internal Scope Push(Scope child)
        {
            child.parent = this;
            child.variablesOnStack = variablesOnStack;
            child.parentDepth = variablesOnStack;
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

        internal Function FindFunction(string name)
        {
            foreach (var function in functions)
                if (function.name == name) return function;
            if (parent != null) return parent.FindFunction(name);
            return null;
        }

        internal static bool IsBuiltIn(String s)
        {
            if (s == "word") return true;
            return false;
        }

        public Struct FindType(string s)
        {
            foreach (var @struct in structs)
                if (@struct.name == s) return @struct;
            if (parent != null) return parent.FindType(s);
            return null;
        }
    }
}
