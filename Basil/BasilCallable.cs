using System;
using System.Collections.Generic;

namespace BasilLang
{
    interface BasilCallable
    {
        int arity();
        object call(Interpreter interpreter, List<object> arguments);
    }

    public class BasilInstance
    {
        private BasilClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public BasilInstance(BasilClass klass)
        {
            this.klass = klass;
        }

        public object get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            BasilFunction method = klass.findMethod(name.lexeme);
            if (method != null) return method.bind(this);

            throw new RuntimeError(name,
                "Undefined property '" + name.lexeme + "'.");
        }

        public void set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            return klass.name + " instance";
        }
    }

    public class BasilClass : BasilCallable
    {
        public readonly string name;
        public readonly BasilClass superclass;
        private readonly Dictionary<string, BasilFunction> methods;

        public BasilClass(string name, BasilClass superclass, Dictionary<string, BasilFunction> methods)
        {
            this.name = name;
            this.superclass = superclass;
            this.methods = methods;
        }

        public BasilFunction findMethod(string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }

            if (superclass != null)
            {
                return superclass.findMethod(name);
            }

            return null;
        }

        public int arity()
        {
            BasilFunction initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            BasilInstance instance = new BasilInstance(this);

            BasilFunction initializer = findMethod("init");
            if (initializer != null)
            {
                initializer.bind(instance).call(interpreter, arguments);
            }

            return instance;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class BasilFunction : BasilCallable
    {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        private readonly bool isInitializer;

        public BasilFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
        }

        public BasilFunction bind(BasilInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.define("this", instance);
            return new BasilFunction(declaration, environment, isInitializer);
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
                if (isInitializer) return closure.getAt(0, "this");

                return returnValue.value;
            }

            if (isInitializer) return closure.getAt(0, "this");

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
