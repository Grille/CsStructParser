using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grille.Parsing.Tcf
{
    internal class TokenList
    {
        public Token[] Tokens { get; }

        public TokenList(Token[] tokens)
        {
            Tokens = tokens;
        }

        public Token Get(int position)
        {
            if (position >= Tokens.Length)
            {
                throw new TcfParserException(Tokens[Length - 1].Line, "Unexpected End of File.");
            }
            return Tokens[position];
        }

        public int FindIndexOf(TokenKind kind, string value)
        {
            for (int i = 0; i < Tokens.Length; i++)
            {
                if (Tokens[i].Kind == kind && Tokens[i].Value == value) return i;
            }
            return -1;
        }

        public int FindIndexOf(string value)
        {
            for (int i = 0; i < Tokens.Length; i++)
            {
                if (Tokens[i].Value == value) return i;
            }
            return -1;
        }

        public Token this[int index] => Get(index);

        public int Length => Tokens.Length;
    }
}
