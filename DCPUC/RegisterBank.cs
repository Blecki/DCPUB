using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class RegisterBank
    {
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Free, 0, 0, 0, 0, 0, 0, RegisterState.Used };
        internal RegisterBank functionBank = null;

        public RegisterBank Push()
        {
            var r = new RegisterBank();
            for (int i = 0; i < 8; ++i) r.registers[i] = registers[i];
            r.functionBank = functionBank;
            return r;
        }

        internal int FindFreeRegister()
        {
            for (int i = 3; i < 8; ++i) if (registers[i] == RegisterState.Free) return i;
            for (int i = 0; i < 3; ++i) if (registers[i] == RegisterState.Free) return i;
            return (int)Register.STACK;
        }

        internal void FreeRegister(int r) { registers[r] = RegisterState.Free; }

        public void UseRegister(int r) 
        { 
            registers[r] = RegisterState.Used;
            if (functionBank != null) functionBank.UseRegister(r);
        }

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
            if (Scope.IsRegister((Register)r)) UseRegister(r);
            return r;
        }

        internal void FreeMaybeRegister(int r) { if (Scope.IsRegister((Register)r)) FreeRegister(r); }

    }
}
