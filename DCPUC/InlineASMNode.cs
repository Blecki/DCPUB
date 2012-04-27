using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class RegisterBindingNode : CompilableNode
    {
        public string target = "";
        public bool preserveTarget = false;
        public Register targetRegister;
        public Variable variableSharingRegister = null;
        public Scope rememberScope = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            target = treeNode.ChildNodes[0].FindTokenAndGetText();
            AddChild("expression", treeNode.ChildNodes[2]);
        }

        public override string TreeLabel()
        {
            return "Bind to register " + target + (preserveTarget ? " PRESERVE" : "");
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (parentState.registers[(int)targetRegister] == RegisterState.Used) preserveTarget = true;
            parentState.UseRegister(targetRegister);

            var scope = rememberScope;
            while (variableSharingRegister == null && scope != null)
            {
                foreach (var v in scope.variables)
                    if (v.location == targetRegister)
                        variableSharingRegister = v;
                if (variableSharingRegister == null) scope = scope.parent;
            }

            Child(0).AssignRegisters(context, parentState, targetRegister);
        }

        public override void  ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            targetRegister = (Register)Enum.Parse(typeof(Register), this.target);
            rememberScope = enclosingScope;
            Child(0).ResolveTypes(context, enclosingScope);
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            if (preserveTarget)
            {
                context.Add("SET", "PUSH", Scope.GetRegisterLabelSecond((int)targetRegister));
                scope.stackDepth += 1;
                //If that register was used by a variable, we might have to move the variable.
                if (variableSharingRegister != null && Scope.IsRegister(variableSharingRegister.location))
                {
                    variableSharingRegister.location = Register.STACK;
                    variableSharingRegister.stackOffset = scope.stackDepth;
                }
            }
            Child(0).Emit(context, scope);
        }

        public void Restore(CompileContext context, Scope scope)
        {
            if (preserveTarget)
            {
                context.Add("SET", Scope.GetRegisterLabelSecond((int)targetRegister), "POP");
                scope.stackDepth -= 1;
                if (variableSharingRegister != null && Scope.IsRegister(variableSharingRegister.location))
                    variableSharingRegister.location = targetRegister;
            }
        }
    }

    public class InlineASMNode : CompilableNode
    {
        public string rawAssembly = "";

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                foreach (var child in treeNode.ChildNodes[1].FirstChild.FirstChild.ChildNodes)
                    AddChild("bound register", child);
            rawAssembly = treeNode.ChildNodes[2].FindTokenAndGetText();
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            for (var i = 0; i < ChildNodes.Count; ++i)
                Child(i).AssignRegisters(context, parentState, Register.DISCARD);
            for (var i = ChildNodes.Count - 1; i >= 0; --i)
                if (!(Child(i) as RegisterBindingNode).preserveTarget) 
                    parentState.FreeRegister((Child(i) as RegisterBindingNode).targetRegister);
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            var lines = rawAssembly.Split(new String[2]{"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < ChildNodes.Count; ++i)
                Child(i).Emit(context, scope);

            context.Barrier();
            foreach (var str in lines)
                context.Add(str + " ;", "", "");
            context.Barrier();

            for (var i = ChildNodes.Count - 1; i >= 0; --i)
                (Child(i) as RegisterBindingNode).Restore(context, scope);
        }
    }

    
}
