using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    [Language("DCPUB OPERAND PEEPHOLE", "0.1", "Peephole optimization instruction replacement definition")]
    public class OperandGrammar : Irony.Parsing.Grammar
    {
        public OperandGrammar()
        {
            var integerLiteral = new NumberLiteral("integer", NumberOptions.IntOnly);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);

            //var Register = ToTerm("A") | "B" | "C" | "X" | "Y" | "Z" | "I" | "J" | "PC" | "EX" | "SP" | "PUSH" | "POP" | "PEEK";
            //Register.Name = "register";
            var Offset = new NonTerminal("offset");
            var Dereference = new NonTerminal("deref");
            var Operand = new NonTerminal("operand");//, typeof(OperandAstNode));
            var Label = new NonTerminal("label");

            Offset.Rule = (integerLiteral + "+" + Label) | (Label + "+" + integerLiteral);
            Dereference.Rule = ToTerm("[") + (Offset | integerLiteral | Label) + "]";
            Label.Rule = TerminalFactory.CreateCSharpIdentifier("identifier");
            Operand.Rule = Dereference | integerLiteral | Label;

            this.Root = Operand;

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation("/", ":", ",", "(", ")", "=>", ";");

        }

    }
}