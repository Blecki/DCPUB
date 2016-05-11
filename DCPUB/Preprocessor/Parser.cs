using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Preprocessor
{    
    public class ResultBuilder
    {
        private StringBuilder Worker = new StringBuilder();
        public int LinesWritten { get; private set; }

        public ResultBuilder()
        {
            LinesWritten = 0;
        }

        public void Append(String S)
        {
            LinesWritten += S.Count(c => c == '\n');
            Worker.Append(S);
        }

        public override string ToString()
        {
            return Worker.ToString();
        }
    }

    public class Parser
    {
        public static bool IsIdentifier(char c) 
        {
            return char.IsLetter(c) | char.IsDigit(c) | c == '_';
        }
                
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
            while (!state.AtEnd() && char.IsWhiteSpace(state.Next())) state.Advance();
        }

        public static int SkipExcludedBlock(ParseState state)
        {
            //Scan document for '#endif".
            int depth = 0;
            int startLine = state.currentLine;
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
                    if (endifLine.Trim() != "#endif")
                    {
                        state.Error("#endif should be alone on line.");
                        return 0;
                    }
                    depth -= 1;
                    if (depth < 0)
                    {
                        // These lines are excised entirely from the finished file; we don't want their lines
                        // messing up our line counts.
                        //state.LineLocationTable.AddLocation(state.filename, startLine, state.currentLine);
                        return state.currentLine - startLine;
                    }
                    else continue;
                }
                else state.Advance();
            }

            return 0;
        }

        public static String ParseDirectiveName(ParseState state)
        {
            SkipWhitespace(state);
            if (state.AtEnd() || !IsIdentifier(state.Next()))
            {
                state.Error("Expected an identifier after a preprocessor directive.");
                return "";
            }
            var ident = ParseIdentifier(state);
            SkipWhitespace(state);
            if (!state.AtEnd())
                state.Error("Did not expect this directive to have a body.");
            return ident;
        }

        public static String ParseDirective(ParseState state)
        {
            var directive = "";

            while (!state.AtEnd() && !char.IsWhiteSpace(state.Next()))
            {
                directive += state.Next();
                state.Advance();
            }

            var rest = ParseLine(state);

            if (directive == "#define")
            {
                var dState = new ParseState(rest);
                SkipWhitespace(dState);
                if (dState.AtEnd() || !IsIdentifier(dState.Next()))
                {
                    state.Error("Define what?");
                    return "";
                }
                var ident = ParseIdentifier(dState);
                var arguments = new List<String>();
                if (!dState.AtEnd() && dState.Next() == '(')
                {
                    dState.Advance(); //skip (
                    SkipWhitespace(dState);
                    while (!dState.AtEnd() && dState.Next() != ')')
                    {
                        var argname = ParseIdentifier(dState);
                        if (String.IsNullOrEmpty(argname))
                        {
                            state.Error("Empty argument name in macro.");
                            argname = "_";
                        }

                        arguments.Add(argname);
                        SkipWhitespace(dState);
                        if (!dState.AtEnd() && dState.Next() == ',')
                        {
                            dState.Advance();
                            SkipWhitespace(dState);
                        }
                        else if (!dState.AtEnd() && dState.Next() == ')') break;
                        else state.Error("Error parsing macro argument names.");
                    }
                    if (dState.AtEnd() || dState.Next() != ')') state.Error("Error parsing macro argument names.");
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
                    return new String('\n', SkipExcludedBlock(state));
                return "";
            }
            else if (directive == "#ifndef")
            {
                if (state.macros.ContainsKey(ParseDirectiveName(new ParseState(rest))))
                    return new String('\n', SkipExcludedBlock(state));
                return "";
            }
            else if (directive == "#endif") return "";
            else if (directive == "#include")
            {
                var fileName = rest.Trim();
                var fullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(state.filename), fileName);
                fullPath = System.IO.Path.GetFullPath(fullPath);

                var result = Preprocess(fullPath, state.readIncludeFile(fullPath), state);
                state.LineLocationTable.Merge(result.Item2.LineLocationTable, state.currentLine - 1);
                state.LineLocationTable.AddLocation(state.filename, state.currentLine + result.Item2.currentLine, state.currentLine);
                state.currentLine += result.Item2.currentLine;
                return /*"//" + directive + rest + "\n" + */ result.Item1;
            }
            else
            {
                state.Error("Unknown preprocessor directive.");
                return "";
            }
        }

        public static String Expand(string name, ParseState state)
        {
            if (state.macros.ContainsKey(name))
            {
                var macro = state.macros[name];
                if (macro.arguments.Count == 0)
                    return state.macros[name].body;
                SkipWhitespace(state);
                if (state.AtEnd() || state.Next() != '(')
                {
                    state.Error("Expected arguments to macro.");
                    return "";
                }
                else
                {
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
                    if (arguments.Count != macro.arguments.Count) state.Error("Wrong number of arguments to macro.");
                    var expansionState = new ParseState(macro.body);
                    for (int i = 0; i < macro.arguments.Count; ++i)
                        expansionState.macros.Add(macro.arguments[i], new Macro { body = arguments[i], arguments = new List<string>() });
                    return ParseBlock(null, expansionState);
                }
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
                if (state.MatchNext("\\\n") || state.MatchNext("\\\r\n"))
                {
                    state.Advance();
                    SkipWhitespace(state);
                }
                else if (state.MatchNext("\r\n"))
                {
                    result.Append("\n");
                    //result.Append(state.Next());
                    state.Advance();
                    state.Advance();
                }
                else
                {
                    result.Append(state.Next());
                    state.Advance();
                }
            }

            return result.ToString();
        }

        public static Tuple<String, ParseState>
            Preprocess(String FileName, String file, ParseState parentState)
        {
            var state = new ParseState(CollapseLineEscapes(file));
            if (parentState != null)
            {
                state.macros = parentState.macros;
                state.readIncludeFile = parentState.readIncludeFile;
                state.ReportErrors = parentState.ReportErrors;
            }
            state.filename = FileName;
            state.LineLocationTable = new PreprocessedLineLocationTable();
            state.LineLocationTable.AddLocation(FileName, 0, 0);
            return Tuple.Create(ParseBlock(null, state), state);
        }

        public static Tuple<String, ParseState>
            Preprocess(String FileName, String file, Func<String,String> fileSource, Action<String> ReportErrors)
        {
            var state = new ParseState(CollapseLineEscapes(file));
            state.filename = FileName;
            state.readIncludeFile = fileSource;
            state.ReportErrors = ReportErrors;
            state.LineLocationTable = new PreprocessedLineLocationTable();
            state.LineLocationTable.AddLocation(FileName, 0, 0);
            return Tuple.Create(ParseBlock(null, state), state);
        }
    }
}
