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

        // scan an individual token
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
                case '%': addToken(Token.TokenType.Percent); break;

                //case '?': addToken(Token.TokenType.If); break;
                //case '|': addToken(Token.TokenType.Else); break;

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

                // if not a language token, determine what the user has entered and if it is valid
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
                        Basil.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        // constructs a token with a string literal
        private void makeString () {                                   
            while (peek() != '"' && !isAtEnd()) {                   
              if (peek() == '\n') line++;                           
              advance();
            }

            // Unterminated string.                                 
            if (isAtEnd()) {
                Basil.error(line, "Unterminated string.");              
                return;                                               
            }

            // The closing ".                                       
            advance();

            // Trim the surrounding quotes.                         
            string value = source.Substring(start + 1, (current - 1) - (start + 1));
            addToken(Token.TokenType.String, value);                                
        }

        // constructs a token with a numeric literal
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
                double.Parse(source.Substring(start, current - start)));
        }

        // constructs a token for a keyword if valid
        private void identifier()
        {
            while (isAlphaNumeric(peek())) advance();

            // See if the identifier is a reserved word.   
            string text = source.Substring(start, current - start);

            Token.TokenType type;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            else type = Token.TokenType.Identifier;
            addToken(type);
        }

        // return if the character is a letter
        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        // return if the character is alphanumeric
        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        // return if the character is a digit
        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }


        // conditionally advance if the expected character is the current character 
        private bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        // return the current character in the source without consuming it
        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        // return the next character in the source without consuming it or the previous character
        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        // consumes the next character in the source and returns it
        private char advance()
        {
            current++;
            return source[current - 1];
        }

        // creates a token for the current lexeme
        private void addToken(Token.TokenType type)
        {
            addToken(type, null);
        }

        // creates a token for the current lexeme, including a literal value
        private void addToken(Token.TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        // determine if the iterator has reached the end of the source string
        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        // keyword mapping to token type
        private static readonly Dictionary<string, Token.TokenType> keywords = new Dictionary<string, Token.TokenType>
        {
            {"and",    Token.TokenType.And },
            {"class",  Token.TokenType.Class  },
            {"else",      Token.TokenType.Else   },
            {"false",  Token.TokenType.False  },
            {"for",    Token.TokenType.For    },
            {"fun",    Token.TokenType.Fun    },
            {"if",      Token.TokenType.If     },
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
