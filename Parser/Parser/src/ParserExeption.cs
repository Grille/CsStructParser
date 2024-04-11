using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grille.Parsing.Tcf
{
    public class TcfParserException : Exception
    {
        public int Line { get; }

        internal TcfParserException(int line, string message) : base("line " + line + ": " + message)
        {
            Line = line;
        }

        internal TcfParserException(Token token, string message) : this(token.Line, $"Unexpected token \"{token.Value}\" {message}")
        { }

        internal TcfParserException(Token token) : this(token.Line, $"Unexpected token \"{token.Value}\"")
        { }
    }
}
