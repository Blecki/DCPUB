using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUB
{
    [Language("DCPUB", "0.3", "C-like language for Notch's DCPU-16")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            var blockComment = new CommentTerminal("comment", "/*", "*/");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(blockComment);

            var integerLiteral = new NumberLiteral("integer",
                NumberOptions.IntOnly | NumberOptions.AllowSign | NumberOptions.AllowLetterAfter);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);
            integerLiteral.AddPrefix("0b", NumberOptions.Binary);
            integerLiteral.AddSuffix("u", TypeCode.UInt16);
            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            identifier.AstNodeType = typeof(Ast.VariableNameNode);
            

            var stringLiteral = new StringLiteral("string", "\"");
            stringLiteral.AstNodeType = typeof(Ast.StringLiteralNode);
            var characterLiteral = new StringLiteral("char", "'", StringOptions.IsChar);
           

            var inlineASM = new NonTerminal("inline", typeof(Ast.InlineASMNode));

            var numberLiteral = new NonTerminal("Number", typeof(Ast.NumberLiteralNode));
            var expression = new NonTerminal("Expression");
            var parenExpression = new NonTerminal("Paren Expression");
            var binaryOperation = new NonTerminal("Binary Operation", typeof(Ast.BinaryOperationNode));
            var unaryNot = new NonTerminal("Unary Not", typeof(Ast.NotOperatorNode));
            var unaryNegate = new NonTerminal("Unary Negate", typeof(Ast.NegateOperatorNode));
            //var comparison = new NonTerminal("Comparison", typeof(Ast.ComparisonNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^" | "<<" | ">>" | "-*" | "-/" | "-%"
                | "==" | "!=" | ">" | "->" | "<" | "-<" | "&&" | "||";
            var comparisonOperator = ToTerm("==") | "!=" | ">" | "<" | "->" | "-<";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(Ast.VariableDeclarationNode));
            var arrayInitialization = new NonTerminal("Array Initialization", typeof(Ast.ArrayInitializationNode));
            var dereference = new NonTerminal("Dereference", typeof(Ast.DereferenceNode));
            var statement = new NonTerminal("Statement");
            var statementList = new NonTerminal("Statement List", typeof(Ast.BlockNode));
            var assignment = new NonTerminal("Assignment", typeof(Ast.AssignmentNode));
            var ifStatement = new NonTerminal("If", typeof(Ast.IfStatementNode));
            var whileStatement = new NonTerminal("While", typeof(Ast.WhileStatementNode));
            var block = new NonTerminal("Block");
            var ifElseStatement = new NonTerminal("IfElse", typeof(Ast.IfStatementNode));
            var parameterList = new NonTerminal("Parameter List");
            var functionDeclaration = new NonTerminal("Function Declaration", typeof(Ast.FunctionDeclarationNode));
            var parameterDeclaration = new NonTerminal("Parameter Declaration");
            var parameterListDeclaration = new NonTerminal("Parameter Declaration List");
            var returnStatement = new NonTerminal("Return", typeof(Ast.ReturnStatementNode));
            var functionCall = new NonTerminal("Function Call", typeof(Ast.FunctionCallNode));
            var structDefinition = new NonTerminal("Struct", typeof(Ast.StructDeclarationNode));
            var memberDeclaration = new NonTerminal("Member", typeof(Ast.MemberNode));
            var memberList = new NonTerminal("Member List");
            var registerBinding = new NonTerminal("Register Binding", typeof(Ast.RegisterBindingNode));
            var registerBindingList = new NonTerminal("Register Binding List");
            var addressOf = new NonTerminal("Address Of", typeof(Ast.AddressOfNode));
            var memberAccess = new NonTerminal("Member Access", typeof(Ast.MemberAccessNode));
            var @sizeof = new NonTerminal("sizeof", typeof(Ast.SizeofNode));
            var offsetof = new NonTerminal("offsetof", typeof(Ast.OffsetOfNode));
            var indexOperator = new NonTerminal("index", typeof(Ast.IndexOperatorNode));
            var dataList = new NonTerminal("Array Data");
            var label = new NonTerminal("Label", typeof(Ast.LabelNode));
            var @goto = new NonTerminal("Goto", typeof(Ast.GotoNode));
            var cast = new NonTerminal("Cast", typeof(Ast.CastNode));
            var @break = new NonTerminal("Break", typeof(Ast.BreakNode));
            var nullStatement = new NonTerminal("NullStatement", typeof(Ast.NullStatementNode));

            numberLiteral.Rule = integerLiteral | characterLiteral;
            expression.Rule = cast | numberLiteral | parenExpression | identifier
                | dereference | functionCall | addressOf | memberAccess | @sizeof | indexOperator
                | unaryNot | unaryNegate | binaryOperation | stringLiteral | offsetof;

            assignment.Rule = (identifier | dereference | memberAccess | indexOperator) 
                + (ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "^=" | "<<=" | ">>=" | "&=" | "|=" | "-*=" | "-/=" | "-%=" )
                + expression;
            binaryOperation.Rule = expression + @operator + expression;
            unaryNot.Rule = ToTerm("!") + expression;
            unaryNegate.Rule = ToTerm("-") + expression;
            //comparison.Rule = expression + comparisonOperator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule =
                (ToTerm("local") | "static" | "external") + identifier + (ToTerm(":") + identifier).Q()
                + (ToTerm("[") + expression + "]").Q()
                + (ToTerm("=") + (expression | arrayInitialization)).Q();
            dereference.Rule = ToTerm("*") + expression;
            statement.Rule = inlineASM | (variableDeclaration + ";")
                | (assignment + ";") | ifStatement | ifElseStatement | whileStatement | block
                | functionDeclaration | structDefinition | (functionCall + ";")
                | (returnStatement + ";") | label | @goto | (@break + ";") | nullStatement;
            nullStatement.Rule = ToTerm(";");
            block.Rule = ToTerm("{") + statementList + "}";
            statementList.Rule = MakeStarRule(statementList, statement);
            addressOf.Rule = ToTerm("&") + identifier;
            memberAccess.Rule = expression + "." + identifier;
            @sizeof.Rule = ToTerm("sizeof") + identifier;
            offsetof.Rule = ToTerm("offsetof") + identifier + "in" + identifier;
            indexOperator.Rule = expression + "[" + expression + "]";
            label.Rule = ToTerm(":") + identifier;
            @goto.Rule = ToTerm("goto") + identifier + ";";
            cast.Rule = expression + ":" + identifier;
            @break.Rule = ToTerm("break");

            registerBinding.Rule = identifier + (ToTerm("=") + expression).Q();
            registerBindingList.Rule = MakeStarRule(registerBindingList, (ToTerm(";") | ToTerm(",")), registerBinding);
            inlineASM.Rule = ToTerm("asm") + (ToTerm("(") + registerBindingList + ")").Q() + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            ifStatement.Rule = ToTerm("if") + "(" + expression + ")" + statement;
            ifElseStatement.Rule = ifStatement + this.PreferShiftHere() + "else" + statement;
            whileStatement.Rule = ToTerm("while") + "(" + expression + ")" + statement;
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = expression + "(" + parameterList + ")";
            parameterDeclaration.Rule = identifier + (ToTerm(":") + identifier).Q();
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);
            functionDeclaration.Rule = ToTerm("function") + identifier + "(" + parameterListDeclaration + ")" 
                + (ToTerm(":") + identifier).Q()
                + block;
            returnStatement.Rule = ToTerm("return") + expression.Q();
            arrayInitialization.Rule = ToTerm("{") + dataList + "}";
            dataList.Rule = MakePlusRule(dataList, ToTerm(","), expression);
            memberDeclaration.Rule = identifier + (ToTerm(":") + identifier).Q() + (ToTerm("[") + expression + "]").Q() + ";";
            memberList.Rule = MakeStarRule(memberList, memberDeclaration);
            structDefinition.Rule = ToTerm("struct") + identifier + "{" + memberList + "}";


            this.Root = statementList;

            this.RegisterBracePair("[", "]");
            this.RegisterBracePair("{", "}");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":", "?", "!");
            this.MarkTransient(expression, parenExpression, statement, block);//, parameterList);

            this.RegisterOperators(1, Associativity.Left, "&&", "||");
            this.RegisterOperators(2, Associativity.Left, "==", "!=", ">", "<", "->", "-<");
            this.RegisterOperators(3, Associativity.Right, "=", "+=", "-=", "*=", "/=", "%=", "^=", "<<=", ">>=", "&=", "|=", "-*=", "-/=", "-%=");
            this.RegisterOperators(4, Associativity.Left, "+", "-");
            this.RegisterOperators(5, Associativity.Left, "*", "/", "%");
            this.RegisterOperators(5, Associativity.Left, "-*", "-/", "-%");
            this.RegisterOperators(6, Associativity.Left, "<<", ">>", "&", "|", "^", "!");
            this.RegisterOperators(7, Associativity.Left, ":");
            
            this.RegisterOperators(8, Associativity.Left, "{", "}", "[", "]");
        }

    }
}