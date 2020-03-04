using System.Collections.Generic;

namespace BasilLang
{
    class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private enum FunctionType
        {
            None,
            Function,
            Initializer,
            Method
        }

        private enum ClassType
        {
            None,
            Class,
            Subclass
        }

        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.None;
        private ClassType currentClass = ClassType.None;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                resolve(statement);
            }
        }

        private void resolve(Stmt stmt)
        {
            stmt.accept(this);
        }

        private void resolve(Expr expr)
        {
            expr.accept(this);
        }

        private void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void endScope()
        {
            scopes.Pop();
        }

        private void declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Basil.error(name,
                    "Variable with this name already declared in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private void define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void resolveLocal(Expr expr, Token name)
        {
            var temp = scopes.ToArray();

            // array might be in wrong order
            //for (int i = 0; i < temp.Length; i++)
            //{
            //    if (temp[i].ContainsKey(name.lexeme))
            //    {
            //        interpreter.resolve(expr, temp.Length - 1 - i);
            //        return;
            //    }
            //}
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                if (temp[i].ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, temp.Length - 1 - i);
                    return;
                }
            }

            //for (int i = scopes.Count - 1; i >= 0; i--)
            //{
            //    if (scopes.get(i).containsKey(name.lexeme))
            //    {
            //        interpreter.resolve(expr, scopes.Count - 1 - i);
            //        return;
            //    }
            //}

            // Not found. Assume it is global.                   
        }

        private void resolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            beginScope();
            foreach (Token param in function.parameters)
            {
                declare(param);
                define(param);
            }
            resolve(function.body);
            endScope();
            currentFunction = enclosingFunction;
        }

        // Assignment visiting

        public object visitAssignExpr(Expr.Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitBinaryExpr(Expr.Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitBlockStmt(Stmt.Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }

        //public object visitClassStmt(Stmt.Class stmt)
        //{
        //    ClassType enclosingClass = currentClass;
        //    currentClass = ClassType.Class;

        //    declare(stmt.name);
        //    define(stmt.name);

        //    if (stmt.superclass != null && stmt.name.lexeme.Equals(stmt.superclass.name.lexeme))
        //    {
        //        Basil.error(stmt.superclass.name,
        //            "A class cannot inherit from itself.");
        //    }

        //    if (stmt.superclass != null)
        //    {
        //        currentClass = ClassType.Subclass;
        //        // set-current-subclass
        //        resolve(stmt.superclass);
        //    }

        //    if (stmt.superclass != null)
        //    {
        //        beginScope();
        //        scopes.Peek()["super"] = true;
        //    }

        //    beginScope();
        //    scopes.Peek()["this"] = true;

        //    foreach (Stmt.Function method in stmt.methods)
        //    {
        //        FunctionType declaration = FunctionType.Method;
        //        //> resolver-initializer-type
        //        if (method.name.lexeme.Equals("init"))
        //        {
        //            declaration = FunctionType.Initializer;
        //        }

        //        //< resolver-initializer-type
        //        resolveFunction(method, declaration); // [local]
        //    }

        //    endScope();
        //    if (stmt.superclass != null) endScope();
        //    currentClass = enclosingClass;

        //    return null;
        //}

        public object visitCallExpr(Expr.Call expr)
        {
            resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                resolve(argument);
            }

            return null;
        }

        //public object visitGetExpr(Expr.Get expr)
        //{
        //    resolve(expr.obj);
        //    return null;
        //}

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitFunctionStmt(Stmt.Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.Function);
            return null;
        }

        public object visitGroupingExpr(Expr.Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public object visitIfStmt(Stmt.If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) resolve(stmt.elseBranch);
            return null;
        }

        public object visitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object visitLogicalExpr(Expr.Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        //public object visitSetExpr(Expr.Set expr)
        //{
        //    resolve(expr.value);
        //    resolve(expr.obj);
        //    return null;
        //}

        //public object visitSuperExpr(Expr.Super expr)
        //{
        //    if (currentClass == ClassType.None)
        //    {
        //        Basil.error(expr.keyword,
        //            "Cannot use 'super' outside of a class.");
        //    }
        //    else if (currentClass != ClassType.Subclass)
        //    {
        //        Basil.error(expr.keyword,
        //            "Cannot use 'super' in a class with no superclass.");
        //    }

        //    resolveLocal(expr, expr.keyword);
        //    return null;
        //}

        //public object visitThisExpr(Expr.This expr)
        //{
        //    if (currentClass == ClassType.None)
        //    {
        //        Basil.error(expr.keyword,
        //            "Cannot use 'this' outside of a class.");
        //        return null;
        //    }

        //    resolveLocal(expr, expr.keyword);
        //    return null;
        //}

        // Statement visiting

        public object visitPrintStmt(Stmt.Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.None)
            {
                Basil.error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.Initializer)
                {
                    Basil.error(stmt.keyword,
                        "Cannot return a value from an initializer.");
                }

                resolve(stmt.value);
            }

            return null;
        }

        public object visitUnaryExpr(Expr.Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object visitVariableExpr(Expr.Variable expr)
        {
            // could be a problem
            if (scopes.Count > 0)
            {
                if (scopes.Peek().ContainsKey(expr.name.lexeme))
                {
                    if (scopes.Peek()[expr.name.lexeme] == false)
                    {
                        Basil.error(expr.name,
                            "Cannot read local variable in its own initializer.");
                    }
                }
                else Basil.error(expr.name, "Variable not found");
            }


            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitVarStmt(Stmt.Var stmt)
        {
            declare(stmt.name);
            if (stmt.initializer != null)
            {
                resolve(stmt.initializer);
            }
            define(stmt.name);
            return null;
        }

        public object visitWhileStmt(Stmt.While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }
    }
}