using BasilLang.NativeFunctions;
using System;
using System.Collections.Generic;

/*
 *     Basil Language Native Function Interface:
 *      define functions here to use in basil
 */

namespace BasilLang.NFI
{
    [NativeFunction]
    public class SayHiFunction : NativeCallable
    {
        public string MethodName => "sayHi";

        public int arity()
        {
            return 1;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine("Hello, " + arguments[0].ToString());
            return null;
        }
    }
}
