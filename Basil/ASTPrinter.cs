using System.Text;

namespace BasilLang
{
    class ASTPrinter : Expr.Visitor<string>
    {
        public string print(Expr expression)
        {
            return "[EXPR]:" + expression.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return expr.name.ToString();
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
