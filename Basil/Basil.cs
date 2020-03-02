using System;
using System.Collections.Generic;

namespace BasilLang
{
    class Basil
    {
        static readonly Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            //Expression expression = new Expression.Binary(
            //    new Expression.Unary(
            //    new Token(Token.TokenType.Minus, "-", null, 1),
            //    new Expression.Literal(123)),
            //    new Token(Token.TokenType.Star, "*", null, 1),
            //    new Expression.Grouping(
            //    new Expression.Literal(45.67))
            //);

            //Console.WriteLine(new ASTPrinter().print(expression));

            runFile(@"C:\Users\nkmac\Desktop\Basil\Basil\example.txt");
        }

        private static void runFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);

            run(text);

            // Indicate an error in the exit code.           
            if (hadError) { }
            if (hadRuntimeError) { }
        }

        private static void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            //foreach (var token in tokens)
            //{
            //    Console.WriteLine(token);
            //}

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.parse();

            // Stop if there was a syntax error.                   
            if (hadError)
            {
                Console.WriteLine("Encountered error.");
                return;
            }

            interpreter.interpret(statements);
            //Console.WriteLine(new ASTPrinter().print(expression));
        }

        public static void error(Token token, String message)
        {
            if (token.type == Token.TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void runtimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message} \n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where} : {message}");
            hadError = true;
        }
    }

    public abstract class Stmt
    {
        public interface Visitor<R>
        {
            //R visitBlockStmt(Block stmt);
            //R visitClassStmt(Class stmt);
            R visitExpressionStmt(Expression stmt);
            //R visitFunctionStmt(Function stmt);
            //R visitIfStmt(If stmt);
            R visitPrintStmt(Print stmt);
            //R visitReturnStmt(Return stmt);
            //R visitVarStmt(Var stmt);
            //R visitWhileStmt(While stmt);
        }

        // Nested Stmt classes here...            

        public abstract R accept<R>(Visitor<R> visitor);

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitExpressionStmt(this);
            }

            public readonly Expr expression;
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitPrintStmt(this);
            }

            public readonly Expr expression;
        }
    }
}
