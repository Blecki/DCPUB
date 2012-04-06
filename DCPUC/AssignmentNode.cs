using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class AssignmentNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("LValue", treeNode.ChildNodes[0].ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);
        }

        public override void Compile(List<string> assembly, Scope scope)
        {
            if (ChildNodes[0] is VariableNameNode)
            {
                var variable = scope.FindVariable(ChildNodes[0].AsString);
                if (variable == null) throw new CompileError("Could not find variable " + ChildNodes[0].AsString);

                (ChildNodes[1] as CompilableNode).Compile(assembly, scope);
                assembly.Add("SET A, SP");
                assembly.Add("SET [" + hex(scope.stackDepth - variable.stackOffset) + "+A], POP");
                scope.stackDepth -= 1;
            }
            else if (ChildNodes[0] is DereferenceNode)
            {
                (ChildNodes[1] as CompilableNode).Compile(assembly, scope);
                (ChildNodes[0].ChildNodes[0] as CompilableNode).Compile(assembly, scope);
                assembly.Add("SET A, POP");
                assembly.Add("SET [A], POP");
                scope.stackDepth -= 2;
            }
        }
    }

    
}
