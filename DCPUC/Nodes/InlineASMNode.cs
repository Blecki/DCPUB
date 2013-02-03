﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class RegisterBindingNode : CompilableNode
    {
        public string targetRegisterName = "";
        public bool preserveTarget = false;
        public Register targetRegister;
        public Scope rememberScope = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            targetRegisterName = treeNode.ChildNodes[0].FindTokenAndGetText();
            AddChild("expression", treeNode.ChildNodes[2]);
        }

        public override string TreeLabel()
        {
            return "Bind to register " + targetRegisterName + (preserveTarget ? " PRESERVE" : "");
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            if (parentState.registers[(int)targetRegister] == RegisterState.Used) preserveTarget = true;
            parentState.UseRegister(targetRegister);

            Child(0).AssignRegisters(context, parentState, targetRegister);
        }

        public override void  ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            targetRegister = (Register)Enum.Parse(typeof(Register), this.targetRegisterName);
            rememberScope = enclosingScope;
            Child(0).ResolveTypes(context, enclosingScope);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            r.AddChild(new Assembly.Annotation("Inline assembly"));
            if (preserveTarget)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand("PUSH"), 
                    Operand(Scope.GetRegisterLabelSecond((int)targetRegister)));
            }
            r.AddChild(Child(0).Emit(context, scope));
            return r;
        }

        public Assembly.Node Restore(CompileContext context, Scope scope)
        {
            var r = new Assembly.Node();
            if (preserveTarget)
            {
                r.AddInstruction(Assembly.Instructions.SET, Operand(Scope.GetRegisterLabelSecond((int)targetRegister)), 
                    Operand("POP"));
            }
            return r;
        }
    }

    public class InlineASMNode : CompilableNode
    {
        public string rawAssembly = "";
        public Assembly.Node parsedNode;
        public static Irony.Parsing.Parser asmParser = new Irony.Parsing.Parser(new Assembly.AssemblyGrammar());

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes[1].FirstChild.ChildNodes.Count > 0)
                foreach (var child in treeNode.ChildNodes[1].FirstChild.FirstChild.ChildNodes)
                    AddChild("bound register", child);
            rawAssembly = treeNode.ChildNodes[2].FindTokenAndGetText();

            var parsed = asmParser.Parse(rawAssembly);
            if (parsed.HasErrors()) throw new CompileError("Error parsing inline ASM: " + parsed.ParserMessages[0].Message);
            parsedNode = (parsed.Root.AstNode as Assembly.InstructionListAstNode).resultNode;
        }

        public override void AssignRegisters(CompileContext context, RegisterBank parentState, Register target)
        {
            for (var i = 0; i < ChildNodes.Count; ++i)
                Child(i).AssignRegisters(context, parentState, Register.DISCARD);
            for (var i = ChildNodes.Count - 1; i >= 0; --i)
                if (!(Child(i) as RegisterBindingNode).preserveTarget) 
                    parentState.FreeRegister((Child(i) as RegisterBindingNode).targetRegister);
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            var r = new Assembly.StatementNode();

            for (var i = 0; i < ChildNodes.Count; ++i)
                r.AddChild(Child(i).Emit(context, scope));

            r.AddChild(parsedNode);

            for (var i = ChildNodes.Count - 1; i >= 0; --i)
                r.AddChild((Child(i) as RegisterBindingNode).Restore(context, scope));

            return r;
        }
    }

    
}