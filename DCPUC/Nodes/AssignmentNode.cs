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
            ResultType = "word";

            if (opcodes == null)
            {
                opcodes = new Dictionary<string, Assembly.Instructions>();
                opcodes.Add("=", Assembly.Instructions.SET);
                opcodes.Add("+=", Assembly.Instructions.ADD);
                opcodes.Add("-=", Assembly.Instructions.SUB);
                opcodes.Add("*=", Assembly.Instructions.MUL);
                opcodes.Add("/=", Assembly.Instructions.DIV);

                opcodes.Add("-*=", Assembly.Instructions.MLI);
                opcodes.Add("-/=", Assembly.Instructions.DVI);

                opcodes.Add("%=", Assembly.Instructions.MOD);
                opcodes.Add("-%=", Assembly.Instructions.MDI);
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
            Child(1).GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            Child(1).ResolveTypes(context, enclosingScope);
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
            var r = new Assembly.StatementNode();
            r.AddChild(new Assembly.Annotation(context.GetSourceSpan(this.Span)));
            r.AddChild(Child(1).Emit(context, scope));

            var opcode = Assembly.Instructions.SET;
            if (opcodes.ContainsKey(@operator + ResultType)) opcode = opcodes[@operator + ResultType];
            else opcode = opcodes[@operator];

            r.AddChild((Child(0) as AssignableNode).EmitAssignment(context, scope, rvalueTargetRegister, opcode));
            return r;
        }
    }

    
}
