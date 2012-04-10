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
        public Register location;
        public string staticLabel;
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
            return "L" + nextLabelID++;
        }

        internal const string TempRegister = "J";

        internal Scope parent = null;
        internal int parentDepth = 0;
        public List<Variable> variables = new List<Variable>();
        public int stackDepth = 0;
        public List<FunctionDeclarationNode> pendingFunctions = new List<FunctionDeclarationNode>();
        public FunctionDeclarationNode activeFunction = null;
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Free, 0, 0, 0, 0, 0, 0, RegisterState.Used };

        public static void Reset()
        {
            nextLabelID = 0;
            dataElements.Clear();
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

        public static List<Tuple<string, List<ushort>>> dataElements = new List<Tuple<string, List<ushort>>>();
        public static void AddData(string label, List<ushort> data)
        {
            dataElements.Add(new Tuple<string, List<ushort>>(label, data));
        }

        private static Irony.Parsing.Parser Parser = new Irony.Parsing.Parser(new DCPUC.Grammar());
            
        private static String extractLine(String s, int c)
        {
            int lc = 0;
            int p = 0;
            while (p < s.Length && lc < c)
            {
                if (s[p] == '\n') lc++;
                ++p;
            }

            int ls = p;
            while (p < s.Length && s[p] != '\n') ++p;

            return s.Substring(ls, p - ls);
        }
        public static void CompileRoot(String code, Assembly assembly, Action<string> onError)
        {
            var program = Parser.Parse(code);
            if (onError == null) onError = (a) => {};
            if (program.HasErrors())
            {
                foreach (var msg in program.ParserMessages)
                {
                    onError(msg.Level + ": " + msg.Message + " [line:" + msg.Location.Line + " column:" + msg.Location.Column + "]\r\n");
                    onError(extractLine(code, msg.Location.Line) + "\r\n");
                    onError(new String(' ', msg.Location.Column) + "^\r\n");
                }
                return;
            }

            DCPUC.Scope.Reset();
            var root = program.Root.AstNode as DCPUC.CompilableNode; 
            var scope = new DCPUC.Scope();
            var end_of_program = new Variable();
            end_of_program.location = Register.STATIC;
            end_of_program.name = "__endofprogram";
            end_of_program.staticLabel = "ENDOFPROGRAM";
            scope.variables.Add(end_of_program);

            var library = new List<String>(System.IO.File.ReadAllLines("libdcpuc.txt"));
            root.InsertLibrary(library);

            try
            {
                root.Compile(assembly, scope, DCPUC.Register.DISCARD);
                assembly.Add("BRK", "", "", "Non-standard");
                foreach (var pendingFunction in scope.pendingFunctions)
                    pendingFunction.CompileFunction(assembly, scope);
                foreach (var dataItem in DCPUC.Scope.dataElements)
                {
                    var datString = "";
                    foreach (var item in dataItem.Item2)
                    {
                        datString += DCPUC.CompilableNode.hex(item);
                        datString += ", ";
                    }
                    assembly.Add(":" + dataItem.Item1, "DAT", datString.Substring(0, datString.Length - 2));
                }
                assembly.Add(":ENDOFPROGRAM", "", "");
            }
            catch (DCPUC.CompileError c)
            {
                onError(c.Message);
                return;
            }

        }
    }
}
