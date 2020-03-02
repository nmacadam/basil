namespace BasilLang
{
    public abstract class Expr
    {
        public interface Visitor<R>
        {
            //R visitAssignExpr(Assign expr);
            R visitBinaryExpr(Binary expr);
            //R visitCallExpr(Call expr);
            //R visitGetExpr(Get expr);
            R visitGroupingExpr(Grouping expr);
            R visitLiteralExpr(Literal expr);
            //R visitLogicalExpr(Logical expr);
            //R visitSetExpr(Set expr);
            //R visitSuperExpr(Super expr);
            //R visitThisExpr(This expr);
            R visitUnaryExpr(Unary expr);
            //R visitVariableExpr(Variable expr);
        }

        public abstract R accept<R>(Visitor<R> visitor);

        // static?
        public class Binary : Expr
        {
            public Binary(Expr left, Token op, Expr right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitBinaryExpr(this);
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

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitGroupingExpr(this);
            }

            public readonly Expr expression;
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                this.value = value;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitLiteralExpr(this);
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

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitUnaryExpr(this);
            }

            public readonly Token op;
            public readonly Expr right;
        }
    }
}
