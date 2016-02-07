using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public class AssignmentNode : CompilableNode
    {
        private static Dictionary<String, Instructions> opcodes = null;

        public Model.Register rvalueTargetRegister = Model.Register.STACK;
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
                opcodes = new Dictionary<string, Instructions>();
                opcodes.Add("=", Instructions.SET);
                opcodes.Add("+=", Instructions.ADD);
                opcodes.Add("-=", Instructions.SUB);
                opcodes.Add("*=", Instructions.MUL);
                opcodes.Add("/=", Instructions.DIV);

                opcodes.Add("-*=", Instructions.MLI);
                opcodes.Add("-/=", Instructions.DVI);

                opcodes.Add("%=", Instructions.MOD);
                opcodes.Add("-%=", Instructions.MDI);
                opcodes.Add("<<=", Instructions.SHL);
                opcodes.Add(">>=", Instructions.SHR);
                opcodes.Add("&=", Instructions.AND);
                opcodes.Add("|=", Instructions.BOR);
                opcodes.Add("^=", Instructions.XOR);
            }
        }

        public override void GatherSymbols(CompileContext context, Model.Scope enclosingScope)
        {
            Child(0).GatherSymbols(context, enclosingScope);
            Child(1).GatherSymbols(context, enclosingScope);
        }

        public override void ResolveTypes(CompileContext context, Model.Scope enclosingScope)
        {
            Child(0).ResolveTypes(context, enclosingScope);
            Child(1).ResolveTypes(context, enclosingScope);
            ResultType = Child(0).ResultType;
        }

        public override Intermediate.IRNode Emit(CompileContext context, Model.Scope scope, Target target)
        {
            var r = new StatementNode();
            r.AddChild(new Annotation(context.GetSourceSpan(this.Span)));

            var fetch_token = Child(1).GetFetchToken();
            if (fetch_token == null)
            {
                var rTarget = Target.Register(context.AllocateRegister());
                r.AddChild(Child(1).Emit(context, scope, rTarget));
                fetch_token = rTarget.GetOperand(TargetUsage.Pop);
            }

            r.AddChild((Child(0) as AssignableNode).EmitAssignment(context, scope, fetch_token, opcodes[@operator]));
            return r;
        }
    }

    
}
