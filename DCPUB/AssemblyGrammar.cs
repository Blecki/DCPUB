using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    [Language("DCPUB ASM", "0.1", "DASM Grammar")]
    public class AssemblyGrammar : Irony.Parsing.Grammar
    {
        public AssemblyGrammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;
            this.WhitespaceChars = "\t ";

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);

            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            var stringLiteral = new StringLiteral("string", "\"");
            var integerLiteral = new NumberLiteral("integer", NumberOptions.IntOnly);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);

            var operandGrammar = new OperandGrammar();

            var instruction = new NonTerminal("instruction");
            instruction.Rule = identifier + operandGrammar.Root + (Empty | (ToTerm(",") + operandGrammar.Root));

            var dat = new NonTerminal("dat");
            var datElement = new NonTerminal("datelement");
            datElement.Rule = integerLiteral | stringLiteral;
            var datList = new NonTerminal("datlist");
            datList.Rule = MakePlusRule(datList, datElement);
            dat.Rule = ToTerm("DAT") + datList;

            var label = new NonTerminal("label");
            label.Rule = ToTerm(":") + identifier;

            var line = new NonTerminal("line");
            line.Rule = dat | instruction | label | Empty;
            var instructionList = new NonTerminal("inslist", typeof(Ast.Assembly.InstructionListAstNode));
            instructionList.Rule = MakeStarRule(instructionList, NewLine | ToTerm(";"), line);

            this.Root = instructionList;

            this.MarkTransient(line);
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation("/", ":", ",", "(", ")", "=>", ";");

        }

    }
}