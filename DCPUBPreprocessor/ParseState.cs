using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Preprocessor
{
    public class Macro
    {
        public List<String> arguments;
        public String body;
    }

    public class ParseState
    {
        public int start;
        public int end;
        public String source;
        public int currentLine = 1;
        public String filename;
        public bool lastWasNewline = false;
        public Dictionary<String, Macro> macros = new Dictionary<string, Macro>();
        public Func<String, String> readIncludeFile;

        public ParseState(String source)
        {
            this.source = source;
            start = 0;
            end = source.Length;
            lastWasNewline = true;
        }

        public char Next()
        {
            if (start >= source.Length)
                throw new CompileError("Preprocessor parser error.");
            return source[start]; 
        }

        public void Advance()
        {
            if (Next() == '\n' || Next() == '\r') lastWasNewline = true;
            else lastWasNewline = false;
            start += 1;
            if (start > end) throw new CompileError("Unexpected end of file");
            if (!AtEnd() && Next() == '\n') 
            
                currentLine += 1;
        }
        
        public bool AtEnd() { return start == end; }

        public bool MatchNext(String str) 
        {
            if (str.Length + start > source.Length) return false;
            return str == source.Substring(start, str.Length); 
        }

        public bool PeekForSpace()
        {
            return !AtEnd() && source[start + 1] == ' ';
        }
    }
}
