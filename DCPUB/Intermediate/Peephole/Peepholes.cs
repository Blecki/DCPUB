using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate.Peephole
{
    public class Peepholes
    {
        public RuleSet root = null;
        public static Irony.Parsing.Parser operandParser = new Irony.Parsing.Parser(new OperandGrammar());

        public Peepholes(string defFile)
        {
            var Parser = new Irony.Parsing.Parser(new Grammar());
            var defs = System.IO.File.ReadAllText(defFile);
            var _root = Parser.Parse(defs);
            if (_root.HasErrors()) root = new RuleSet();
            else root = _root.Root.AstNode as RuleSet; 
        }

        public void ProcessAssembly(List<IRNode> assembly)
        {
            root.ProcessAssembly(assembly);
        }
    }
}
