import "importCode.bsl"
import "importCode 2.bsl"

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

fizzbuzz(1,20);

var in = read();
var x;

print "type: " + getType(in);
print "type x: " + getType(x);

sayHi("Creator");

helloWorld();
goodbyeWorld();