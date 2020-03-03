using System.Text;

namespace BasilLang
{
    class ASTPrinter : Expr.Visitor<string>
    {
        public string print(Expr expression)
        {
            return "[EXPR]:" + expression.accept(this);
        }

        public string visitAssignExpr(Expr.Assign expr)
        {
            throw new System.NotImplementedException();
        }

        public string visitBinaryExpr(Expr.Binary expr)
        {
            return parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string visitCallExpr(Expr.Call expr)
        {
            throw new System.NotImplementedException();
        }

        public string visitGroupingExpr(Expr.Grouping expr)
        {
            return parenthesize("group", expr.expression);
        }

        public string visitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string visitLogicalExpr(Expr.Logical expr)
        {
            throw new System.NotImplementedException();
        }

        public string visitUnaryExpr(Expr.Unary expr)
        {
            return parenthesize(expr.op.lexeme, expr.right);
        }

        public string visitVariableExpr(Expr.Variable expr)
        {
            return expr.name.ToString();
        }

        private string parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
