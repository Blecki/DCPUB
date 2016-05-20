//if (x > 3 && y < 4) {}

local b = 5;
if (b) {}

local x = 5;
local y = 9;
local z = x && y;
local i = 5 == 9;
local j = x || y;
local k = x && y || i;

if (x && y)
{
	// Test for short circuit.
}

if ((x > 0) && (y < 4))
{
	x = 3;
}

if (x > 0 && y < 4) 
{
	x = 5;
}
