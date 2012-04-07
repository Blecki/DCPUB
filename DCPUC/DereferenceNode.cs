using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class DereferenceNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            
        }

        public override void Compile(Assembly assembly, Scope scope, Register target)
        {
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope, target);
            assembly.Add("SET", Scope.TempRegister, Scope.GetRegisterLabelSecond((int)target));
            assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "["+Scope.TempRegister+"]");
        }
    }

    
}
