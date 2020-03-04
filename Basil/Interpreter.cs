using System;
using System.Collections.Generic;

namespace BasilLang
{
    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        public readonly Environment globals = new Environment();
        /*private*/public Environment environment;

        /*private*/public readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            environment = globals;

            globals.define("clock", new ClockFunction());
            globals.define("ticks", new TickFunction());
            globals.define("print", new PrintFunction());

            globals.define("DEBUG_DUMPLOCALS", new Debug_DumpLocalsFunction(this));
            globals.define("DEBUG_DUMPGLOBALS", new Debug_DumpGlobalsFunction(this));
            globals.define("DEBUG_DUMPSCOPE", new Debug_DumpScopeFunction(this));
        }

        public void interpret(List<Stmt> statements)
        {
            try
            {
                // statements be empty?
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

        // evaluates a number of binary expressions
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

                // arithmetic operators
                case Token.TokenType.Minus:
                    checkNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case Token.TokenType.Plus:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string) {
                        return (string)left + (string)right;
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

        public object visitCallExpr(Expr.Call expr)
        {
            object callee = evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if (!(callee is BasilCallable)) {
                throw new RuntimeError(expr.paren,
                    "Can only call functions and classes.");
            }

            BasilCallable function = (BasilCallable)callee;
            if (arguments.Count != function.arity())
            {
                throw new RuntimeError(expr.paren, "Expected " +
                    function.arity() + " arguments but got " +
                    arguments.Count + ".");
            }

            return function.call(this, arguments);
        }

        public object visitGetExpr(Expr.Get expr)
        {
            object obj = evaluate(expr.obj);
            if (obj is BasilInstance)
            {
                return ((BasilInstance)obj).get(expr.name);
            }
            else if (obj == null) Console.WriteLine("obj is null");
            else
            {
                Console.WriteLine($"obj is type {obj.GetType()}");
            }

            throw new RuntimeError(expr.name,
                "Only instances have properties.");
        }

        // evaluates grouped (parenthesis) expressions
        public object visitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.expression);
        }

        // Converts the literal tree node into a runtime value
        public object visitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        // allows short circuiting the check, unlike binary expr
        public object visitLogicalExpr(Expr.Logical expr)
        {
            object left = evaluate(expr.left);

            if (expr.op.type == Token.TokenType.Or) {
                if (isTruthy(left)) return left;
            } else {
                if (!isTruthy(left)) return left;
            }

            return evaluate(expr.right);
        }

        public object visitSetExpr(Expr.Set expr)
        {
            object obj = evaluate(expr.obj);

            if (!(obj is BasilInstance)) {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = evaluate(expr.value);
            ((BasilInstance)obj).set(expr.name, value);
            return value;
        }

        public object visitThisExpr(Expr.This expr)
        {
            return lookUpVariable(expr.keyword, expr);
        }

        public object visitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            BasilClass superclass = (BasilClass)environment.getAt(
                distance, "super");
            //> super-find-this

            // "this" is always one level nearer than "super"'s environment.
            BasilInstance obj = (BasilInstance)environment.getAt(
                distance - 1, "this");
            //< super-find-this
            //> super-find-method

            BasilFunction method = superclass.findMethod(expr.method.lexeme);
            //> super-no-method

            if (method == null)
            {
                throw new RuntimeError(expr.method,
                    "Undefined property '" + expr.method.lexeme + "'.");
            }

            //< super-no-method
            return method.bind(obj);
            //< super-find-method
        }

        // evaluates sub-expression and applies unary operator
        public object visitUnaryExpr(Expr.Unary expr)
        {
            object right = evaluate(expr.right);

            switch (expr.op.type) {
                case Token.TokenType.Bang:
                    return !isTruthy(right);
                case Token.TokenType.Minus:
                    return -(double)right;
            }

            // Unreachable.                              
            return null;
        }

        public object visitVariableExpr(Expr.Variable expr)
        {
            return lookUpVariable(expr.name, expr);
        }

        private Object lookUpVariable(Token name, Expr expr)
        {
            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                return environment.getAt(distance, name.lexeme);
            }
            else
            {
                return globals.get(name);
            }
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
            if (left is double && right is double) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        // handle logic for dynamic typing
        // follows rule : false/nil false, all others true
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

        // send the expression back through the interpreter's visitor implementation
        private object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        private void execute(Stmt stmt)
        {
            stmt.accept(this);
        }

        public void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public void resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public object visitIfStatement(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }
            return null;
        }

        public object visitPrintStmt(Stmt.Print stmt)
        {
            object value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public object visitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = evaluate(stmt.initializer);
            }

            environment.define(stmt.name.lexeme, value);
            return null;
        }

        public object visitWhileStmt(Stmt.While stmt)
        {
            while (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }
            return null;
        }

        public object visitAssignExpr(Expr.Assign expr)
        {
            object value = evaluate(expr.value);

            if (locals.ContainsKey(expr))
            {
                int distance = locals[expr];
                environment.assignAt(distance, expr.name, value);
            }
            else
            {
                globals.assign(expr.name, value);
            }

            return value;
        }

        public object visitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object visitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.superclass != null)
            {
                superclass = evaluate(stmt.superclass);
                if (!(superclass is BasilClass)) {
                    throw new RuntimeError(stmt.superclass.name,
                        "Superclass must be a class.");
                }
            }

            environment.define(stmt.name.lexeme, null);

            if (stmt.superclass != null)
            {
                environment = new Environment(environment);
                environment.define("super", superclass);
            }

            Dictionary<string, BasilFunction> methods = new Dictionary<string, BasilFunction>();
            foreach (Stmt.Function method in stmt.methods)
            {
                /* Classes interpret-methods < Classes interpreter-method-initializer
                      LoxFunction function = new LoxFunction(method, environment);
                */
                //> interpreter-method-initializer
                BasilFunction function = new BasilFunction(method, environment,
                    method.name.lexeme.Equals("init"));
                //< interpreter-method-initializer
                methods[method.name.lexeme] = function;
            }

            BasilClass klass = new BasilClass(stmt.name.lexeme,
                (BasilClass)superclass, methods);

            if (superclass != null)
            {
                environment = environment.enclosing;
            }

            environment.assign(stmt.name, klass);
            return null;
        }

        public object visitIfStmt(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }
            return null;
        }

        public object visitFunctionStmt(Stmt.Function stmt)
        {
            BasilFunction function = new BasilFunction(stmt, environment, false);
            environment.define(stmt.name.lexeme, function);
            return null;
        }

        public object visitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = evaluate(stmt.value);

            throw new Return(value);
        }
    }
}
