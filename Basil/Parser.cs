using System;
using System.Collections.Generic;

namespace BasilLang
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(statement());
            }

            return statements;
        }

        private Expr expression()
        {
            return equality();
        }

        private Stmt statement()
        {
            if (match(Token.TokenType.Print)) return printStatement();

            return expressionStatement();
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(Token.TokenType.Semicolon, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(Token.TokenType.Semicolon, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr equality()
        {
            Expr expr = comparison();

            while (match(Token.TokenType.BangEqual, Token.TokenType.EqualEqual))
            {
                Token op = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr comparison()
        {
            Expr expr = addition();

            while (match(Token.TokenType.Greater, Token.TokenType.GreaterEqual, Token.TokenType.Less, Token.TokenType.LessEqual))
            {
                Token op = previous();
                Expr right = addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr addition()
        {
            Expr expr = multiplication();

            while (match(Token.TokenType.Minus, Token.TokenType.Plus))
            {
                Token op = previous();
                Expr right = multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr multiplication()
        {
            Expr expr = unary();

            while (match(Token.TokenType.Slash, Token.TokenType.Star))
            {
                Token op = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if (match(Token.TokenType.Bang, Token.TokenType.Minus))
            {
                Token op = previous();
                Expr right = unary();
                return new Expr.Unary(op, right);
            }

            return primary();
        }

        private Expr primary()
        {
            if (match(Token.TokenType.False)) return new Expr.Literal(false);
            if (match(Token.TokenType.True)) return new Expr.Literal(true);
            if (match(Token.TokenType.Nil)) return new Expr.Literal(null);

            if (match(Token.TokenType.Number, Token.TokenType.String))
            {
                return new Expr.Literal(previous().literal);
            }

            if (match(Token.TokenType.LeftParenthesis))
            {
                Expr expr = expression();
                consume(Token.TokenType.RightParenthesis, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        private Token consume(Token.TokenType type, string message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        private ParseError error(Token token, String message)
        {
            Basil.error(token, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().type == Token.TokenType.Semicolon) return;

                switch (peek().type)
                {
                    case Token.TokenType.Class:
                    case Token.TokenType.Fun:
                    case Token.TokenType.Var:
                    case Token.TokenType.For:
                    case Token.TokenType.If:
                    case Token.TokenType.While:
                    case Token.TokenType.Print:
                    case Token.TokenType.Return:
                        return;
                }

                advance();
            }
        }

        private bool match(params Token.TokenType[] types)
        {
            foreach (var type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private bool check(Token.TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private bool isAtEnd()
        {
            return peek().type == Token.TokenType.EOF;
        }

        private Token peek()
        {
            return tokens[current];
        }

        private Token previous()
        {
            return tokens[current - 1];
        }
    }
}
