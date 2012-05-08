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
        private Irony.Parsing.SourceSpan headerSpan;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            headerSpan = new Irony.Parsing.SourceSpan(this.Span.Location,
                treeNode.ChildNodes[2].Span.EndPosition - this.Span.Location.Position);

            AddChild("Block", treeNode.ChildNodes[4]);
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
            function.returnType = treeNode.ChildNodes[3].FindTokenAndGetText();
            if (function.returnType == null) function.returnType = "unsigned";
            ResultType = function.returnType;
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

            for (int i = parameters.Count - 1; i >= 0; --i)
            {
                var variable = new Variable();
                variable.scope = function.localScope;
                variable.name = parameters[i].Item1;
                variable.typeSpecifier = parameters[i].Item2;
                if (variable.typeSpecifier == null) variable.typeSpecifier = "unsigned";
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

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            foreach (var child in ChildNodes)
                (child as CompilableNode).ResolveTypes(context, function.localScope);
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            return null;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            var localBank = new RegisterBank();
            localBank.functionBank = usedRegisters;
            for (int i = 0; i < 3 && i < function.parameterCount; ++i)
                localBank.UseRegister((Register)i);

            Child(0).AssignRegisters(context, localBank, Register.DISCARD);

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.AssignRegisters(context, parentState, Register.DISCARD);
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            throw new CompileError("Function was not removed by Fold pass");
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Function was not removed by fold pass");
        }

        public virtual Assembly.Node CompileFunction(CompileContext context)
        {
            var r = new Assembly.Function
            {
                functionName = function.name,
                entranceLabel = function.label,
                parameterCount = function.parameterCount
            };

            function.localScope.stackDepth += 1;

            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.headerSpan)));

            r.AddLabel(function.label);

            //Save registers
            //ABC saved by caller
            for (int i = Math.Min(3, function.parameterCount); i < 7; ++i)
                if (usedRegisters.registers[i] == RegisterState.Used)
                {
                    r.AddInstruction(Assembly.Instructions.SET, "PUSH", Scope.GetRegisterLabelSecond(i));
                    function.localScope.stackDepth += 1;
                }

            var localScope = function.localScope.Push();

            for (int i = 0; i < Math.Min(3, function.parameterCount); ++i)
            {
                if (function.localScope.variables[i].addressTaken)
                {
                    r.AddInstruction(Assembly.Instructions.SET, "PUSH", Scope.GetRegisterLabelSecond((int)function.localScope.variables[i].location));
                    function.localScope.variables[i].location = Register.STACK;
                    function.localScope.variables[i].stackOffset = localScope.stackDepth;
                    localScope.stackDepth += 1;
                }
            }

            r.AddChild(Child(0).Emit(context, localScope));

            if (localScope.stackDepth - function.localScope.stackDepth > 0)
                r.AddInstruction(Assembly.Instructions.ADD, "SP", Hex.hex(localScope.stackDepth - function.localScope.stackDepth));

            r.AddLabel(footerLabel);

            //Restore registers
            for (int i = 6; i >= Math.Min(3, function.parameterCount); --i)
                if (usedRegisters.registers[i] == RegisterState.Used)
                    r.AddInstruction(Assembly.Instructions.SET, Scope.GetRegisterLabelSecond(i), "POP");

            r.AddInstruction(Assembly.Instructions.SET, "PC", "POP");

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));

            return r;
        }

        internal virtual Assembly.Node CompileReturn(CompileContext context, Scope localScope)
        {
            var r = new Assembly.Node();
            if (localScope.stackDepth - function.localScope.stackDepth > 0)
                r.AddInstruction(Assembly.Instructions.ADD, "SP", Hex.hex(localScope.stackDepth - function.localScope.stackDepth)); 
            r.AddInstruction(Assembly.Instructions.SET, "PC", footerLabel);
            return r;
        }
    }
}
