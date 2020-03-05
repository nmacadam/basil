using System;
using System.Collections.Generic;

namespace BasilLang
{
    // consume sequence and translate each rule to C#
    // this is the first step in actually converting Basil -> C#

    // each expression built must cascade from a low level of precedence
    // to the highest one, i.e. we have to traverse a tree of expression building
    // until we can reduce each piece into it's atomic parts
    // the rules for how we branch has to follow an order of precedence,
    // (e.g. PEMDAS)
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
                statements.Add(declaration());
                //statements.Add(statement());
            }

            return statements;
        }

        private Stmt declaration()
        {
            try
            {
                if (match(Token.TokenType.Fun)) return function("function");
                if (match(Token.TokenType.Var)) return varDeclaration();

                return statement();
            }
            catch (ParseError error)
            {
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            Token name = consume(Token.TokenType.Identifier, "Expect variable name.");

            Expr initializer = null;
            if (match(Token.TokenType.Equal))
            {
                initializer = expression();
            }

            consume(Token.TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt whileStatement()
        {
            consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'while'.");
            Expr condition = expression();
            consume(Token.TokenType.RightParenthesis, "Expect ')' after condition.");
            Stmt body = statement();

            return new Stmt.While(condition, body);
        }

        private Expr expression()
        {
            return assignment();
        }

        // parse individual statements
        private Stmt statement()
        {
            if (match(Token.TokenType.For)) return forStatement();
            if (match(Token.TokenType.If)) return ifStatement();
            if (match(Token.TokenType.Print)) return printStatement();
            if (match(Token.TokenType.Return)) return returnStatement();
            if (match(Token.TokenType.While)) return whileStatement();
            if (match(Token.TokenType.LeftBrace)) return new Stmt.Block(block());

            return expressionStatement();
        }

        // our first desugaring
        private Stmt forStatement()
        {
            consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'for'.");

            Stmt initializer;
            if (match(Token.TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (match(Token.TokenType.Var))
            {
                initializer = varDeclaration();
            }
            else
            {
                initializer = expressionStatement();
            }

            Expr condition = null;
            if (!check(Token.TokenType.Semicolon))
            {
                condition = expression();
            }
            consume(Token.TokenType.Semicolon, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!check(Token.TokenType.RightParenthesis))
            {
                increment = expression();
            }
            consume(Token.TokenType.RightParenthesis, "Expect ')' after for clauses.");

            Stmt body = statement();

            if (increment != null)
            {
                body = new Stmt.Block(
                    new List<Stmt>()
                    {
                        body,
                        new Stmt.Expression(increment)
                    }
                );
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(
                    new List<Stmt>()
                    {
                        initializer,
                        body
                    }
                );
            }

            return body;
        }

        private Stmt ifStatement()
        {
            consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(Token.TokenType.RightParenthesis, "Expect ')' after if condition.");

            Stmt thenBranch = statement();
            Stmt elseBranch = null;
            if (match(Token.TokenType.Else))
            {
                elseBranch = statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(Token.TokenType.Semicolon, "Expect ';' after value.");
            //Console.WriteLine(Basil.printer.print(value));
            return new Stmt.Print(value);
        }

        private Stmt returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            if (!check(Token.TokenType.Semicolon))
            {
                value = expression();
            }

            consume(Token.TokenType.Semicolon, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(Token.TokenType.Semicolon, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function function(String kind)
        {
            Token name = consume(Token.TokenType.Identifier, "Expect " + kind + " name.");
            consume(Token.TokenType.LeftParenthesis, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!check(Token.TokenType.RightParenthesis))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        error(peek(), "Cannot have more than 255 parameters.");
                    }

                    parameters.Add(consume(Token.TokenType.Identifier, "Expect parameter name."));
                } while (match(Token.TokenType.Comma));
            }
            consume(Token.TokenType.RightParenthesis, "Expect ')' after parameters.");

            consume(Token.TokenType.LeftBrace, "Expect '{' before " + kind + " body.");
            List<Stmt> body = block();
            return new Stmt.Function(name, parameters, body);
        }

        private List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!check(Token.TokenType.RightBrace) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(Token.TokenType.RightBrace, "Expect '}' after block.");
            return statements;
        }

        private Expr assignment()
        {
            Expr expr = or();

            if (match(Token.TokenType.Equal))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr is Expr.Variable) {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr or()
        {
            Expr expr = and();

            while (match(Token.TokenType.Or))
            {
                Token op = previous();
                Expr right = and();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while (match(Token.TokenType.And))
            {
                Token op = previous();
                Expr right = equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        // generates an expr for equality check
        private Expr equality()
        {
            Expr expr = comparison();

            // exit when we find an equality operator
            while (match(Token.TokenType.BangEqual, Token.TokenType.EqualEqual))
            {
                Token op = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for comparison
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

        // generates an expr for addition
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

        // generates an expr for multiplication
        private Expr multiplication()
        {
            Expr expr = unary();

            while (match(Token.TokenType.Slash, Token.TokenType.Star, Token.TokenType.Percent))
            {
                Token op = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for unary operators (e.g. ! and -)
        private Expr unary()
        {
            if (match(Token.TokenType.Bang, Token.TokenType.Minus))
            {
                Token op = previous();
                Expr right = unary();
                return new Expr.Unary(op, right);
            }

            return call();
        }

        private Expr finishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!check(Token.TokenType.RightParenthesis))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        error(peek(), "Cannot have more than 255 arguments.");
                    }
                    arguments.Add(expression());
                } while (match(Token.TokenType.Comma));
            }

            Token paren = consume(Token.TokenType.RightParenthesis, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr call()
        {
            Expr expr = primary();

            while (true)
            {
                if (match(Token.TokenType.LeftParenthesis))
                {
                    expr = finishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        // generates an expr for primary expressions (e.g. Number, String, true/false)
        // this is the highest level of precedence
        private Expr primary()
        {
            if (match(Token.TokenType.False)) return new Expr.Literal(false);
            if (match(Token.TokenType.True)) return new Expr.Literal(true);
            if (match(Token.TokenType.Nil)) return new Expr.Literal(null);

            if (match(Token.TokenType.Number, Token.TokenType.String))
            {
                return new Expr.Literal(previous().literal);
            }

            if (match(Token.TokenType.Identifier))
            {
                return new Expr.Variable(previous());
            }

            if (match(Token.TokenType.LeftParenthesis))
            {
                Expr expr = expression();
                consume(Token.TokenType.RightParenthesis, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        // checks if the next token is of the expected type, and advances
        private Token consume(Token.TokenType type, string message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        // reports an error about a token
        private ParseError error(Token token, string message)
        {
            Basil.error(token, message);
            return new ParseError();
        }

        // discard tokens until the beginning of the next statement in the case that a statement fails
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

        // checks if the current token is a given type, and consumes it if true
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

        // returns true if the current token is of the given type
        private bool check(Token.TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        // consumes the current token and returns it
        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        // returns if the current token is the end-of-file
        private bool isAtEnd()
        {
            return peek().type == Token.TokenType.EOF;
        }

        // returns the current token
        private Token peek()
        {
            return tokens[current];
        }

        // returns the previous token
        private Token previous()
        {
            return tokens[current - 1];
        }
    }
}
