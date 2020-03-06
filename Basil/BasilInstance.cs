using System.Collections.Generic;

namespace BasilLang
{
    internal class BasilInstance
    {
        private readonly BasilClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public BasilInstance(BasilClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            BasilFunction method = klass.FindMethod(this, name.lexeme);
            if (method != null) return method;

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            return $"{klass.Name} instance";
        }
    }
}
