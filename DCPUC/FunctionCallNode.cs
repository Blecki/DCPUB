using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class FunctionCallNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AsString = treeNode.ChildNodes[0].FindTokenAndGetText();
            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter", parameter);
        }

        private static FunctionDeclarationNode findFunction(AstNode node, string name)
        {
            foreach (var child in node.ChildNodes)
                if (child is FunctionDeclarationNode && (child as FunctionDeclarationNode).AsString == name)
                    return child as FunctionDeclarationNode;
            if (node.Parent != null) return findFunction(node.Parent, name);
            return null;
        }

        public override void Compile(List<string> assembly, Scope scope, Register target)
        {
            var func = findFunction(this, AsString);
            if (func == null) throw new CompileError("Can't find function - " + AsString);
            if (func.parameterCount != ChildNodes.Count) throw new CompileError("Incorrect number of arguments - " + AsString);
            foreach (var child in ChildNodes)
                (child as CompilableNode).Compile(assembly, scope, Register.STACK);
            assembly.Add("JSR " + func.label);
            assembly.Add("SET PUSH, A");
        }

        
    }
}
