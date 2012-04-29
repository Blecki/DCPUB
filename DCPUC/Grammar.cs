﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    [Language("DCPUC", "0.1", "C-like language for Notch's DCPU-16")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);

            var integerLiteral = new NumberLiteral("integer",
                NumberOptions.IntOnly | NumberOptions.AllowSign | NumberOptions.AllowLetterAfter);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);
            integerLiteral.AddSuffix("u", TypeCode.UInt16);
            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            identifier.AstNodeType = typeof(VariableNameNode);
            

            var stringLiteral = new StringLiteral("string", "\"");
            var characterLiteral = new StringLiteral("char", "'", StringOptions.IsChar);
           

            var inlineASM = new NonTerminal("inline", typeof(InlineASMNode));

            var numberLiteral = new NonTerminal("Number", typeof(NumberLiteralNode));
            var blockLiteral = new NonTerminal("BlockLiteral", typeof(BlockLiteralNode));
            var dataLiteral = new NonTerminal("Data");
            var dataLiteralChain = new NonTerminal("DataChain", typeof(DataLiteralNode));
            var expression = new NonTerminal("Expression");
            var parenExpression = new NonTerminal("Paren Expression");
            var binaryOperation = new NonTerminal("Binary Operation", typeof(BinaryOperationNode));
            var comparison = new NonTerminal("Comparison", typeof(ComparisonNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^" | "<<" | ">>";
            var comparisonOperator = ToTerm("==") | "!=" | ">" | "<";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(VariableDeclarationNode));
            var dereference = new NonTerminal("Dereference", typeof(DereferenceNode));
            var statement = new NonTerminal("Statement");
            var statementList = new NonTerminal("Statement List", typeof(BlockNode));
            var assignment = new NonTerminal("Assignment", typeof(AssignmentNode));
            var ifStatement = new NonTerminal("If", typeof(IfStatementNode));
            var whileStatement = new NonTerminal("While", typeof(WhileStatementNode));
            var block = new NonTerminal("Block");
            var ifElseStatement = new NonTerminal("IfElse", typeof(IfStatementNode));
            var parameterList = new NonTerminal("Parameter List");
            var functionDeclaration = new NonTerminal("Function Declaration", typeof(FunctionDeclarationNode));
            var parameterDeclaration = new NonTerminal("Parameter Declaration");
            var parameterListDeclaration = new NonTerminal("Parameter Declaration List");
            var returnStatement = new NonTerminal("Return", typeof(ReturnStatementNode));
            var functionCall = new NonTerminal("Function Call", typeof(FunctionCallNode));
            var structDefinition = new NonTerminal("Struct", typeof(StructDeclarationNode));
            var memberDeclaration = new NonTerminal("Member", typeof(MemberNode));
            var memberList = new NonTerminal("Member List");
            var registerBinding = new NonTerminal("Register Binding", typeof(RegisterBindingNode));
            var registerBindingList = new NonTerminal("Register Binding List");
            var addressOf = new NonTerminal("Address Of", typeof(AddressOfNode));

            numberLiteral.Rule = integerLiteral | characterLiteral;
            blockLiteral.Rule = ToTerm("[") + integerLiteral + "]";
            dataLiteral.Rule = MakePlusRule(dataLiteral, (numberLiteral | stringLiteral | blockLiteral | characterLiteral));
            dataLiteralChain.Rule = ToTerm("&") + dataLiteral;
            expression.Rule = numberLiteral | characterLiteral | binaryOperation | parenExpression | identifier
                | dereference | functionCall | dataLiteralChain | addressOf;
            assignment.Rule = (identifier | dereference) + (ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "^=" | "<<=" | ">>=" | "&=" | "|=" ) + expression;
            binaryOperation.Rule = expression + @operator + expression;
            comparison.Rule = expression + comparisonOperator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule =
                (ToTerm("var") | "static" | "const") + identifier + (ToTerm(":") + identifier).Q()                
                + "=" + (expression | dataLiteralChain | blockLiteral);
            dereference.Rule = ToTerm("*") + expression;
            statement.Rule = inlineASM | (variableDeclaration + ";")
                | (assignment + ";") | ifStatement | ifElseStatement | whileStatement | block 
                | functionDeclaration | structDefinition | (functionCall + ";")
                | (returnStatement + ";");
            block.Rule = ToTerm("{") + statementList + "}";
            statementList.Rule = MakeStarRule(statementList, statement);
            addressOf.Rule = ToTerm("&") + identifier;

            registerBinding.Rule = /*((ToTerm("?") + integerLiteral) | */identifier/*)*/ + "=" + expression;
            registerBindingList.Rule = MakePlusRule(registerBindingList, ToTerm(";"), registerBinding);
            inlineASM.Rule = ToTerm("asm") + (ToTerm("(") + registerBindingList + ")").Q() + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            ifStatement.Rule = ToTerm("if") + "(" + (expression | comparison) + ")" + statement;
            ifElseStatement.Rule = ifStatement + this.PreferShiftHere() + "else" + statement;
            whileStatement.Rule = ToTerm("while") + "(" + (expression | comparison) + ")" + statement;
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = identifier + "(" + parameterList + ")";
            parameterDeclaration.Rule = identifier + (ToTerm(":") + identifier).Q();
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);
            functionDeclaration.Rule = ToTerm("function") + identifier + "(" + parameterListDeclaration + ")" 
                + (ToTerm(":") + identifier).Q()
                + block;
            returnStatement.Rule = ToTerm("return") + expression;
            memberDeclaration.Rule = identifier + (ToTerm(":") + identifier).Q() + ";";
            memberList.Rule = MakeStarRule(memberList, memberDeclaration);
            structDefinition.Rule = ToTerm("struct") + identifier + "{" + memberList + "}";


            this.Root = statementList;

            this.RegisterBracePair("[", "]");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");
            this.MarkTransient(expression, parenExpression, statement, block);//, parameterList);

            this.RegisterOperators(1, Associativity.Right, "==", "!=");
            this.RegisterOperators(2, Associativity.Right, "=", "+=", "-=", "*=", "/=", "%=", "^=", "<<=", ">>=", "&=", "|=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/", "%");
            this.RegisterOperators(5, Associativity.Left, "<<", ">>", "&", "|", "^");
            
            this.RegisterOperators(6, Associativity.Left, "[", "]", "<", ">");
        }

    }
}