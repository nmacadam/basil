# basil
a dynamically typed, interpreted scripting language (and interpreter written in C#!)

### ...ok, but why?
Basil is a fully-featured, dynamically typed, interpreted ~~mouthful~~ scripting language and C# interpreter designed with user-extensibility in mind.  Created to be a companion to C# game development environments (notably, Unity), to be used to create powerful debugging and/or modding tools.  

With a simple Native Function interface, users can create their own Basil functions from a C# perspective, making it easy to integrate into other C# environments, and letting the user define how restrictive they want the bridge between C# and Basil to be.

Also, [writing an interpreter](https://craftinginterpreters.com/) and designing your own programming language is fun!

### ⚗ interpreter features
- native function interface
- preprocessing
- lexical analysis
- parsing
- intermediate code generation
- optimization
- code generation

### 💭 language features
- dynamic typing
- automatic memory management
- data types
- expressions
- statements
- variables
- control flow
- functions
- classes
- inheritance

### 📝 examples
```javascript
// fizzbuzz
fun fizzbuzz(start, end)
{
    for (var i = 0; i < end; i++)
    {
        if (i % 3 == 0 and i % 5 == 0) print "FizzBuzz";
        else if (i % 3 == 0) print "Fizz";
        else if (i % 5 == 0) print "Buzz";
        else print i;
    }
}

fizzbuzz(1,15);
```

### 🌉 native function interface
Creating Basil functions in C# looks like...
```csharp
namespace BasilLang.NFI
{
    // The NativeFunction attribute tells the interpreter to add this to it's Basil function library

    [NativeFunction]
    public class SayHiFunction : NativeCallable
    {
        // what is the name of this method in Basil?
        public string MethodName => "sayHi";

        // How many arguments does the function have?
        public int Arity()
        {
            return 1;
        }

        //  sayHi("Developer");
        //  will print 'Hello, Developer'
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine("Hello, " + arguments[0].ToString());
            return null;
        }
    }
}

```

### 🚋 roadmap
- finalize basic language syntax
- add switch statements
- add list and map types
- optimize interpreter
- create some tools for using Basil
___
crafted with knowledge from Bob Nystrom's [*Crafting Interpreters*](https://craftinginterpreters.com/)
