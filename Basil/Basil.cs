using System;
using System.Collections.Generic;

namespace BasilLang
{
    class Basil
    {
        static readonly Interpreter interpreter = new Interpreter();
        public static readonly ASTPrinter printer = new ASTPrinter();
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

            //runPrompt();
            runFile(@"C:\Users\nkmac\Desktop\basil-master\Basil\example.bsl");
        }

        private static void runPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                run(Console.ReadLine());
                hadError = false;
            }
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
            Importer importer = new Importer(source);
            string preprocessedSource = importer.import();

            Scanner scanner = new Scanner(preprocessedSource);
            List<Token> tokens = scanner.scanTokens();

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

        public static void error(int line, string message)
        {
            report(line, "", message);
        }

        public static void error(Token token, string message)
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
}
