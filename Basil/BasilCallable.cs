using System;
using System.Collections.Generic;

namespace BasilLang
{
    interface BasilCallable
    {
        int arity();
        object call(Interpreter interpreter, List<object> arguments);
    }

    class BasilFunction : BasilCallable
    {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;

        public BasilFunction(Stmt.Function declaration, Environment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int arity()
        {
            return declaration.parameters.Count;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.parameters.Count; i++) {
                environment.define(declaration.parameters[i].lexeme,
                    arguments[i]);
            }

            try
            {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }

            return null;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }

    // use anonymous class

    public class ClockFunction : BasilCallable
    {
        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return (string)System.DateTime.Now.ToString("hh:mm tt");
        }
    }

    public class TickFunction : BasilCallable
    {
        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return (double)System.Environment.TickCount / 1000.0;
        }
    }

    public class PrintFunction : BasilCallable
    {
        public int arity()
        {
            return 1;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine(arguments[0].ToString());
            return null;
        }
    }

    public class Debug_DumpLocalsFunction : BasilCallable
    {
        private Interpreter interpreter;

        public Debug_DumpLocalsFunction(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine("[LOCAL VARIABLES]");
            Console.WriteLine("{");

            foreach (var item in interpreter.locals)
            {
                if (item.Key is Expr.Variable variable)
                {
                    Console.WriteLine($"\t k: {item.Key} (name: {variable.name})\t v: {item.Value}");
                }
                else if (item.Key is Expr.Literal literal)
                {
                    Console.WriteLine($"\t k: {item.Key} (name: {literal.value})\t v: {item.Value}");
                }
                else
                {
                    Console.WriteLine($"\t k: {item.Key}\t v: {item.Value}");
                }
            }

            Console.WriteLine("}");

            return null;
        }
    }

    public class Debug_DumpGlobalsFunction : BasilCallable
    {
        private Interpreter interpreter;

        public Debug_DumpGlobalsFunction(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine("[GLOBAL VARIABLES]");
            Console.WriteLine("{");

            foreach (var item in interpreter.globals.values)
            {
                Console.WriteLine($"\t name: {item.Key}\t type: {item.Value}");
            }

            Console.WriteLine("}");


            return null;
        }
    }

    public class Debug_DumpScopeFunction : BasilCallable
    {
        private Interpreter interpreter;

        public Debug_DumpScopeFunction(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine("[SCOPE VARIABLES]");
            Console.WriteLine("{");

            foreach (var item in interpreter.environment.values)
            {
                Console.WriteLine($"\t name: {item.Key}\t type: {item.Value}");
            }

            Console.WriteLine("}");

            return null;
        }
    }
}
