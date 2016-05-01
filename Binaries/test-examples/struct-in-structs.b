struct foo
{
	a;
	b;
}

struct bar
{
	a:foo[sizeof foo];
	b:foo[sizeof foo];
	c;
}

local x = sizeof bar;
