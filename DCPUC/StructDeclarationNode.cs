using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class MemberNode : CompilableNode
    {
        public Member member = null;
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            member = new Member();
            member.name = treeNode.ChildNodes[0].FindTokenAndGetText();
            member.typeSpecifier = treeNode.ChildNodes[1].FindTokenAndGetText() ?? "unsigned";
        }

        public override string TreeLabel()
        {
            return "member " + member.name + " " + member.typeSpecifier;
        }

    }

    public class StructDeclarationNode : CompilableNode
    {
        Struct @struct = null;

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            @struct = new Struct();
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
            foreach (var child in ChildNodes)
                @struct.members.Add((child as MemberNode).member);
            enclosingScope.structs.Add(@struct);
        }

        public override CompilableNode FoldConstants(CompileContext context)
        {
            return null;
        }

        public override void Compile(CompileContext context, Scope scope, Register target)
        {
            throw new CompileError("Struct was not removed by Fold pass");
        }

        public override void Emit(CompileContext context, Scope scope)
        {
            throw new CompileError("Struct was not removed by fold pass");
        }
    }
}
