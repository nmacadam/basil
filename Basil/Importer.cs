using System;
using System.Collections.Generic;

namespace BasilLang
{
    // Pretty messy, but allows importing code from external files
    // note that there is no safety check for importing the same file multiple times
    // the import "path.bsl" call is directly replaced with an external file's code

    public class Importer
    {
        public enum PreprocessingTokens
        {
            Import
        }

        // import-once is not functional
        private readonly List<string> imports = new List<string>();
        private string source;

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Importer(string source)
        {
            this.source = source;
        }

        public string import()
        {
            while (!isAtEnd())
            {
                start = current;
                scanToken();
            }
            return source;
        }

        private void scanToken()
        {
            char c = advance();
            if (isAlpha(c))
            {
                identifier();
            }
        }

        // constructs a token for a keyword if valid
        private void identifier()
        {
            int tokenStart = current - 1;

            while (isAlphaNumeric(peek()))
            {
                advance();
            }

            // See if the identifier is a reserved word.   
            string text = source.Substring(start, current - start);

            PreprocessingTokens type;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];

                switch (type)
                {
                    case PreprocessingTokens.Import:
                        importToken(tokenStart);
                        break;
                    default:
                        break;
                }
            }
        }

        private void importToken(int tokenStart)
        {
            //var c = peek();
            while (isWhiteSpace(peek()))
            {
                //c = peek();
                advance();
            }

            if (peek() != '"')
            {
                Basil.error(line, "Unexpected character.");
                return;
            }
            else
            {
                advance();

                string path = makeString(current);

                // import
                if (!imports.Contains(path))
                {
                    imports.Add(path);

                    // scrub preprocessing from code
                    var tokenLength = current - tokenStart;
                    source = source.Remove(tokenStart, tokenLength);

                    current -= tokenLength;

                    string newSource = System.IO.File.ReadAllText(path);

                    Importer importer = new Importer(newSource);
                    string preprocessedNewSource = importer.import();

                    source = source.Insert(current, preprocessedNewSource);

                    current += preprocessedNewSource.Length + 1;
                }
                else
                {
                    // scrub preprocessing from code
                    source = source.Remove(tokenStart, current - tokenStart);
                }
            }
        }

        // return the current character in the source without consuming it
        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private char advance()
        {
            current++;
            return source[current - 1];
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
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

        private bool isWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c);
        }

        // constructs a token with a string literal
        private string makeString(int startIndex)
        {
            while (peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n') line++;
                advance();
            }

            // Unterminated string.                                 
            if (isAtEnd())
            {
                Basil.error(line, "Unterminated string.");
                return "";
            }

            // The closing ".                                       
            advance();

            // Trim the surrounding quotes.                         
            string value = source.Substring(startIndex, (current) - (startIndex + 1));
            return value;
        }

        // keyword mapping to token type
        private static readonly Dictionary<string, PreprocessingTokens> keywords = new Dictionary<string, PreprocessingTokens>
        {
            {"import",  PreprocessingTokens.Import  }
        };
    }
}
