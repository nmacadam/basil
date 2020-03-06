class Fib {
	calculate(n) {
		var a = 1;
		var b = 1;

		for (var i = 0; i < n; i = i + 1) {
			print a;
			var temp = a;
			a = b;
			b = temp + b;
		}
	}
}

var f = Fib();
f.calculate(20); 
