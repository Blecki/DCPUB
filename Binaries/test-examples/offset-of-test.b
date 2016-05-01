struct foo
{
	bar;
	stool;
}

local x = offsetof stool in foo;
local y = sizeof foo;

