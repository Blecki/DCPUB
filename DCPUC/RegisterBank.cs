using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class RegisterBank
    {
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Used, 0, 0, 0, 0, 0, 0, RegisterState.Used };
        internal RegisterBank functionBank = null;

        public RegisterBank Push()
        {
            var r = new RegisterBank();
            for (int i = 0; i < 8; ++i) r.registers[i] = registers[i];
            r.functionBank = functionBank;
            return r;
        }

        internal Register FindFreeRegister()
        {
            for (int i = 0; i < 8; ++i) if (registers[i] == RegisterState.Free) return (Register)i;
            //for (int i = 0; i < 3; ++i) if (registers[i] == RegisterState.Free) return (Register)i;
            return Register.STACK;
        }

        internal bool RegisterInUse(Register r)
        {
            return registers[(int)r] == RegisterState.Used;
        }

        internal void FreeRegister(Register r) { registers[(int)r] = RegisterState.Free; }

        public void UseRegister(Register r) 
        { 
            registers[(int)r] = RegisterState.Used;
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

        internal Register FindAndUseFreeRegister()
        {
            var r = FindFreeRegister();
            if (Scope.IsRegister((Register)r)) UseRegister(r);
            return (Register)r;
        }

        internal void FreeMaybeRegister(Register r) 
        { 
            if (Scope.IsRegister(r))
                FreeRegister(r);
        }

        internal void FreeRegisters(params Register[] rs)
        {
            for (int i = 0; i < rs.Length; ++i) FreeMaybeRegister(rs[i]);
        }

    }
}
