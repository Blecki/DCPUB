﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    public class MemberNode : CompilableNode
    {
        public Member member = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            member = new Member();
            member.name = treeNode.ChildNodes[0].FindTokenAndGetText();
            member.typeSpecifier = treeNode.ChildNodes[1].FindTokenAndGetText() ?? "word";
            if (treeNode.ChildNodes[2].FirstChild.ChildNodes.Count > 0)
                AddChild("array size", treeNode.ChildNodes[2].FirstChild.FirstChild);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);
            if (ChildNodes.Count > 0)
            {
                var token = Child(0).GetFetchToken();
                if (!token.IsIntegralConstant()) throw new CompileError(this, "Array sizes must be compile time constants.");
                member.size = token.constant;
                member.isArray = true;
            }
            else
            {
                member.size = 1;
                member.isArray = false;
            }
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            if (ChildNodes.Count > 0)
            {
                if (!Child(0).IsIntegralConstant()) throw new CompileError(this, "Array sizes must be compile time constants.");
                member.size = Child(0).GetConstantValue();
                member.isArray = true;
                //var _struct = context.globalScope.FindType(member.typeSpecifier);
                //if (_struct != null) member.size *= _struct.size;
            }
            else
            {
                member.size = 1;
                member.isArray = false;
            }
            return this;
        }

    }

    public class StructDeclarationNode : CompilableNode
    {
        Struct @struct = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            @struct = new Struct();
            @struct.size = 0;
            @struct.name = treeNode.ChildNodes[1].FindTokenAndGetText();
            foreach (var member in treeNode.ChildNodes[2].ChildNodes)
                AddChild("member", member);
            @struct.Node = this;
        }

        public override string TreeLabel()
        {
            return "struct " + @struct.name;
        }

        public override void GatherSymbols(CompileContext context, Scope enclosingScope)
        {
            base.GatherSymbols(context, enclosingScope);

            if (enclosingScope.type != ScopeType.Global)
                throw new CompileError(this, "Structs must be declared at global scope.");
            foreach (var child in ChildNodes)
                @struct.members.Add((child as MemberNode).member);
            enclosingScope.structs.Add(@struct);
        }

        public override void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            base.ResolveTypes(context, enclosingScope);

            int offset = 0;
            foreach (var member in @struct.members)
            {
                member.offset = offset;
                offset += member.size;
            }
            @struct.size = offset;
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            base.FoldConstants(context);
            int offset = 0;
            foreach (var member in @struct.members)
            {
                member.offset = offset;
                offset += member.size;
            }
            @struct.size = offset;
            return null;
        }

        public override Assembly.Node Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Struct was not removed by fold pass");
        }

        public override Assembly.Node Emit2(CompileContext context, Scope scope, Target target)
        {
            return new Assembly.Annotation("Declaration of struct " + @struct.name);
        }
    }
}
