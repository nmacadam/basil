using System;
using System.Collections.Generic;

namespace BasilLang
{
    public interface BasilCallable
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
}
