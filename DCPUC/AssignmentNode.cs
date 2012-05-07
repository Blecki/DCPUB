using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        private static Dictionary<String, Assembly.Instructions> opcodes = null;

        public Register rvalueTargetRegister = Register.STACK;
        public Register lvalueTargetRegister = Register.STACK;
        public Variable assignTo = null;
        public Boolean dereferenceLvalue = false;
        public String @operator;
        
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("LValue", treeNode.ChildNodes[0].ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);
            @operator = treeNode.ChildNodes[1].FindTokenAndGetText();
            ResultType = "void";

            if (opcodes == null)
            {
                opcodes = new Dictionary<string, Assembly.Instructions>();
                opcodes.Add("=", Assembly.Instructions.SET);
                opcodes.Add("+=", Assembly.Instructions.ADD);
                opcodes.Add("-=", Assembly.Instructions.SUB);
                opcodes.Add("*=", Assembly.Instructions.MUL);
                opcodes.Add("/=", Assembly.Instructions.DIV);

                opcodes.Add("*=signed", Assembly.Instructions.MLI);
                opcodes.Add("/=signed", Assembly.Instructions.DVI);

                opcodes.Add("%=", Assembly.Instructions.MOD);
                opcodes.Add("<<=", Assembly.Instructions.SHL);
                opcodes.Add(">>=", Assembly.Instructions.SHR);
                opcodes.Add("&=", Assembly.Instructions.AND);
                opcodes.Add("|=", Assembly.Instructions.BOR);
                opcodes.Add("^=", Assembly.Instructions.XOR);
            }
        }

        public override string TreeLabel()
        {
            return @operator + " " + "[r:" + rvalueTargetRegister.ToString() + "]";
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            Child(0).GatherSymbols(context, enclosingScope);
            if (Child(0) is VariableNameNode)
            {
                var assignTo = (Child(0) as VariableNameNode).variable;
                if (assignTo.type == VariableType.Constant || assignTo.type == VariableType.ConstantReference)
                    throw new CompileError("Can't assign to constants");
            }
            
            Child(1).GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            Child(1).ResolveTypes(context, enclosingScope);
            if (Child(0).ResultType != Child(1).ResultType)
                context.AddWarning(Span, CompileContext.TypeWarning(Child(1).ResultType, Child(0).ResultType));
            ResultType = Child(0).ResultType;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (target != Register.DISCARD)
                throw new CompileError("Assignment should always target discard");

            rvalueTargetRegister = parentState.FindAndUseFreeRegister();
            (Child(0) as AssignableNode).IsAssignedTo = true;
            Child(0).AssignRegisters(context, parentState, Register.DISCARD);
            Child(1).AssignRegisters(context, parentState, rvalueTargetRegister);
            parentState.FreeMaybeRegister(rvalueTargetRegister);

        }
        
        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            r.AddChild(Child(1).Emit(context, scope));

            var opcode = Assembly.Instructions.SET;
            if (opcodes.ContainsKey(@operator + ResultType)) opcode = opcodes[@operator + ResultType];
            else opcode = opcodes[@operator];

            r.AddChild((Child(0) as AssignableNode).EmitAssignment(context, scope, rvalueTargetRegister, opcode));
            if (rvalueTargetRegister == Register.STACK)
                scope.stackDepth -= 1;
            return r;
        }
    }

    
}
