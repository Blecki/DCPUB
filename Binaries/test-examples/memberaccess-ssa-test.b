// Do not run this code.

struct foo
{
	a;
	b;
}

local x:foo[sizeof foo];

x.a = 4;
x.b = 5;

if (x.a > x.b)
{
	x.a = 6;
}

local y:foo = 0x2000; // Don't do this.

if (y.a != y.b)
{
	y.b = x.a;
}


