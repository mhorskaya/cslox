using System.Collections.Generic;
using static Lox.TokenType;

namespace Lox
{
    public class Scanner
    {
        public string Source { get; }
        public List<Token> Tokens { get; } = new List<Token>();
        public int Start { get; private set; }
        public int Current { get; private set; }
        public int Line { get; private set; } = 1;

        public static Dictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>
        {
            {"and", AND},
            {"class", CLASS},
            {"else", ELSE},
            {"false", FALSE},
            {"for", FOR},
            {"fun", FUN},
            {"if", IF},
            {"nil", NIL},
            {"or", OR},
            {"print", PRINT},
            {"return", RETURN},
            {"super", SUPER},
            {"this", THIS},
            {"true", TRUE},
            {"var", VAR},
            {"while", WHILE}
        };

        public Scanner(string source)
        {
            Source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                Start = Current;
                ScanToken();
            }

            Tokens.Add(new Token(EOF, string.Empty, null, Line));
            return Tokens;
        }

        private void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    Line++;
                    break;

                case '"':
                    String();
                    break;

                default:
                    if (IsDigit(c)) Number();
                    else if (IsAlpha(c)) Identifier();
                    else Lox.Error(Line, "Unexpected character.");
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();

            var text = Source[Start..Current];
            var type = Keywords.ContainsKey(text) ? Keywords[text] : IDENTIFIER;

            AddToken(type);
        }

        private void Number()
        {
            while (IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek()))
                    Advance();
            }

            AddToken(NUMBER, double.Parse(Source[Start..Current]));
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                    Line++;

                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(Line, "Unterminated string.");
                return;
            }

            Advance();

            AddToken(STRING, Source[(Start + 1)..(Current - 1)]);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (Source[Current] != expected) return false;
            Current++;
            return true;
        }

        private char Advance()
        {
            return Source[++Current - 1];
        }

        private char Peek()
        {
            return IsAtEnd() ? '\0' : Source[Current];
        }

        private char PeekNext()
        {
            return Current + 1 >= Source.Length ? '\0' : Source[Current + 1];
        }

        private bool IsAtEnd()
        {
            return Current >= Source.Length;
        }

        private static bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void AddToken(TokenType type, object literal = null)
        {
            Tokens.Add(new Token(type, Source[Start..Current], literal, Line));
        }
    }
}