using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly.Peephole
{
    public class Peepholes
    {
        public static RuleSet root = null;
        public static Irony.Parsing.Parser operandParser = new Irony.Parsing.Parser(new OperandGrammar());

        public static void InitializePeepholes()
        {
            if (root != null) return;
            var Parser = new Irony.Parsing.Parser(new Grammar());
            var defs = System.IO.File.ReadAllText("Assembly/Peephole/peepholedef.txt");
            var _root = Parser.Parse(defs);
            if (_root.HasErrors()) root = new RuleSet();
            else root = _root.Root.AstNode as RuleSet; 
        }
    }
}
