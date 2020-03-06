using System.Collections.Generic;

namespace BasilLang
{
    internal class Resolver : Expr.IExprVisitor<object>, Stmt.IStmtVisitor<object>
    {
        private enum ClassType
        {
            None,
            Class,
            Subclass
        }
        private enum FunctionType
        {
            None,
            Function,
            Initializer,
            Method
        }
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private ClassType currentClass = ClassType.None;
        private FunctionType currentFunction = FunctionType.None;


        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (var param in function.parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            int depth = 0;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, depth);
                    return;
                }
                depth++;
            }
            // not found, assume global
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;
            if (scopes.Peek().ContainsKey(name.lexeme))
            {
                Basil.Error(name, "Variable with this name is already declared in this scope.");
            }
            scopes.Peek()[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach (var arg in expr.arguments)
            {
                Resolve(arg);
            }
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Objekt);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Objekt);
            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.None)
            {
                Basil.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.Subclass)
            {
                Basil.Error(expr.Keyword, "Cannot use 'super' in a class with no superclass.");
            }
            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.None)
            {
                Basil.Error(expr.Keyword, "Cannot use 'this' outside of a class.");
                return null;
            }
            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count != 0 && scopes.Peek().ContainsKey(expr.name.lexeme) && scopes.Peek()[expr.name.lexeme] == false)
            {
                Basil.Error(expr.name, "Cannot read local variable in its own initializer.");
            }
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.Class;
            if (stmt.Superclass != null)
            {
                currentClass = ClassType.Subclass;
                Resolve(stmt.Superclass);
                BeginScope();
                scopes.Peek().Add("super", true);
            }
            BeginScope();
            scopes.Peek().Add("this", true);
            foreach (var method in stmt.Methods)
            {
                FunctionType declaration = FunctionType.Method;
                if (method.name.lexeme.Equals("init"))
                {
                    declaration = FunctionType.Initializer;
                }
                ResolveFunction(method, declaration);
            }
            EndScope();
            if (stmt.Superclass != null) EndScope();
            currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            ResolveFunction(stmt, FunctionType.Function);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.None)
            {
                Basil.Error(stmt.keyword, "Cannot return from top level code.");
            }
            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.Initializer)
                {
                    Basil.Error(stmt.keyword, "Cannot return a value from an initializer.");
                }
                Resolve(stmt.value);
            }
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }
    }
}
