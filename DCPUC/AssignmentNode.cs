using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

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
                opcodes = new Dictionary<string, string>();
                opcodes.Add("=", "SET");
                opcodes.Add("+=", "ADD");
                opcodes.Add("-=", "SUB");
                opcodes.Add("*=", "MUL");
                opcodes.Add("/=", "DIV");

                opcodes.Add("*=signed", "MLI");
                opcodes.Add("/=signed", "DVI");

                opcodes.Add("%=", "MOD");
                opcodes.Add("<<=", "SHL");
                opcodes.Add(">>=", "SHR");
                opcodes.Add("&=", "AND");
                opcodes.Add("|=", "BOR");
                opcodes.Add("^=", "XOR");
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
        
        public override void Emit(CompileContext context, Scope scope)
        {
            Child(1).Emit(context, scope);

            var opcode = "";
            if (opcodes.ContainsKey(@operator + ResultType)) opcode = opcodes[@operator + ResultType];
            else opcode = opcodes[@operator];

            (Child(0) as AssignableNode).EmitAssignment(context, scope, rvalueTargetRegister, opcode);
            if (rvalueTargetRegister == Register.STACK)
                scope.stackDepth -= 1;

        }
    }

    
}
