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

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
                //statements.Add(statement());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(Token.TokenType.Fun)) return Function("function");
                if (Match(Token.TokenType.Var)) return VarDeclaration();

                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(Token.TokenType.Identifier, "Expect variable name.");

            Expr initializer = null;
            if (Match(Token.TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(Token.TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt WhileStatement()
        {
            Consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(Token.TokenType.RightParenthesis, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        // parse individual statements
        private Stmt Statement()
        {
            if (Match(Token.TokenType.For)) return ForStatement();
            if (Match(Token.TokenType.If)) return IfStatement();
            if (Match(Token.TokenType.Print)) return PrintStatement();
            if (Match(Token.TokenType.Return)) return ReturnStatement();
            if (Match(Token.TokenType.While)) return WhileStatement();
            if (Match(Token.TokenType.LeftBrace)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        // our first desugaring
        private Stmt ForStatement()
        {
            Consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(Token.TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Match(Token.TokenType.Var))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(Token.TokenType.Semicolon))
            {
                condition = Expression();
            }
            Consume(Token.TokenType.Semicolon, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(Token.TokenType.RightParenthesis))
            {
                increment = Expression();
            }
            Consume(Token.TokenType.RightParenthesis, "Expect ')' after for clauses.");

            Stmt body = Statement();

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

        private Stmt IfStatement()
        {
            Consume(Token.TokenType.LeftParenthesis, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(Token.TokenType.RightParenthesis, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(Token.TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(Token.TokenType.Semicolon, "Expect ';' after value.");
            //Console.WriteLine(Basil.printer.print(value));
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(Token.TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(Token.TokenType.Semicolon, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(Token.TokenType.Semicolon, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function Function(String kind)
        {
            Token name = Consume(Token.TokenType.Identifier, "Expect " + kind + " name.");
            Consume(Token.TokenType.LeftParenthesis, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!Check(Token.TokenType.RightParenthesis))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    parameters.Add(Consume(Token.TokenType.Identifier, "Expect parameter name."));
                } while (Match(Token.TokenType.Comma));
            }
            Consume(Token.TokenType.RightParenthesis, "Expect ')' after parameters.");

            Consume(Token.TokenType.LeftBrace, "Expect '{' before " + kind + " body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(Token.TokenType.RightBrace) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(Token.TokenType.RightBrace, "Expect '}' after block.");
            return statements;
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(Token.TokenType.Equal))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable) {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(Token.TokenType.Or))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(Token.TokenType.And))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        // generates an expr for equality check
        private Expr Equality()
        {
            Expr expr = Comparison();

            // exit when we find an equality operator
            while (Match(Token.TokenType.BangEqual, Token.TokenType.EqualEqual))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for comparison
        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(Token.TokenType.Greater, Token.TokenType.GreaterEqual, Token.TokenType.Less, Token.TokenType.LessEqual))
            {
                Token op = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for addition
        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(Token.TokenType.Minus, Token.TokenType.Plus))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for multiplication
        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(Token.TokenType.Slash, Token.TokenType.Star, Token.TokenType.Percent))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        // generates an expr for unary operators (e.g. ! and -)
        private Expr Unary()
        {
            if (Match(Token.TokenType.Bang, Token.TokenType.Minus))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(Token.TokenType.RightParenthesis))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(Token.TokenType.Comma));
            }

            Token paren = Consume(Token.TokenType.RightParenthesis, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(Token.TokenType.LeftParenthesis))
                {
                    expr = FinishCall(expr);
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
        private Expr Primary()
        {
            if (Match(Token.TokenType.False)) return new Expr.Literal(false);
            if (Match(Token.TokenType.True)) return new Expr.Literal(true);
            if (Match(Token.TokenType.Nil)) return new Expr.Literal(null);

            if (Match(Token.TokenType.Number, Token.TokenType.String))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(Token.TokenType.Identifier))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(Token.TokenType.LeftParenthesis))
            {
                Expr expr = Expression();
                Consume(Token.TokenType.RightParenthesis, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        // Helpers

        // checks if the next token is of the expected type, and advances
        private Token Consume(Token.TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        // reports an error about a token
        private ParseError Error(Token token, string message)
        {
            Basil.Error(token, message);
            return new ParseError();
        }

        // discard tokens until the beginning of the next statement in the case that a statement fails
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == Token.TokenType.Semicolon) return;

                switch (Peek().type)
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

                Advance();
            }
        }

        // checks if the current token is a given type, and consumes it if true
        private bool Match(params Token.TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        // returns true if the current token is of the given type
        private bool Check(Token.TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        // consumes the current token and returns it
        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        // returns if the current token is the end-of-file
        private bool IsAtEnd()
        {
            return Peek().type == Token.TokenType.EOF;
        }

        // returns the current token
        private Token Peek()
        {
            return tokens[current];
        }

        // returns the previous token
        private Token Previous()
        {
            return tokens[current - 1];
        }
    }
}
