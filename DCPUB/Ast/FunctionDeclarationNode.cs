using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Ast
{
    public class FunctionDeclarationNode : CompilableNode
    {
        public Model.Function function = null;
        public List<Tuple<String,String>> parameters = new List<Tuple<String,String>>();
        protected Intermediate.Label footerLabel = null;
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
            function = new Model.Function();
            function.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            function.Node = this;
            function.parameterCount = parameters.Count;
            function.localScope = new Model.Scope();
            function.localScope.type = Model.ScopeType.Function;
            function.localScope.activeFunction = this;
            function.returnType = treeNode.ChildNodes[3].FindTokenAndGetText();
            if (function.returnType == null) function.returnType = "word";
            ResultType = function.returnType;
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            (Child(0) as BlockNode).bypass = true;

            function.LabelName = enclosingScope.activeFunction.function.LabelName + "_" + function.name;
            function.label = Intermediate.Label.Make(function.LabelName);
            footerLabel = Intermediate.Label.Make(function.name + "_footer");

            if (enclosingScope.type != Model.ScopeType.Global)
                context.AddWarning(this, "Experimental feature: Function declared within function.");

            enclosingScope.functions.Add(function);
            enclosingScope.activeFunction.function.SubordinateFunctions.Add(function);
            function.localScope.parent = enclosingScope;

            for (int i = parameters.Count - 1; i >= 0; --i)
            {
                var variable = new Model.Variable();
                variable.scope = function.localScope;
                variable.name = parameters[i].Item1;
                variable.typeSpecifier = parameters[i].Item2;
                if (variable.typeSpecifier == null) variable.typeSpecifier = "word";
                function.localScope.variables.Add(variable);
                variable.type = Model.VariableType.Local;
                
                    variable.stackOffset = i + 2; //Need an extra space for the return pointer and stored frame pointer
                    //function.localScope.variablesOnStack += 1;
                
            }

            base.GatherSymbols(context, function.localScope);
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            foreach (var child in ChildNodes)
                (child as CompilableNode).ResolveTypes(context, function.localScope);
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            return new Intermediate.Annotation("Declaration of function " + function.name);
        }

        public virtual Intermediate.IRNode CompileFunction(CompileContext context)
        {
            if (context.options.strip && !function.reached)
                return new Intermediate.Annotation("Function " + function.name + " stripped.");

            context.nextVirtualRegister = 0;
            var body = new Intermediate.IRNode();
            foreach (var child in ChildNodes)
                body.AddChild((child as CompilableNode).Emit(context, function.localScope, Target.Discard));

            body.CollapseTransientNodes();

            body.PeepholeTree(context.peepholes);

            var registers = new bool[] { true, false, false, false, false, false, false, true };
            var used = 0;

            // If we are emitting intermediate representation, we don't want to replace virtual registers.
            if (!context.options.emit_ir)
            {
                body.AssignRegisters(null);
                body.MarkUsedRealRegisters(registers);
                used = registers.Count((rs) => rs == true) - 2; //Why -2? Don't preserve A or J.
                body.CorrectVariableOffsets(-used);
            }

            var r = new Intermediate.Function
            {
                functionName = function.name,
                entranceLabel = function.label,
                parameterCount = function.parameterCount
            };

            r.AddChild(new Intermediate.Annotation(context.GetSourceSpan(this.headerSpan)));

            r.AddLabel(function.label);

            r.AddChild(new Intermediate.Annotation("Save frame pointer in J"));
            r.AddInstruction(Intermediate.Instructions.SET, Operand("PUSH"), Operand("J"));
            r.AddInstruction(Intermediate.Instructions.SET, Operand("J"), Operand("SP"));

            //Save registers
            for (int i = 1; i < 7; ++i)
                if (registers[i])
                    r.AddInstruction(Intermediate.Instructions.SET, Operand("PUSH"), Operand((Model.Register)i));
            
            r.AddChild(body);

            r.AddLabel(footerLabel);
            r.AddInstruction(Intermediate.Instructions.SET, Operand("SP"), Operand("J"));
            if (used != 0)
                r.AddInstruction(Intermediate.Instructions.SUB, Operand("SP"), Constant((ushort)used));

            //Restore registers
            for (int i = 6; i >= 1; --i)
                if (registers[i])
                    r.AddInstruction(Intermediate.Instructions.SET, Operand((Model.Register)i), Operand("POP"));

            r.AddInstruction(Intermediate.Instructions.SET, Operand("J"), Operand("POP"));
            r.AddInstruction(Intermediate.Instructions.SET, Operand("PC"), Operand("POP"));

            foreach (var nestedFunction in function.SubordinateFunctions)
                r.AddChild(nestedFunction.Node.CompileFunction(context));

            return r;
        }

        internal virtual Intermediate.IRNode CompileReturn(CompileContext context, Model.Scope localScope)
        {
            var r = new Intermediate.TransientNode();
            r.AddInstruction(Intermediate.Instructions.SET, Operand("PC"), Label(footerLabel));
            return r;
        }
    }
}
