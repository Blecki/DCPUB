using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUC.Assembly.Peephole
{
    [Language("DCPUC OPERAND PEEPHOLE", "0.1", "Peephole optimization instruction replacement definition")]
    public class OperandGrammar : Irony.Parsing.Grammar
    {
        public OperandGrammar()
        {
            var integerLiteral = new NumberLiteral("integer", NumberOptions.IntOnly);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);

            var Register = ToTerm("A") | "B" | "C" | "X" | "Y" | "Z" | "I" | "J" | "PC" | "EX" | "SP" | "PUSH" | "POP" | "PEEK";
            Register.Name = "register";
            var Offset = new NonTerminal("offset");
            var Dereference = new NonTerminal("deref");
            var Operand = new NonTerminal("operand");

            Offset.Rule = (integerLiteral + "+" + Register) | (Register + "+" + integerLiteral);
            Dereference.Rule = ToTerm("[") + (Offset | integerLiteral | Register) + "]";
            Operand.Rule = Dereference | Register | integerLiteral;

            this.Root = Operand;

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation("/", ":", ",", "(", ")", "=>", ";");

        }

    }
}