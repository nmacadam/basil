# basil
a dynamically typed, interpreted scripting language (and C# interpreter!)

### interpreter features
- preprocessing
- lexical analysis
- parsing
- intermediate code generation
- optimization

### language features
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

### examples
```javascript
// fizzbuzz
fun fizzbuzz(start, end)
{
	for (var i = 0; i < end; i = i + 1)
	{
		if (i % 3 == 0 and i % 5 == 0) print "FizzBuzz";
		else if (i % 3 == 0) print "Fizz";
		else if (i % 5 == 0) print "Buzz";
		else print i;
	}
}

fizzbuzz(1,15);
```

crafted with knowledge from Bob Nystrom's [*Crafting Interpreters*](https://craftinginterpreters.com/)
