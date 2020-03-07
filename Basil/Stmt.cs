using System.Collections.Generic;

namespace BasilLang
{
    public abstract class Stmt
    {
        public interface IStmtVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitBreakStmt(Break stmt);
            R VisitClassStmt(Class stmt);
            R VisitContinueStmt(Continue stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
        }

        // Nested Stmt classes here...            

        public abstract R Accept<R>(IStmtVisitor<R> visitor);

        public class Block : Stmt
        {
            public Block(List<Stmt> statement)
            {
                this.Statements = statement;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public readonly List<Stmt> Statements;
        }

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public readonly Expr expression;
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public readonly Expr expression;
        }

        public class Var : Stmt
        {
            public Var(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public readonly Token name;
            public readonly Expr initializer;
        }

        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;
        }

        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt body;
        }

        public class Function : Stmt
        {
            public Function(Token name, List<Token> parameters, List<Stmt> body)
            {
                this.name = name;
                this.parameters = parameters;
                this.body = body;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public readonly Token name;
            public readonly List<Token> parameters;
            public readonly List<Stmt> body;
        }

        public class Return : Stmt
        {
            public Return(Token keyword, Expr value)
            {
                this.keyword = keyword;
                this.value = value;
            }

            public override R Accept<R>(IStmtVisitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public readonly Token keyword;
            public readonly Expr value;
        }

        public class Class : Stmt
        {
            public Token Name { get; }
            public Expr Superclass { get; }
            public List<Stmt.Function> Methods { get; }
            public Class(Token name, Expr superclass, List<Stmt.Function> methods)
            {
                Name = name;
                Superclass = superclass;
                Methods = methods;
            }

            public override T Accept<T>(IStmtVisitor<T> visitor)
            {
                return visitor.VisitClassStmt(this);
            }
        }

        public class Break : Stmt
        {
            public override T Accept<T>(IStmtVisitor<T> visitor)
            {
                return visitor.VisitBreakStmt(this);
            }
        }

        public class Continue : Stmt
        {
            public override T Accept<T>(IStmtVisitor<T> visitor)
            {
                return visitor.VisitContinueStmt(this);
            }
        }
    }
}
