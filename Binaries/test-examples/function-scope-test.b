function a() {}		// Okay.

function b() { 		// Okay.
	function c() {
		a();
	
		local x = 5;
		local y = 5;
		
		if (x == y)
		{
			b();
			c();	
		}
	} // ERROR: Functions must be at global scope
	
	local x = a() + c();

	if (x == 0)
	{
		function d() {}
	}
}

a();
b();
//c(); // Error: Cannot find function

