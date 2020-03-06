using System.Collections.Generic;

namespace BasilLang
{
    internal class BasilClass : ICallable
    {
        private readonly BasilClass superclass;
        private readonly Dictionary<string, BasilFunction> methods;

        public BasilClass(string name, BasilClass superclass, Dictionary<string, BasilFunction> methods)
        {
            Name = name;
            this.superclass = superclass;
            this.methods = methods;
        }

        public string Name { get; }

        public int Arity()
        {
            if (methods.TryGetValue("init", out BasilFunction initializer))
            {
                return initializer.Arity();
            }
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            BasilInstance instance = new BasilInstance(this);
            if (methods.TryGetValue("init", out BasilFunction initializer))
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public BasilFunction FindMethod(BasilInstance instance, string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name].Bind(instance);
            }
            if (superclass != null)
            {
                return superclass.FindMethod(instance, name);
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
