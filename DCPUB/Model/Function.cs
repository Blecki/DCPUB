using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUB.Model
{
    public class Function
    {
        public String name;
        public Ast.FunctionDeclarationNode Node;
        public Scope localScope;
        public Intermediate.Label label;
        public string LabelName;
        public int parameterCount = 0;
        public String returnType = "void";
        public List<Label> labels = new List<Label>();

        public bool reached = false;
        public List<Function> Calls = new List<Function>();
        public List<Function> SubordinateFunctions = new List<Function>();

        public void MarkReachableFunctions()
        {
            if (reached) return;
            reached = true;
            foreach (var child in Calls) child.MarkReachableFunctions();
        }
    }
}
