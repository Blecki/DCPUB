using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionDeclarationNode : CompilableNode
    {
        public Function function = null;
        public List<Tuple<String,String>> parameters = new List<Tuple<String,String>>();
        public RegisterBank usedRegisters = new RegisterBank();
        protected String footerLabel = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Block", treeNode.ChildNodes[3]);
            foreach (var parameter in treeNode.ChildNodes[2].ChildNodes)
            {
                var name = parameter.ChildNodes[0].FindTokenAndGetText();
                var type = parameter.ChildNodes[1].FindTokenAndGetText();
                parameters.Add(new Tuple<string, string>(name, type));
            }
            function = new Function();
            function.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            function.Node = this;
            function.parameterCount = parameters.Count;
            function.localScope = new Scope();
            function.localScope.type = ScopeType.Function;
            function.localScope.activeFunction = this;
        }

        public override string TreeLabel()
        {
            return "Function " + function.name + " " + function.parameterCount;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            function.label = context.GetLabel() + function.name;
            footerLabel = context.GetLabel() + function.name + "_footer";
            enclosingScope.functions.Add(function);
            function.localScope.parent = enclosingScope;

            for (int i = 0; i < parameters.Count; ++i)
            {
                var variable = new Variable();
                variable.scope = function.localScope;
                variable.name = parameters[i].Item1;
                variable.typeSpecifier = parameters[i].Item2;
                function.localScope.variables.Add(variable);

                if (i < 3)
                {
                    variable.location = (Register)i;
                    function.localScope.UseRegister(i);
                }
                else
                {
                    variable.location = Register.STACK;
                    variable.stackOffset = function.localScope.stackDepth;
                    function.localScope.stackDepth += 1;
                }
            }

            Child(0).GatherSymbols(context, function.localScope);
        }

        public override CompilableNode FoldConstants()
        {
            base.FoldConstants();
            return null;
        }

        public override void AssignRegisters(RegisterBank parentState, Register target)
        {
            var localBank = new RegisterBank();
            localBank.functionBank = usedRegisters;
            for (int i = 0; i < 3 && i < function.parameterCount; ++i)
                localBank.UseRegister((Register)i);

            Child(0).AssignRegisters(localBank, Register.DISCARD);

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.AssignRegisters(parentState, Register.DISCARD);
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            throw new CompileError("Function was not removed by Fold pass");
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Function was not removed by fold pass");
        }

        public virtual void CompileFunction(CompileContext context)
        {
            function.localScope.stackDepth += 1;

            context.Add(":" + function.label, "", "");

            //Save registers
            //ABC saved by caller
            for (int i = Math.Min(3, function.parameterCount); i < 7; ++i)
                if (usedRegisters.registers[i] == RegisterState.Used)
                {
                    context.Add("SET", "PUSH", Scope.GetRegisterLabelSecond(i));
                    function.localScope.stackDepth += 1;
                }

            var localScope = function.localScope.Push();

            Child(0).Emit(context, localScope);
            context.Add(":" + footerLabel, "", "");
            context.Barrier();

            //Restore registers
            for (int i = 6; i >= Math.Min(3, function.parameterCount); --i)
                if (usedRegisters.registers[i] == RegisterState.Used)
                    context.Add("SET", Scope.GetRegisterLabelSecond(i), "POP");

            context.Add("SET", "PC", "POP");
            context.Barrier();

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.CompileFunction(context);
        }

        internal virtual void CompileReturn(CompileContext context, Scope localScope)
        {
            if (localScope.stackDepth - function.localScope.stackDepth > 0)
                context.Add("ADD", "SP", Hex.hex(localScope.stackDepth - function.localScope.stackDepth), "Cleanup stack"); 
            context.Add("SET", "PC", footerLabel, "Return");
        }
    }
}
