using System.Collections.Generic;
using static Lox.TokenType;

namespace Lox
{
    public class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start;
        private int _current;
        private int _line = 1;

        private static readonly Dictionary<string, TokenType> Keywords =
            new Dictionary<string, TokenType>
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
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(EOF, string.Empty, null, _line));
            return _tokens;
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
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
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
                    _line++;
                    break;

                case '"':
                    String();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = _source[_start.._current];
            var type = Keywords.ContainsKey(text) ? Keywords[text] : IDENTIFIER;

            AddToken(type);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(NUMBER, double.Parse(_source[_start.._current]));
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }

            Advance();

            AddToken(STRING, _source[(_start + 1)..(_current - 1)]);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        private char Advance()
        {
            return _source[++_current - 1];
        }

        private char Peek()
        {
            return IsAtEnd() ? '\0' : _source[_current];
        }

        private char PeekNext()
        {
            return _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            _tokens.Add(new Token(type, _source[_start.._current], literal, _line));
        }
    }
}