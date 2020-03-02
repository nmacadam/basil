using System;
using System.Collections.Generic;

namespace BasilLang
{
    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        public void interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Basil.runtimeError(error);
            }
        }

        public object visitBinaryExpr(Expr.Binary expr)
        {
            object left = evaluate(expr.left);
            object right = evaluate(expr.right);

            switch (expr.op.type) {
                case Token.TokenType.Greater:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case Token.TokenType.GreaterEqual:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case Token.TokenType.Less:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case Token.TokenType.LessEqual:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case Token.TokenType.Minus:
                    checkNumberOperands(expr.op, left, right);
                    return (float)left - (float)right;
                case Token.TokenType.Plus:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string) {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.op,
                        "Operands must be two numbers or two strings.");

                case Token.TokenType.Slash:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;       
                case Token.TokenType.Star:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case Token.TokenType.BangEqual: return !isEqual(left, right);
                case Token.TokenType.EqualEqual: return isEqual(left, right);
            }

            // Unreachable.                                
            return null;
        }

        public object visitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.expression);
        }

        public object visitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object visitUnaryExpr(Expr.Unary expr)
        {
            object right = evaluate(expr.right);

            switch (expr.op.type) {
                case Token.TokenType.Bang:
                    return !isTruthy(right);
                case Token.TokenType.Minus:
                    return -(float)right;
            }

            // Unreachable.                              
            return null;
        }

        private void checkNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            Console.WriteLine("TEMP_ERROR::OPERAND MUST BE A NUMBER.");
            //throw new Exception();
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void checkNumberOperands(Token op,
                                   Object left, Object right)
        {
            if (left is Double && right is Double) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private bool isTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        private bool isEqual(object a, object b)
        {
            // nil is only equal to nil.               
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string stringify(object obj)
        {
            if (obj == null) return "nil";

            // Hack. Work around Java adding ".0" to integer-valued doubles.
            if (obj is double) {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        private object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        private void execute(Stmt stmt)
        {
            stmt.accept(this);
        }

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public object visitPrintStmt(Stmt.Print stmt)
        {
            object value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }
    }
}
