namespace BasilLang
{
    public class Token
    {
        public enum TokenType
        {
            // Single-character tokens
            LeftParenthesis, RightParenthesis, LeftBrace, RightBrace,
            Comma, Dot, Minus, Plus, Semicolon, Slash, Star,

            // One or two character tokens
            Bang, BangEqual, Equal, EqualEqual, Greater, GreaterEqual, Less, LessEqual,

            // Literals
            Identifier, String, Number,

            // Keywords
            And, Class, Else, False, Fun, For, If, Nil, Or, Print, Return, Super, This, True, Var, While, EOF
        }

        public readonly TokenType type;
        public readonly string lexeme;
        public readonly object literal;
        public readonly int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }
}
