using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class VariableNameNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            var variable = scope.FindVariable(AsString);
            if (variable == null) throw new CompileError("Could not find variable " + AsString);
            if (scope.stackDepth - variable.stackOffset > 0)
            {
                assembly.Add("SET A, SP");
                assembly.Add("SET PUSH, [" + hex(scope.stackDepth - variable.stackOffset) + "+A]");
            }
            else
                assembly.Add("SET PUSH, PEEK");
            scope.stackDepth += 1;
        }
    }

    
}
