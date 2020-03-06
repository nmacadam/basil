class Benchmark 
{
	testTypeCheck()
	{
		var start = tick();
		var type = getType(this);
		var end = tick();
		
		var elapsed = end - start;
		
		print "elapsed:" + elapsed + "s";
	}
	
	testPrint()
	{
		var start = tick();
		
		for (var i = 0; i < 100; i = i + 1)
		{
			print "hello world";
		}
		
		var end = tick();
		
		var elapsed = end - start;
		
		print "elapsed:" + elapsed + "s";
	}
	
	testMath ()
	{
		var start = tick();
		
		for (var i = 0; i < 100; i = i + 1)
		{
			var r = random();
			var s = sqrt(r);
		}
		
		var end = tick();
		
		var elapsed = end - start;
		
		print "elapsed:" + elapsed + "s";
	}
}

var b = Benchmark();
b.testMath();