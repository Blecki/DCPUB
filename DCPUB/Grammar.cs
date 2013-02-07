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
            identifier.AstNodeType = typeof(VariableNameNode);
            

            var stringLiteral = new StringLiteral("string", "\"");
            stringLiteral.AstNodeType = typeof(StringLiteralNode);
            var characterLiteral = new StringLiteral("char", "'", StringOptions.IsChar);
           

            var inlineASM = new NonTerminal("inline", typeof(InlineASMNode));

            var numberLiteral = new NonTerminal("Number", typeof(NumberLiteralNode));
            var expression = new NonTerminal("Expression");
            var parenExpression = new NonTerminal("Paren Expression");
            var binaryOperation = new NonTerminal("Binary Operation", typeof(BinaryOperationNode));
            var unaryNot = new NonTerminal("Unary Not", typeof(NotOperatorNode));
            var unaryNegate = new NonTerminal("Unary Negate", typeof(NegateOperatorNode));
            var comparison = new NonTerminal("Comparison", typeof(ComparisonNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^" | "<<" | ">>" | "-*" | "-/" | "-%"
                | "==" | "!=" | ">" | "->" | "<" | "-<";
            var comparisonOperator = ToTerm("==") | "!=" | ">" | "<" | "->" | "-<";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(VariableDeclarationNode));
            var arrayInitialization = new NonTerminal("Array Initialization", typeof(ArrayInitializationNode));
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
            var memberAccess = new NonTerminal("Member Access", typeof(MemberAccessNode));
            var @sizeof = new NonTerminal("sizeof", typeof(SizeofNode));
            var indexOperator = new NonTerminal("index", typeof(IndexOperatorNode));
            var dataList = new NonTerminal("Array Data");
            var label = new NonTerminal("Label", typeof(LabelNode));
            var @goto = new NonTerminal("Goto", typeof(GotoNode));
            var cast = new NonTerminal("Cast", typeof(CastNode));
            var @break = new NonTerminal("Break", typeof(BreakNode));
            var nullStatement = new NonTerminal("NullStatement", typeof(NullStatementNode));

            numberLiteral.Rule = integerLiteral | characterLiteral;
            expression.Rule = cast | numberLiteral | parenExpression | identifier
                | dereference | functionCall | addressOf | memberAccess | @sizeof | indexOperator
                | unaryNot | unaryNegate | binaryOperation | stringLiteral;

            assignment.Rule = (identifier | dereference | memberAccess | indexOperator) 
                + (ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "^=" | "<<=" | ">>=" | "&=" | "|=" | "-*=" | "-/=" | "-%=" )
                + expression;
            binaryOperation.Rule = expression + @operator + expression;
            unaryNot.Rule = ToTerm("!") + expression;
            unaryNegate.Rule = ToTerm("-") + expression;
            comparison.Rule = expression + comparisonOperator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule =
                (ToTerm("local") | "static" | "constant" | "external") + identifier + (ToTerm(":") + identifier).Q()
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
            @sizeof.Rule = ToTerm("sizeof") + "(" + identifier + ")";
            indexOperator.Rule = expression + "[" + expression + "]";
            label.Rule = ToTerm(":") + identifier;
            @goto.Rule = ToTerm("goto") + identifier + ";";
            cast.Rule = expression + ":" + identifier;
            @break.Rule = ToTerm("break");

            registerBinding.Rule = /*((ToTerm("?") + integerLiteral) | */identifier/*)*/ + "=" + expression;
            registerBindingList.Rule = MakePlusRule(registerBindingList, ToTerm(";"), registerBinding);
            inlineASM.Rule = ToTerm("asm") + (ToTerm("(") + registerBindingList + ")").Q() + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            ifStatement.Rule = ToTerm("if") + "(" + (comparison | expression) + ")" + statement;
            ifElseStatement.Rule = ifStatement + this.PreferShiftHere() + "else" + statement;
            whileStatement.Rule = ToTerm("while") + "(" + (comparison | expression) + ")" + statement;
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = expression + "(" + parameterList + ")";
            parameterDeclaration.Rule = identifier + (ToTerm(":") + identifier).Q();
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);
            functionDeclaration.Rule = ToTerm("function") + identifier + "(" + parameterListDeclaration + ")" 
                + (ToTerm(":") + identifier).Q()
                + block;
            returnStatement.Rule = ToTerm("return") + expression;
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

            this.RegisterOperators(1, Associativity.Right, "==", "!=", ">", "<", "->", "-<");
            this.RegisterOperators(2, Associativity.Right, "=", "+=", "-=", "*=", "/=", "%=", "^=", "<<=", ">>=", "&=", "|=", "-*=", "-/=", "-%=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/", "%");
            this.RegisterOperators(5, Associativity.Left, "-*", "-/", "-%");
            this.RegisterOperators(6, Associativity.Left, "<<", ">>", "&", "|", "^", "!");
            this.RegisterOperators(7, Associativity.Left, ":");
            
            this.RegisterOperators(8, Associativity.Left, "{", "}", "[", "]");
        }

    }
}