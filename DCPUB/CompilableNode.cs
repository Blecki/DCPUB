using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using DCPUB.Intermediate;

namespace DCPUB
{
    public partial class CompilableNode : AstNode
    {
        public string ResultType = "word";

        public virtual Intermediate.IRNode Emit(CompileContext context, Scope scope, Target target)
        {
            return new Annotation("Emit not implemented on " + this.GetType().Name);
        }

        /// <summary>
        /// Returns an operand to fetch the value of a node. Returns null if the value cannot
        /// be fetched by a single operand. If this does not return null, the emission of 
        /// this nodes op-codes can be skipped.
        /// </summary>
        /// <returns>An operand to fetch the value of the node</returns>
        public virtual Intermediate.Operand GetFetchToken() { return null; }

        public virtual void ResolveTypes(CompileContext context, Scope enclosingScope)
        {
            foreach (CompilableNode child in ChildNodes)
                child.ResolveTypes(context, enclosingScope);
        }

        public CompilableNode Child(int n) { return ChildNodes[n] as CompilableNode; }

        public virtual void GatherSymbols(CompileContext context, Scope enclosingScope) 
        {
            foreach (CompilableNode child in ChildNodes)
                child.GatherSymbols(context, enclosingScope);
        }        
    }
}
