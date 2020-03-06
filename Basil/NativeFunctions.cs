using System;
using System.Collections.Generic;
using System.IO;

namespace BasilLang
{
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class NativeFunction : Attribute { }

    namespace NativeFunctions
    {
        public interface NativeCallable : ICallable
        {
            string MethodName { get; }
        }

        #region I/O

        [NativeFunction]
        public class PrintFunction : NativeCallable
        {
            public string MethodName => "print";

            public int Arity()
            {
                return 1;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                Console.WriteLine(arguments[0].ToString());
                return null;
            }
        }

        [NativeFunction]
        public class ReadFunction : NativeCallable
        {
            public string MethodName => "read";

            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                var input = Console.ReadLine();

                if (double.TryParse(input, out double doubleValue))
                {
                    return doubleValue;
                }
                else if (bool.TryParse(input, out bool boolValue))
                {
                    return boolValue;
                }
                else
                {
                    return input;
                }
            }
        }

        [NativeFunction]
        public class Open : NativeCallable
        {
            public string MethodName => "open";

            public int Arity()
            {
                return 1;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                string contents;
                try
                {
                    contents = System.IO.File.ReadAllText(arguments[0].ToString());

                    return contents;
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("file not found");
                }
                
                return null;
            }
        }

        #endregion

        #region Math

        [NativeFunction]
        public class Abs : NativeCallable
        {
            public string MethodName => "abs";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Abs((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Ceiling : NativeCallable
        {
            public string MethodName => "ceiling";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Ceiling((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Cos : NativeCallable
        {
            public string MethodName => "cos";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Cos((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Floor : NativeCallable
        {
            public string MethodName => "floor";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Floor((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Log : NativeCallable
        {
            public string MethodName => "log";
            public int Arity() { return 2; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Log((double)arguments[0], (double)arguments[1]);
            }
        }

        [NativeFunction]
        public class Max : NativeCallable
        {
            public string MethodName => "max";
            public int Arity() { return 2; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Max((double)arguments[0], (double)arguments[1]);
            }
        }

        [NativeFunction]
        public class Min : NativeCallable
        {
            public string MethodName => "min";
            public int Arity() { return 2; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Min((double)arguments[0], (double)arguments[1]);
            }
        }

        [NativeFunction]
        public class Pow : NativeCallable
        {
            public string MethodName => "min";
            public int Arity() { return 2; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Pow((double)arguments[0], (double)arguments[1]);
            }
        }

        [NativeFunction]
        public class Random : NativeCallable
        {
            public string MethodName => "random";
            public int Arity() { return 0; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                var rand = new System.Random();
                return (double)rand.Next();
            }
        }

        [NativeFunction]
        public class Round : NativeCallable
        {
            public string MethodName => "round";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Round((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Sign : NativeCallable
        {
            public string MethodName => "sign";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return (double)Math.Sign((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Sin : NativeCallable
        {
            public string MethodName => "sin";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Sin((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Sqrt : NativeCallable
        {
            public string MethodName => "sqrt";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Sqrt((double)arguments[0]);
            }
        }

        [NativeFunction]
        public class Tan : NativeCallable
        {
            public string MethodName => "tan";
            public int Arity() { return 1; }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return Math.Tan((double)arguments[0]);
            }
        }

        #endregion

        #region Time

        [NativeFunction]
        public class ClockFunction : NativeCallable
        {
            public string MethodName => "clock";

            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return (string)System.DateTime.Now.ToString("hh:mm tt");
            }
        }

        [NativeFunction]
        public class DateFunction : NativeCallable
        {
            public string MethodName => "date";

            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return (string)System.DateTime.Now.ToString("MMMM dd, yyyy");
            }
        }

        [NativeFunction]
        public class TickFunction : NativeCallable
        {
            public string MethodName => "tick";

            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return (double)System.Environment.TickCount / 1000.0;
            }
        }

        #endregion

        #region Utilities

        [NativeFunction]
        public class GetTypeFunction : NativeCallable
        {
            public string MethodName => "getType";

            public int Arity()
            {
                return 1;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] == null)
                {
                    return "nil";
                }

                if (arguments[0] is null)
                {
                    return "nil";
                }
                else if (arguments[0] is double)
                {
                    return "number";
                }
                else if (arguments[0] is bool)
                {
                    return "boolean";
                }
                else if (arguments[0] is string)
                {
                    return "string";
                }
                else if (arguments[0] is BasilClass bClass)
                {
                    return bClass.Name;
                }
                else if (arguments[0] is BasilInstance bInstance)
                {
                    return bInstance.GetClass().Name;
                }
                else if (arguments[0] is BasilFunction basilFunction)
                {
                    return basilFunction.GetName();
                }

                throw new RuntimeError(null, "Type-check failed!");
            }
        }

        #endregion
    }
}
