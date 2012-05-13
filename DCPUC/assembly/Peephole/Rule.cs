using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC.Assembly.Peephole
{
    public class Rule : AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("matcher", treeNode.ChildNodes[0]);
            AddChild("replacement", treeNode.ChildNodes[1]);
        }

        public bool TryAt(List<Node> assembly, int at)
        {
            var values = new Dictionary<string, Operand>();
            if ((ChildNodes[0] as Matcher).Match(assembly, at, values))
            {
                assembly.RemoveRange(at, ChildNodes[0].ChildNodes.Count);
                assembly.InsertRange(at, (ChildNodes[1] as ReplacementSet).Generate(values));
                return true;
            }
            return false;
        }
    }

    public class RuleSet : AstNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
                AddChild("rule", child);
        }

        public void ProcessAssembly(List<Node> assembly)
        {
            int rulesMatched = 0;
            do
            {
                rulesMatched = 0;
                var place = 0;
                while (place < assembly.Count)
                {
                    foreach (var rule in ChildNodes)
                        if ((rule as Rule).TryAt(assembly, place))
                            rulesMatched += 1;
                    place += 1;
                }
            } while (rulesMatched > 0);
        }
    }
}
