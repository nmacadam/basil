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

        public string Process()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }
            return source;
        }

        private void ScanToken()
        {
            char c = Advance();
            if (IsAlpha(c))
            {
                Identifier();
            }
        }

        // constructs a token for a keyword if valid
        private void Identifier()
        {
            int tokenStart = current - 1;

            while (IsAlphaNumeric(Peek()))
            {
                Advance();
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
                        ImportToken(tokenStart);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ImportToken(int tokenStart)
        {
            //var c = peek();
            while (IsWhiteSpace(Peek()))
            {
                //c = peek();
                Advance();
            }

            if (Peek() != '"')
            {
                Basil.Error(line, "Unexpected character.");
                return;
            }
            else
            {
                Advance();

                string path = MakeString(current);

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
                    string preprocessedNewSource = importer.Process();

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
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
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

        private bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c);
        }

        // constructs a token with a string literal
        private string MakeString(int startIndex)
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            // Unterminated string.                                 
            if (IsAtEnd())
            {
                Basil.Error(line, "Unterminated string.");
                return "";
            }

            // The closing ".                                       
            Advance();

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
