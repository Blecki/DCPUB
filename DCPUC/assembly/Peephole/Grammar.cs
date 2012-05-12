using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUC.Assembly.Peephole
{
    [Language("DCPUC PEEPHOLE", "0.1", "Peephole optimization instruction replacement definition")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", ";", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);

            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            identifier.AstNodeType = typeof(VariableNameNode);

            var replacementToken = new NonTerminal("r token");
            replacementToken.Rule = identifier;
            var replacementInstruction = new NonTerminal("r instruction");
            replacementInstruction.Rule = replacementToken + replacementToken + "," + replacementToken;
            var replacementSet = new NonTerminal("r set", typeof(ReplacementSet));
            replacementSet.Rule = MakePlusRule(replacementSet, ToTerm("/"), replacementInstruction);
            var matchToken = new NonTerminal("token");
            matchToken.Rule = (ToTerm("!")).Q() + identifier;
            var matchInstruction = new NonTerminal("Match instruction");
            matchInstruction.Rule = matchToken + matchToken + "," + matchToken;
            var matchSet = new NonTerminal("match set");
            matchSet.Rule = MakePlusRule(matchSet, ToTerm("/"), matchInstruction);
            var definition = new NonTerminal("definition", typeof(Rule));
            definition.Rule = matchSet + ":" + replacementSet + "=>" + replacementSet;
            var definitionList = new NonTerminal("definition list", typeof(RuleSet));
            definitionList.Rule = MakeStarRule(definitionList, definition);

            this.Root = definitionList;

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation("/", ":", ",");
            
            this.RegisterOperators(1, Associativity.Right, "==", "!=");
            this.RegisterOperators(2, Associativity.Right, "=", "+=", "-=", "*=", "/=", "%=", "^=", "<<=", ">>=", "&=", "|=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/", "%");
            this.RegisterOperators(5, Associativity.Left, "<<", ">>", "&", "|", "^");
            
            this.RegisterOperators(6, Associativity.Left, "{", "}", "[", "]", "<", ">");
        }

    }
}