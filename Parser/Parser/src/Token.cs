using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grille.Parsing.Tcf
{
    enum TokenKind { Word, Number, Bool, String, Symbol };

    class Token
    {
        public int Line { get; }
        public TokenKind Kind { get; }
        public string Value { get; }

        public Token(TokenKind kind, int line, string value)
        {
            Kind = kind;
            Line = line;
            Value = value;
        }

        public static Token Empty { get; } = new Token(TokenKind.Word, 0, string.Empty);

        public bool Equals(TokenKind kind) => kind == this.Kind;

        public bool Equals(TokenKind kind, string value) => kind == this.Kind && value == this.Value;

        public bool Equals(string value) => value == this.Value;
    }
}
