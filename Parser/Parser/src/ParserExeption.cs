using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    class ParserException : Exception
    {
        public int Line { get;private set;}
        public ParserException(int line, string message) : base("line " + line + ": " + message)
        {
            Line = line;
        }
        public ParserException(Token token,string message) : base("line " + token.line + ": "+message)
        {
            Line = token.line;
        }
        public ParserException(Token token) : base("line " + token.line + ": " + "Unexpected token \"" + token.value + "\"")
        {
            Line = token.line;
        }
    }
}
