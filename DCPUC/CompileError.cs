using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public class CompileError : Exception
    {
        public Irony.Parsing.SourceSpan? span = null;

        public CompileError(String msg) : base(msg) { }

        public CompileError(Irony.Parsing.SourceSpan span, String msg) 
            : base(msg) 
        {
            this.span = span;        
        }

        public CompileError(CompilableNode node, String msg)
            : base(msg)
        {
            this.span = node.Span;
        }
    }
}
