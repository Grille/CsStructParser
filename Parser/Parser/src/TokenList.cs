using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGL.IO
{
    class TokenList
    {
        Token[] tokens;
        public TokenList(int length)
        {
            tokens = new Token[length];
        }
        public int Length
        {
            get => tokens.Length;
            set
            {
                Array.Resize(ref tokens, value);
            }
        }
        public ref Token this[int i]
        {
            get
            {
                return ref tokens[i];
            }
        }
    }
}
