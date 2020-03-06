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
            //runPrompt();
            RunFile(@"C:\Users\nkmac\Desktop\basil-master\Basil\example.bsl");
            //RunFile(@"C:\Users\nkmac\Desktop\basil-master\Basil\cake.bsl");
            //RunFile(@"C:\Users\nkmac\Desktop\basil-master\Basil\superclass.bsl");
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void RunFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);

            Run(text);

            // Indicate an error in the exit code.
            if (hadError) { }
            if (hadRuntimeError) { }
        }

        private static void Run(string source)
        {
            Importer importer = new Importer(source);
            string preprocessedSource = importer.Process();

            Scanner scanner = new Scanner(preprocessedSource);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                Console.WriteLine("Encountered error.");
                return;
            }

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadError) return;

            interpreter.Interpret(statements);
            //Console.WriteLine(new ASTPrinter().print(expression));
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == Token.TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"[line {error.token.line}] Runtime Error : {error.Message}");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where} : {message}");
            hadError = true;
        }
    }
}
