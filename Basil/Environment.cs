using System;
using System.Collections.Generic;

namespace BasilLang
{
    public class Environment
    {
        private readonly Environment enclosing;

        /*private*/
        public readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public object get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            if (enclosing != null) return enclosing.get(name);

            throw new RuntimeError(name,
                "Undefined variable '" + name.lexeme + "'.");
        }

        public void assign(Token name, Object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                //values.Add(name.lexeme, value);
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name,
                "Undefined variable '" + name.lexeme + "'.");
        }

        public void define(string name, object value)
        {
            //values.Add(name, value);

            values[name] = value;
        }
    }
}
