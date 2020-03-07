using System;
using System.Collections.Generic;

namespace BasilLang
{
    class Scanner
    {
        readonly string source;
        readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        // scan all tokens from source string
        public List<Token> ScanTokens()
        {
            while(!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(Token.TokenType.EOF, "", null, line));
            return tokens;
        }

        // scan an individual token
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single-character
                case '(': AddToken(Token.TokenType.LeftParenthesis); break;
                case ')': AddToken(Token.TokenType.RightParenthesis); break;
                case '{': AddToken(Token.TokenType.LeftBrace); break;
                case '}': AddToken(Token.TokenType.RightBrace); break;
                //case '[': addToken(Token.TokenType.LeftBracket); break;
                //case ']': addToken(Token.TokenType.RightBracket); break;
                case ',': AddToken(Token.TokenType.Comma); break;
                case '.': AddToken(Token.TokenType.Dot); break;
                //case '-': AddToken(Token.TokenType.Minus); break;
                //case '+': AddToken(Token.TokenType.Plus); break;
                case ';': AddToken(Token.TokenType.Semicolon); break;
                //case '*': AddToken(Token.TokenType.Star); break;
                //case '%': AddToken(Token.TokenType.Percent); break;

                case '-':
                    if (Match('-')) AddToken(Token.TokenType.MinusMinus);
                    else if (Match('=')) AddToken(Token.TokenType.MinusEqual);
                    else AddToken(Token.TokenType.Minus);
                    break;
                case '+':
                    if (Match('+')) AddToken(Token.TokenType.PlusPlus);
                    else if (Match('=')) AddToken(Token.TokenType.PlusEqual);
                    else AddToken(Token.TokenType.Plus);
                    break;
                case '*': AddToken(Match('=') ? Token.TokenType.StarEqual : Token.TokenType.Minus); break;
                case '%': AddToken(Match('=') ? Token.TokenType.PercentEqual : Token.TokenType.Minus); break;

                //case '?': addToken(Token.TokenType.If); break;
                //case '|': addToken(Token.TokenType.Else); break;

                // Single or double characters
                case '!': AddToken(Match('=') ? Token.TokenType.BangEqual : Token.TokenType.Bang); break;
                case '=': AddToken(Match('=') ? Token.TokenType.EqualEqual : Token.TokenType.Equal); break;
                case '<': AddToken(Match('=') ? Token.TokenType.LessEqual : Token.TokenType.Less); break;
                case '>': AddToken(Match('=') ? Token.TokenType.GreaterEqual : Token.TokenType.Greater); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.                
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else if (Match('*'))
                    {
                        // A comment goes until the end of the block                
                        while (!IsAtEnd())
                        {
                            if (Peek() == '*' && PeekNext() == '/')
                            {
                                Advance();
                                Advance();
                                break;
                            }

                            Advance();
                        }
                    }
                    else if (Match('='))
                    {
                        AddToken(Token.TokenType.SlashEqual);
                    }
                    else
                    {
                        AddToken(Token.TokenType.Slash);
                    }
                    break;

                // Literals
                case '"': MakeString(); break;

                // White-space
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.                      
                    break;
                case '\n':
                    line++;
                    break;

                // if not a language token, determine what the user has entered and if it is valid
                default:
                    if (IsDigit(c))
                    {
                        MakeNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Basil.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        // constructs a token with a string literal
        private void MakeString () {                                   
            while (Peek() != '"' && !IsAtEnd()) {                   
              if (Peek() == '\n') line++;                           
              Advance();
            }

            // Unterminated string.                                 
            if (IsAtEnd()) {
                Basil.Error(line, "Unterminated string.");              
                return;                                               
            }

            // The closing ".                                       
            Advance();

            // Trim the surrounding quotes.                         
            string value = source.Substring(start + 1, (current - 1) - (start + 1));
            AddToken(Token.TokenType.String, value);                                
        }

        // constructs a token with a numeric literal
        private void MakeNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.                            
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."                                      
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(Token.TokenType.Number,
                double.Parse(source.Substring(start, current - start)));
        }

        // constructs a token for a keyword if valid
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if the identifier is a reserved word.   
            string text = source.Substring(start, current - start);

            Token.TokenType type;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            else type = Token.TokenType.Identifier;
            AddToken(type);
        }

        // return if the character is a letter
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        // return if the character is alphanumeric
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        // return if the character is a digit
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }


        // conditionally advance if the expected character is the current character 
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        // return the current character in the source without consuming it
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        // return the next character in the source without consuming it or the previous character
        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        // consumes the next character in the source and returns it
        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        // creates a token for the current lexeme
        private void AddToken(Token.TokenType type)
        {
            AddToken(type, null);
        }

        // creates a token for the current lexeme, including a literal value
        private void AddToken(Token.TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        // determine if the iterator has reached the end of the source string
        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        // keyword mapping to token type
        private static readonly Dictionary<string, Token.TokenType> keywords = new Dictionary<string, Token.TokenType>
        {
            {"and",         Token.TokenType.And         },
            {"break",       Token.TokenType.Break       },
            {"class",       Token.TokenType.Class       },
            {"continue",    Token.TokenType.Continue    },
            {"else",        Token.TokenType.Else        },
            {"false",       Token.TokenType.False       },
            {"for",         Token.TokenType.For         },
            {"fun",         Token.TokenType.Fun         },
            {"if",          Token.TokenType.If          },
            {"nil",         Token.TokenType.Nil         },
            {"or",          Token.TokenType.Or          },
            {"print",       Token.TokenType.Print       },
            {"return",      Token.TokenType.Return      },
            {"super",       Token.TokenType.Super       },
            {"this",        Token.TokenType.This        },
            {"true",        Token.TokenType.True        },
            {"var",         Token.TokenType.Var         },
            {"while",       Token.TokenType.While       }
        };
    }
}
