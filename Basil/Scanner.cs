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

        public List<Token> scanTokens()
        {
            while(!isAtEnd())
            {
                start = current;
                scanToken();
            }

            tokens.Add(new Token(Token.TokenType.EOF, "", null, line));
            return tokens;
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                // Single-character
                case '(': addToken(Token.TokenType.LeftParenthesis); break;
                case ')': addToken(Token.TokenType.RightParenthesis); break;
                case '{': addToken(Token.TokenType.LeftBrace); break;
                case '}': addToken(Token.TokenType.RightBrace); break;
                case ',': addToken(Token.TokenType.Comma); break;
                case '.': addToken(Token.TokenType.Dot); break;
                case '-': addToken(Token.TokenType.Minus); break;
                case '+': addToken(Token.TokenType.Plus); break;
                case ';': addToken(Token.TokenType.Semicolon); break;
                case '*': addToken(Token.TokenType.Star); break;
                
                // Single or double characters
                case '!': addToken(match('=') ? Token.TokenType.BangEqual : Token.TokenType.Bang); break;
                case '=': addToken(match('=') ? Token.TokenType.EqualEqual : Token.TokenType.Equal); break;
                case '<': addToken(match('=') ? Token.TokenType.LessEqual : Token.TokenType.Less); break;
                case '>': addToken(match('=') ? Token.TokenType.GreaterEqual : Token.TokenType.Greater); break;
                case '/':
                    if (match('/'))
                    {
                        // A comment goes until the end of the line.                
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else
                    {
                        addToken(Token.TokenType.Slash);
                    }
                    break;

                // Literals
                case '"': makeString(); break;

                // White-space
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.                      
                    break;
                case '\n':
                    line++;
                    break;

                default:
                    if (isDigit(c))
                    {
                        makeNumber();
                    }
                    else if (isAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        //Basil.error(line, "Unexpected character.");
                        Console.WriteLine("TEMP_ERROR::UNEXPECTED CHARACTER");
                    }
                    break;
            }
        }

        private void makeString () {                                   
            while (peek() != '"' && !isAtEnd()) {                   
              if (peek() == '\n') line++;                           
              advance();
            }

            // Unterminated string.                                 
            if (isAtEnd()) {
                //Basil.error(line, "Unterminated string.");              
                Console.WriteLine("TEMP_ERROR::UNTERMINATED STRING");
                return;                                               
            }

            // The closing ".                                       
            advance();

            // Trim the surrounding quotes.                         
            string value = source.Substring(start + 1, (current - 1) - start);
            addToken(Token.TokenType.String, value);                                
        }

        private void makeNumber()
        {
            while (isDigit(peek())) advance();

            // Look for a fractional part.                            
            if (peek() == '.' && isDigit(peekNext()))
            {
                // Consume the "."                                      
                advance();

                while (isDigit(peek())) advance();
            }

            addToken(Token.TokenType.Number,
                float.Parse(source.Substring(start, current - start)));
        }

        private void identifier()
        {
            while (isAlphaNumeric(peek())) advance();

            // See if the identifier is a reserved word.   
            String text = source.Substring(start, current - start);

            Token.TokenType type;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            else type = Token.TokenType.Idenfitier;
            addToken(type);
        }

        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private char advance()
        {
            current++;
            return source[current - 1];
        }

        private void addToken(Token.TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(Token.TokenType type, Object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private static readonly Dictionary<string, Token.TokenType> keywords = new Dictionary<string, Token.TokenType>
        {
            {"and",    Token.TokenType.And },
            {"class",  Token.TokenType.Class  },
            {"else",   Token.TokenType.Else   },
            {"false",  Token.TokenType.False  },
            {"for",    Token.TokenType.For    },
            {"fun",    Token.TokenType.Fun    },
            {"if",     Token.TokenType.If     },
            {"nil",    Token.TokenType.Nil    },
            {"or",     Token.TokenType.Or     },
            {"print",  Token.TokenType.Print  },
            {"return", Token.TokenType.Return },
            {"super",  Token.TokenType.Super  },
            {"this",   Token.TokenType.This   },
            {"true",   Token.TokenType.True   },
            {"var",    Token.TokenType.Var    },
            {"while",  Token.TokenType.While  }
        };
    }
}
