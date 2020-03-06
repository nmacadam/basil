using System.Collections.Generic;

namespace BasilLang
{
    internal class BasilFunction : ICallable
    {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        private readonly bool isInitializer;

        public BasilFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.declaration = declaration;
            this.closure = closure;
            this.isInitializer = isInitializer;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public BasilFunction Bind(BasilInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new BasilFunction(declaration, environment, isInitializer);
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }
            if (isInitializer) return closure.GetAt(0, "this");
            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}
