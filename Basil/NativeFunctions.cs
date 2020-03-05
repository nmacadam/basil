﻿using System;
using System.Collections.Generic;

namespace BasilLang
{
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class NativeFunction : Attribute { }

    namespace NativeFunctions
    {
        public interface NativeCallable : BasilCallable
        {
            string MethodName { get; }
        }

        [NativeFunction]
        public class ClockFunction : NativeCallable
        {
            public string MethodName => "clock";

            public int arity()
            {
                return 0;
            }

            public object call(Interpreter interpreter, List<object> arguments)
            {
                return (string)System.DateTime.Now.ToString("hh:mm tt");
            }
        }

        [NativeFunction]
        public class TickFunction : NativeCallable
        {
            public string MethodName => "tick";

            public int arity()
            {
                return 0;
            }

            public object call(Interpreter interpreter, List<object> arguments)
            {
                return (double)System.Environment.TickCount / 1000.0;
            }
        }

        [NativeFunction]
        public class PrintFunction : NativeCallable
        {
            public string MethodName => "print";

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

        [NativeFunction]
        public class ReadFunction : NativeCallable
        {
            public string MethodName => "read";

            public int arity()
            {
                return 0;
            }

            public object call(Interpreter interpreter, List<object> arguments)
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
        public class GetTypeFunction : NativeCallable
        {
            public string MethodName => "getType";

            public int arity()
            {
                return 1;
            }

            public object call(Interpreter interpreter, List<object> arguments)
            {
                if (arguments[0] == null)
                {
                    return "nil";
                }

                string obj = arguments[0].ToString();

                if (obj is null)
                {
                    return "nil";
                }
                else if (double.TryParse(obj, out double doubleValue))
                {
                    return "number";
                }
                else if (bool.TryParse(obj, out bool boolValue))
                {
                    return "boolean";
                }
                else
                {
                    return "string";
                }
            }
        }
    }
}