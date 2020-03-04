using System;
using System.Collections.Generic;

namespace BasilLang
{
    public class Environment
    {
        public readonly Environment enclosing;

        /*private*/ public readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        //~Environment()
        //{
        //    Console.WriteLine("CLOSING ENVIRONMENT");
        //    foreach (var item in values)
        //    {
        //        Console.WriteLine($"\t k: {item.Key}\t v: {item.Value}");
        //    }
        //}

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

        public Object getAt(int distance, String name)
        {
            if (ancestor(distance).values.ContainsKey(name))
            {
                return ancestor(distance).values[name];
            }
            else return null;
        }

        public void assignAt(int distance, Token name, Object value)
        {
            ancestor(distance).values[name.lexeme] = value;
        }

        Environment ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        public override string ToString()
        {
            string result = values.ToString();
            if (enclosing != null)
            {
                result += " -> " + enclosing.ToString();
            }

            return result;
        }
    }
}
