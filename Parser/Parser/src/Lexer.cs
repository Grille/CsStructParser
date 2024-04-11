using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grille.Parsing.Tcf
{
    internal static class Lexer
    {
        static char[] symbolChars = new char[] { ',', '{', '}', '[', ']', '=', '+', '-', '*', '/', ':', '<', '>', '>' };
        static char[] endChars = new char[] { '\n', ' ', '\r', ';', ';' };

        enum CommentMode
        {
            None,
            SingleLine,
            MultiLine,
        }
        public static Token[] Tokenize(string data)
        {
            var tokens = new List<Token>();
            int line = 1;
            CommentMode commentMode = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '\n')
                    line++;
                if (commentMode == 0)
                {
                    if (data[i] == '/' && data[i + 1] == '/') commentMode = CommentMode.SingleLine;
                    else if (data[i] == '/' && data[i + 1] == '*') commentMode = CommentMode.MultiLine;
                    else
                    {
                        if (data[i] == '"')
                        {
                            i += 1;
                            int start = i, end = 0;
                            while (data[i] != '"' || data[i - 1] == '\\')
                            {
                                if (data[i] == '\n')
                                    line++;
                                end = i++;
                                if (i >= data.Length)
                                    break;
                            }
                            // += 1;
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Console.Write(data.Substring(start, end- start+1));
                            var value = end != 0 ? data.Substring(start, end - start + 1).Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\"", "\"") : "";
                            var token = new Token(TokenKind.String, line, value);
                            tokens.Add(token);

                        }
                        else if (data[i] == '(' && data[i + 1] == ')' && data[i + 2] == '{')
                        {
                            i += 3;
                            int start = i, end = 0;
                            int scope = 0;
                            while (data[i] != '}' || scope > 0)
                            {
                                switch (data[i])
                                {
                                    case '{': scope++; break;
                                    case '}': scope--; break;
                                    case '\n': line++; break;
                                }
                                end = i++;
                                if (i >= data.Length)
                                    break;
                            }
                            // += 1;
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Console.Write(data.Substring(start, end- start+1));
                            var value = end != 0 ? data.Substring(start, end - start + 1) : "";
                            var token = new Token(TokenKind.String, line, value);
                            tokens.Add(token);

                        }
                        else if (data[i] == ',' || data[i] == '{' || data[i] == '}' || data[i] == '[' || data[i] == ']' || data[i] == '=' || data[i] == '+' || data[i] == '-' || data[i] == '*' || data[i] == '/' || data[i] == ':' || data[i] == '<' || data[i] == '>' || data[i] == '&')
                        {
                            //Console.ForegroundColor = ConsoleColor.Blue;
                            //Console.Write(data[i]);

                            var token = new Token(TokenKind.Symbol, line, data[i].ToString());
                            tokens.Add(token);
                        }
                        else if (data[i] == '\n' || data[i] == ' ' || data[i] == '\r' || data[i] == ';' || data[i] == ';')
                        {
                            /*
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(data[i]);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            */
                        }
                        else
                        {
                            int start = i, end = 0;
                            while ((data[i] >= 'A' && data[i] <= 'Z') || (data[i] >= 'a' && data[i] <= 'z') || (data[i] >= '0' && data[i] <= '9') || data[i] == '_' || data[i] == '.')
                            {
                                end = i++;
                                if (i >= data.Length)
                                    break;
                            }
                            i -= 1;
                            if (end != 0)
                            {
                                //Console.ForegroundColor = ConsoleColor.Gray;
                                //Console.Write(data.Substring(start, end - start + 1)+" ");
                                var value = data.Substring(start, end - start + 1);
                                var kind = GetTokenKindFromValue(value);
                                var token = new Token(kind, line, value);
                                tokens.Add(token);
                            }
                            else
                            {
                                throw new TcfParserException(line, "Unexpected symbol \"" + data[i + 1] + "\"");
                            }
                        }
                    }
                }
                else if (commentMode == CommentMode.SingleLine && data[i] == '\n') commentMode = 0;
                else if (commentMode == CommentMode.MultiLine && data[i] == '*' && data[i + 1] == '/') { commentMode = 0; i++; }
            }

            return tokens.ToArray();
        }

        private static TokenKind GetTokenKindFromFirstChar(char value)
        {
            if (value >= '0' && value <= '9') { 
                return TokenKind.Number; 
            }

            if ((value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z') || value == '_' || value == '.') {
                return TokenKind.Word;
            }

            throw new InvalidOperationException();
        }

        private static TokenKind GetTokenKindFromValue(string value)
        {
            var kind = GetTokenKindFromFirstChar(value[0]);

            if (kind == TokenKind.Word)
            {
                if (value == "true" || value == "false")
                {
                    return TokenKind.Bool;
                }
            }

            return kind;
        }
    }
}
