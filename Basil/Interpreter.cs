using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BasilLang
{
    public class Interpreter : Expr.IExprVisitor<object>, Stmt.IStmtVisitor<object>
    {
        public readonly Environment globals = new Environment();
        private Environment environment;
        
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            environment = globals;
            
            // Define native functions
            var nativeFunctions = 
                typeof(NativeFunctions.NativeCallable).Assembly.GetTypes().Where
                (
                    t => t.IsClass &&
                    typeof(NativeFunctions.NativeCallable).IsAssignableFrom(t) && 
                    (t.GetCustomAttribute(typeof(NativeFunction), false) != null)
                );

            foreach (var item in nativeFunctions)
            {
                var method = (NativeFunctions.NativeCallable)Activator.CreateInstance(item);
                globals.Define(method.MethodName, method);
            }
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                // statements be empty?
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Basil.RuntimeError(error);
            }
        }

        // evaluates a number of binary expressions
        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.type) {
                case Token.TokenType.Greater:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case Token.TokenType.GreaterEqual:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case Token.TokenType.Less:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case Token.TokenType.LessEqual:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;

                // arithmetic operators
                case Token.TokenType.Minus:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case Token.TokenType.Plus:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string) {
                        return (string)left + (string)right;
                    }

                    // try to ToString whatever the value is
                    if (left is string && !(right is string) && right != null)
                    {
                        return (string)left + right.ToString();
                    }
                    else if (!(left is string) && left != null && right is string)
                    {
                        return left.ToString() + (string)right;
                    }

                    if (left is double && right is double)
                    { }
                    else Console.WriteLine("Not Double");

                    if (left is string && right is string)
                    { }
                    else Console.WriteLine("Not String");

                    Console.WriteLine(left.GetType());

                    // THIS IS THROWING ERROR
                    Console.WriteLine($"{expr.op}, left:{left}, right:{right}");
                    throw new RuntimeError(expr.op,
                        "Operands must be two numbers or two strings.");

                case Token.TokenType.Slash:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;
                case Token.TokenType.Star:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case Token.TokenType.Percent:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left % (double)right;
                case Token.TokenType.BangEqual: return !IsEqual(left, right);
                case Token.TokenType.EqualEqual: return IsEqual(left, right);
            }

            // Unreachable.                                
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ICallable)) {
                throw new RuntimeError(expr.paren,
                    "Can only call functions and classes.");
            }

            ICallable function = (ICallable)callee;
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, "Expected " +
                    function.Arity() + " arguments but got " +
                    arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        // evaluates grouped (parenthesis) expressions
        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        // Converts the literal tree node into a runtime value
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        // allows short circuiting the check, unlike binary expr
        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == Token.TokenType.Or) {
                if (IsTruthy(left)) return left;
            } else {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object objekt = Evaluate(expr.Objekt);
            if (objekt is BasilInstance)
            {
                return ((BasilInstance)objekt).Get(expr.Name);
            }
            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            object objekt = Evaluate(expr.Objekt);
            if (!(objekt is BasilInstance))
            {
                throw new RuntimeError(expr.Name, "Only instances have fields.");
            }

            object value = Evaluate(expr.Value);
            ((BasilInstance)objekt).Set(expr.Name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            BasilClass superclass = (BasilClass)environment.GetAt(distance, "super");
            // "this" is always one level nearer than "super"'s environment.
            BasilInstance objekt = (BasilInstance)environment.GetAt(distance - 1, "this");
            BasilFunction method = superclass.FindMethod(objekt, expr.Method.lexeme);
            if (method == null)
            {
                throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.lexeme}'.");
            }
            return method;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.Keyword, expr);
        }

        // evaluates sub-expression and applies unary operator
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type) {
                case Token.TokenType.Bang:
                    return !IsTruthy(right);
                case Token.TokenType.Minus:
                    return -(double)right;
            }

            // Unreachable.                              
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }
            return globals.Get(name);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            Console.WriteLine("TEMP_ERROR::OPERAND MUST BE A NUMBER.");
            //throw new Exception();
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op,
                                   Object left, Object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        // handle logic for dynamic typing
        // follows rule : false/nil false, all others true
        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            // nil is only equal to nil.               
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string Stringify(object obj)
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

        // send the expression back through the interpreter's visitor implementation
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            environment.Define(stmt.Name.lexeme, null);
            object superclass = null;
            if (stmt.Superclass != null)
            {
                superclass = Evaluate(stmt.Superclass);
                if (!(superclass is BasilClass))
                {
                    throw new RuntimeError(stmt.Name, "Superclass must be a class.");
                }
                environment = new Environment(environment);
                environment.Define("super", superclass);
            }
            Dictionary<string, BasilFunction> methods = new Dictionary<string, BasilFunction>();
            foreach (var method in stmt.Methods)
            {
                BasilFunction function = new BasilFunction(method, environment, method.name.lexeme.Equals("init"));
                methods.Add(method.name.lexeme, function);
            }
            BasilClass klass = new BasilClass(stmt.Name.lexeme, (BasilClass)superclass, methods);
            if (superclass != null) environment = environment.Enclosing;
            environment.Assign(stmt.Name, klass);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        //public object VisitIfStatement(Stmt.If stmt)
        //{
        //    if (IsTruthy(Evaluate(stmt.condition)))
        //    {
        //        Execute(stmt.thenBranch);
        //    }
        //    else if (stmt.elseBranch != null)
        //    {
        //        Execute(stmt.elseBranch);
        //    }
        //    return null;
        //}

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            if (locals.TryGetValue(expr, out int distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }
            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(environment));
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            BasilFunction function = new BasilFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }
    }
}
