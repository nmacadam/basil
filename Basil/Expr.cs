using System.Collections.Generic;

namespace BasilLang
{
    public abstract class Expr
    {
        // allows defining methods for handling different expressions without modifying their code
        public interface IExprVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitCallExpr(Call expr);
            R VisitGetExpr(Get expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitLogicalExpr(Logical expr);
            R VisitSetExpr(Set expr);
            R VisitSuperExpr(Super expr);
            R VisitThisExpr(This expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
        }

        public abstract R Accept<R>(IExprVisitor<R> visitor);

        
        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public readonly Token name;
            public readonly Expr value;
        }

        // defines a binary expression; a left expr, an operator, and a right expr
        public class Binary : Expr
        {
            public Binary(Expr left, Token op, Expr right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public readonly Expr left;
            public readonly Token op;
            public readonly Expr right;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public readonly Expr expression;
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                this.value = value;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public readonly object value;
        }

        public class Unary : Expr
        {
            public Unary(Token op, Expr right)
            {
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public readonly Token op;
            public readonly Expr right;
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public readonly Token name;
        }

        // e.g. and or keywords
        public class Logical : Expr
        {
            public Logical(Expr left, Token op, Expr right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public readonly Expr left;
            public readonly Token op;
            public readonly Expr right;
        }

        public class Call : Expr
        {
            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public override R Accept<R>(IExprVisitor<R> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public readonly Expr callee;
            public readonly Token paren;
            public readonly List<Expr> arguments;
        }

        public class Get : Expr
        {
            public Expr Objekt { get; }
            public Token Name { get; }
            public Get(Expr objekt, Token name)
            {
                Objekt = objekt;
                Name = name;
            }

            public override T Accept<T>(IExprVisitor<T> visitor)
            {
                return visitor.VisitGetExpr(this);
            }
        }

        public class Set : Expr
        {
            public Expr Objekt { get; }
            public Token Name { get; }
            public Expr Value { get; }
            public Set(Expr objekt, Token name, Expr value)
            {
                Objekt = objekt;
                Name = name;
                Value = value;
            }

            public override T Accept<T>(IExprVisitor<T> visitor)
            {
                return visitor.VisitSetExpr(this);
            }
        }

        public class Super : Expr
        {
            public Token Keyword { get; }
            public Token Method { get; }
            public Super(Token keyword, Token method)
            {
                Keyword = keyword;
                Method = method;
            }

            public override T Accept<T>(IExprVisitor<T> visitor)
            {
                return visitor.VisitSuperExpr(this);
            }
        }

        public class This : Expr
        {
            public Token Keyword { get; }
            public This(Token keyword)
            {
                Keyword = keyword;
            }

            public override T Accept<T>(IExprVisitor<T> visitor)
            {
                return visitor.VisitThisExpr(this);
            }
        }
    }
}
