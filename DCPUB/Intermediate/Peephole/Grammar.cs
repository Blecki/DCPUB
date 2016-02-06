using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUB.Intermediate.Peephole
{
    [Language("DCPUB PEEPHOLE", "0.1", "Peephole optimization instruction replacement definition")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
            var blockComment = new CommentTerminal("comment", "/*", "*/");
            NonGrammarTerminals.Add(blockComment);

            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            var stringLiteral = new StringLiteral("string", "\"");

            var matchOr = new NonTerminal("or", typeof(OperandOr));
            var matchOperand = new NonTerminal("token");
            var matchNot = new NonTerminal("not", typeof(OperandNot));
            var matchAnd = new NonTerminal("and", typeof(OperandAnd));
            var matchParen = new NonTerminal("paren");
            var matchValue = new NonTerminal("value", typeof(OperandMatchValue));
            var matchRaw = new NonTerminal("raw", typeof(OperandMatchRaw));
           
            matchOr.Rule = matchOperand + "|" + matchOperand;
            matchNot.Rule = ToTerm("!") + matchOperand;
            matchParen.Rule = ToTerm("(") + matchOperand + ")";
            matchValue.Rule = identifier;
            matchRaw.Rule = stringLiteral;
            matchOperand.Rule = matchRaw | matchValue | matchOr | matchNot | matchParen;

            var matchInsRaw = new NonTerminal("minsraw", typeof(InstructionMatchRaw));
            var matchInsOr = new NonTerminal("minsor", typeof(InstructionMatchOr));
            var matchInsNot = new NonTerminal("minsnot", typeof(InstructionMatcherNot));
            var matchInstruction = new NonTerminal("mins");
            var matchInsParen = new NonTerminal("minsparen");

            matchInsRaw.Rule = identifier;
            matchInsOr.Rule = matchInstruction + "|" + matchInstruction;
            matchInsNot.Rule = ToTerm("!") + matchInstruction;
            matchInsParen.Rule = ToTerm("(") + matchInstruction + ")";
            matchInstruction.Rule = matchInsRaw | matchInsOr | matchInsNot | matchInsParen;

            var matchWholeInstruction = new NonTerminal("Match instruction");
            //var matchWIPlus = new NonTerminal("plus");
            //var matchWIStar = new NonTerminal("star");
            var matchWIParen = new NonTerminal("insparen");
            var matchWIValue = new NonTerminal("insident", typeof(WholeInstructionMatchRaw));
            var matchWIOr = new NonTerminal("insor", typeof(WholeInstructionMatchOr));
            var matchWINot = new NonTerminal("insnot", typeof(WholeInstructionMatchNot));
            //var matchWISkip = new NonTerminal("skip");

            //matchWIPlus.Rule = matchWholeInstruction + "+";
            //matchWIStar.Rule = matchWholeInstruction + "*";
            matchWIParen.Rule = ToTerm("(") + matchWholeInstruction + ")";
            matchWIValue.Rule = matchInstruction + matchOperand + "," + matchOperand;
            matchWIOr.Rule = matchWholeInstruction + "|" + matchWholeInstruction;
            matchWINot.Rule = ToTerm("!") + matchWholeInstruction;
            //matchWISkip.Rule = ToTerm("$");
            matchWholeInstruction.Rule = /*matchWISkip |*/ matchWIValue | /*matchWIPlus | matchWIStar |*/ matchWIParen
                | matchWIOr | matchWINot;

            var matchSet = new NonTerminal("match set", typeof(Matcher));
            matchSet.Rule = MakePlusRule(matchSet, ToTerm("/"), matchWholeInstruction);
            
            var replacementOperand = new NonTerminal("roperand");
            var replacementInstruction = new NonTerminal("rins", typeof(ReplacementInstruction));
            var replacementDereference = new NonTerminal("rderef", typeof(ReplacementDereference));
            var replacementRaw = new NonTerminal("rraw", typeof(ReplacementRaw));
            var replacementValue = new NonTerminal("rvalue", typeof(ReplacementValue));

            replacementValue.Rule = identifier;
            replacementRaw.Rule = stringLiteral;
            replacementDereference.Rule = ToTerm("[") + (replacementRaw | replacementValue) + "]";
            replacementOperand.Rule = replacementValue | replacementRaw | replacementDereference;

            replacementInstruction.Rule = identifier + replacementOperand + "," + replacementOperand;
            
            var replacementSet = new NonTerminal("r set", typeof(ReplacementSet));
            replacementSet.Rule = MakeStarRule(replacementSet, ToTerm("/"), replacementInstruction);

            var rule = new NonTerminal("rule", typeof(Rule));
            rule.Rule = matchSet + "=>" + replacementSet + ";";
            var ruleList = new NonTerminal("rule list", typeof(RuleSet));
            ruleList.Rule = MakeStarRule(ruleList, rule);

            this.Root = ruleList;

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation("/", ":", ",", "(", ")", "=>", ";");
            this.MarkTransient(
                matchParen, 
                matchOperand,
                matchInsParen,
                matchInstruction, 
                matchWIParen, 
                matchWholeInstruction,
                replacementOperand);

            this.RegisterOperators(1, Associativity.Right, "=>");
            this.RegisterOperators(4, Associativity.Left, "/");
            this.RegisterOperators(5, Associativity.Left, "&", "|");
            
            this.RegisterOperators(6, Associativity.Left, "{", "}", "[", "]", "<", ">");
        }

    }
}