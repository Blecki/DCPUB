using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class FunctionDeclarationNode : CompilableNode
    {
        public Function function = null;
        public List<Tuple<String,String>> parameters = new List<Tuple<String,String>>();
        public RegisterBank usedRegisters = new RegisterBank();
        protected Assembly.Label footerLabel = null;
        private Irony.Parsing.SourceSpan headerSpan;
        private int registersPreserved = 0;

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
            if (function.returnType == null) function.returnType = "word";
            ResultType = function.returnType;
        }

        public override string TreeLabel()
        {
            return "Function " + function.name + " " + function.parameterCount;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            (Child(0) as BlockNode).bypass = true;
            
            function.label = Assembly.Label.Make(function.name);
            footerLabel = Assembly.Label.Make(function.name + "_footer");
            if (enclosingScope.type != ScopeType.Global)
                throw new CompileError(this, "Functions must be at global scope.");
            enclosingScope.functions.Add(function);
            function.localScope.parent = enclosingScope;

            for (int i = parameters.Count - 1; i >= 0; --i)
            {
                var variable = new Variable();
                variable.scope = function.localScope;
                variable.name = parameters[i].Item1;
                variable.typeSpecifier = parameters[i].Item2;
                if (variable.typeSpecifier == null) variable.typeSpecifier = "word";
                function.localScope.variables.Add(variable);
                variable.type = VariableType.Local;
                
                    variable.stackOffset = i + 2; //Need an extra space for the return pointer and stored frame pointer
                    //function.localScope.variablesOnStack += 1;
                
            }

            base.GatherSymbols(context, function.localScope);
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
            
            base.AssignRegisters(context, localBank, Register.DISCARD);

            foreach (var nestedFunction in function.localScope.functions)
                nestedFunction.Node.AssignRegisters(context, parentState, Register.DISCARD);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Function was not removed by fold pass");
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Declaration of function " + function.name);
        }

        public virtual Assembly.Node CompileFunction(CompileContext context)
        {
            var body = new Assembly.Node();
            foreach (var child in ChildNodes)
                body.AddChild((child as CompilableNode).Emit(context, function.localScope));
            body.CollapseTree(context.peepholes);

            var registers = new RegisterBank();
            body.MarkRegisters(registers);
            var used = registers.registers.Count((rs) => rs == RegisterState.Used) - 2; //Why -2? Don't preserve A or J.
            body.AdjustVariableOffsets(-used);

            var r = new Assembly.Function
            {
                functionName = function.name,
                entranceLabel = function.label,
                parameterCount = function.parameterCount
            };

            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.headerSpan)));

            r.AddLabel(function.label);

            r.AddChild(new Assembly.Annotation("Save frame pointer in J"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Operand("J"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("J"), Operand("SP"));
            
            //Save registers
            for (int i = 1; i < 7; ++i)
                if (registers.registers[i] == RegisterState.Used)
                    r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Operand(Scope.GetRegisterLabelSecond(i)));

            r.AddChild(body);

            r.AddLabel(footerLabel);            
            r.AddInstruction(Assembly.Instructions.SET, Operand("SP"), Operand("J"));
            r.AddInstruction(Assembly.Instructions.SUB, Operand("SP"), Constant((ushort)used));

            //Restore registers
            for (int i = 6; i >= 1; --i)
                if (registers.registers[i] == RegisterState.Used)
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelSecond(i)), Operand("POP"));

            r.AddInstruction(Assembly.Instructions.SET, Operand("J"), Operand("POP"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Operand("POP"));

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));

            return r;
        }

        public void AssignVirtualRegisters(Assembly.Node root)
        {
            root.AssignRegisters(null);
        }

        public virtual Assembly.Node CompileFunction2(CompileContext context)
        {
            context.nextVirtualRegister = 0;
            var body = new Assembly.Node();
            foreach (var child in ChildNodes)
                body.AddChild((child as CompilableNode).Emit2(context, function.localScope, Target.Discard));
            body.CollapseTree(context.peepholes);

            if (!context.options.skip_virtual_register_assignment) AssignVirtualRegisters(body);

            var registers = new RegisterBank();
            body.MarkRegisters(registers);
            var used = registers.registers.Count((rs) => rs == RegisterState.Used) - 2; //Why -2? Don't preserve A or J.
            body.AdjustVariableOffsets(-used);

            var r = new Assembly.Function
            {
                functionName = function.name,
                entranceLabel = function.label,
                parameterCount = function.parameterCount
            };

            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.headerSpan)));

            r.AddLabel(function.label);

            r.AddChild(new Assembly.Annotation("Save frame pointer in J"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Operand("J"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("J"), Operand("SP"));

            //Save registers
            for (int i = 1; i < 7; ++i)
                if (registers.registers[i] == RegisterState.Used)
                    r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), Operand(Scope.GetRegisterLabelSecond(i)));
            
            r.AddChild(body);

            r.AddLabel(footerLabel);
            r.AddInstruction(Assembly.Instructions.SET, Operand("SP"), Operand("J"));
            r.AddInstruction(Assembly.Instructions.SUB, Operand("SP"), Constant((ushort)used));

            //Restore registers
            for (int i = 6; i >= 1; --i)
                if (registers.registers[i] == RegisterState.Used)
                    r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelSecond(i)), Operand("POP"));

            r.AddInstruction(Assembly.Instructions.SET, Operand("J"), Operand("POP"));
            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Operand("POP"));

            foreach (var nestedFunction in function.localScope.functions)
                r.AddChild(nestedFunction.Node.CompileFunction2(context));

            return r;
        }

        internal virtual Assembly.Node CompileReturn(CompileContext context, Scope localScope)
        {
            var r = new Assembly.TransientNode();
            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(footerLabel));
            return r;
        }

        internal virtual Assembly.Node CompileReturn2(CompileContext context, Scope localScope)
        {
            var r = new Assembly.TransientNode();
            r.AddInstruction(Assembly.Instructions.SET, Operand("PC"), Label(footerLabel));
            return r;
        }
    }
}
