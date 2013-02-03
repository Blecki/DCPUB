using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Preprocessor
{    
    public class Parser
    {
        private static string identifier = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        public static bool IsIdentifier(char c) { return identifier.Contains(c); }
        private static string whitespace = " \t\r\n";
        public static bool IsWhitespace(char c) { return whitespace.Contains(c); }

        public static string ParseIdentifier(ParseState state)
        {
            string r = "";
            while (!state.AtEnd() && IsIdentifier(state.Next()))
            {
                r += state.Next();
                state.Advance();
            }
            return r;
        }

        public static String ParseLine(ParseState state)
        {
            var rest = "";
            while (!state.AtEnd() && state.Next() != '\n' && state.Next() != '\r')
            {
                rest += state.Next();
                state.Advance();
            }
            return rest;
        }

        public static void SkipWhitespace(ParseState state)
        {
            while (!state.AtEnd() && IsWhitespace(state.Next())) state.Advance();
        }

        public static void SkipExcludedBlock(ParseState state)
        {
            //Scan document for '#endif".
            int depth = 0;
            while (!state.AtEnd())
            {
                if (state.lastWasNewline && (state.MatchNext("#ifdef") || state.MatchNext("#ifndef")))
                {
                    depth += 1;
                    state.Advance();
                }
                else if (state.lastWasNewline && state.MatchNext("#endif"))
                {
                    var endifLine = ParseLine(state);
                    if (endifLine.Trim() != "#endif") throw new CompileError("Endif should be bare.");
                    depth -= 1;
                    if (depth < 0) return;
                    else continue;
                }
                else state.Advance();
            }
        }

        public static String ParseDirectiveName(ParseState state)
        {
            SkipWhitespace(state);
            if (state.AtEnd() || !IsIdentifier(state.Next())) throw new CompileError("Expected an identifier after a preprocessor directive.");
            var ident = ParseIdentifier(state);
            SkipWhitespace(state);
            if (!state.AtEnd()) throw new CompileError("Did not expect this directive to have a body.");
            return ident;
        }

        public static String ParseDirective(ParseState state)
        {
            var directive = "";
            while (!state.AtEnd() && !IsWhitespace(state.Next()))
            {
                directive += state.Next();
                state.Advance();
            }
            var rest = ParseLine(state);

            if (directive == "#define")
            {
                var dState = new ParseState(rest);
                SkipWhitespace(dState);
                if (dState.AtEnd() || !IsIdentifier(dState.Next())) throw new CompileError("Define what?");
                var ident = ParseIdentifier(dState);
                var arguments = new List<String>();
                if (!dState.AtEnd() && dState.Next() == '(')
                {
                    dState.Advance(); //skip (
                    SkipWhitespace(dState);
                    while (!dState.AtEnd() && dState.Next() != ')')
                    {
                        var argname = ParseIdentifier(dState);
                        if (String.IsNullOrEmpty(argname)) throw new CompileError("Empty argument name in macro.");
                        arguments.Add(argname);
                        SkipWhitespace(dState);
                        if (!dState.AtEnd() && dState.Next() == ',')
                        {
                            dState.Advance();
                            SkipWhitespace(dState);
                        }
                        else if (!dState.AtEnd() && dState.Next() == ')') break;
                        else throw new CompileError("Error parsing macro argument names.");
                    }
                    if (dState.AtEnd() || dState.Next() != ')') throw new CompileError("Error parsing macro argument names.");
                    dState.Advance(); //skip )
                }
                SkipWhitespace(dState);
                var body = ParseLine(dState);
                state.macros.Upsert(ident, new Macro { body = body, arguments = arguments });
                return "";
            }
            else if (directive == "#undef")
            {
                var ident = ParseDirectiveName(new ParseState(rest));
                if (state.macros.ContainsKey(ident)) state.macros.Remove(ident);
                return "";
            }
            else if (directive == "#ifdef")
            {
                if (!state.macros.ContainsKey(ParseDirectiveName(new ParseState(rest))))
                    SkipExcludedBlock(state);
                return "";
            }
            else if (directive == "#ifndef")
            {
                if (state.macros.ContainsKey(ParseDirectiveName(new ParseState(rest))))
                    SkipExcludedBlock(state);
                return "";
            }
            else if (directive == "#endif") return "";
            else if (directive == "#include")
            {
                var fileName = rest.Trim();
                return Preprocess(state.readIncludeFile(fileName), state);
            }
            else
                throw new CompileError("Unknown preprocessor directive.");
        }

        public static String Expand(string name, ParseState state)
        {
            if (state.macros.ContainsKey(name))
            {
                var macro = state.macros[name];
                if (macro.arguments.Count == 0)
                    return state.macros[name].body;
                SkipWhitespace(state);
                if (state.AtEnd() || state.Next() != '(') throw new CompileError("Expected arguments to macro.");
                //Peel off arguments, separated by ,
                state.Advance(); //skip '('
                List<String> arguments = new List<String>();
                while (!state.AtEnd())
                {
                    var argument = ParseBlock((c) => c == ',' || c == ')', state);
                    bool foundEnd = false;
                    if (argument.Length != 0 && argument[argument.Length - 1] == ')') foundEnd = true;
                    if (argument.Length != 0)
                        argument = argument.Substring(0, argument.Length - 1);
                    arguments.Add(argument);
                    if (foundEnd) break;
                }
                if (arguments.Count != macro.arguments.Count) throw new CompileError("Wrong number of arguments to macro.");
                var expansionState = new ParseState(macro.body);
                for (int i = 0; i < arguments.Count; ++i)
                    expansionState.macros.Add(macro.arguments[i], new Macro { body = arguments[i], arguments = new List<string>() });
                return ParseBlock(null, expansionState);
            }
            else
                return name;
        }

        public static string ParseBlock(Func<char, bool> isTerminal, ParseState state)
        {
            string r = "";
            if (isTerminal != null)
            {
                r += state.Next();
                state.Advance();
            }
            while (!state.AtEnd())
            {
                if (state.Next() == '#' && state.lastWasNewline)
                {
                    r += ParseDirective(state);
                }
                else if (isTerminal != null && isTerminal(state.Next()))
                {
                    r += state.Next();
                    state.Advance();
                    return r;
                }
                else if (IsIdentifier(state.Next()))
                {
                    var identifier = ParseIdentifier(state);
                    r += Expand(identifier, state);
                }
                else if (state.Next() == '(')
                    r += ParseBlock((c) => c == ')', state);
                else if (state.Next() == '{')
                    r += ParseBlock((c) => c =='}', state);
                else if (state.Next() == '[')
                    r += ParseBlock((c) => c == ']', state);
                else
                {
                    r += state.Next();
                    state.Advance();
                }
            }
            return r;
        }

        public static String CollapseLineEscapes(String file)
        {
            var result = new StringBuilder();
            var state = new ParseState(file);
            while (!state.AtEnd())
            {
                if (state.MatchNext("\\\n") || state.MatchNext("\\\r"))
                {
                    state.Advance();
                    SkipWhitespace(state);
                }
                else
                {
                    result.Append(state.Next());
                    state.Advance();
                }
            }
            return result.ToString();
        }

        public static String Preprocess(String file, ParseState parentState)
        {
            var state = new ParseState(CollapseLineEscapes(file));
            if (parentState != null)
            {
                state.macros = parentState.macros;
                state.readIncludeFile = parentState.readIncludeFile;
            }
            return ParseBlock(null, state);
        }

        public static String Preprocess(String file, Func<String,String> fileSource)
        {
            var state = new ParseState(CollapseLineEscapes(file));
            state.readIncludeFile = fileSource;
            return ParseBlock(null, state);
        }
    }
}
