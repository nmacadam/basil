/* foo bar */

var i = 1;
while (i < 15)
{
	i = i + 1;
	
	print /*i*/i + 1;
	if (i > 4) continue;
	print "foo";
}

// continue is broken for for :(
// the iterator doesnt run after continue currently

//for (var i = 1; i < 15; i = i + 1)
//{
//	print i;
//	
//	if (i > 4) continue;
//	
//	print "foo";
//}