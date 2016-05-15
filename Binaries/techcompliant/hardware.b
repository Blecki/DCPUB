/*
*    _________.________________.      __.___  ____________________
*    \______   \   \__    ___/  \    /  \   |/   _____/\_   _____/
*     |    |  _/   | |    |  \   \/\/   /   |\_____  \  |    __)_ 
*     |    |   \   | |    |   \        /|   |/        \ |        \ 
*     |______  /___| |____|    \__/\  / |___/_______  //_______  /
*            \/                     \/              \/         \/ 
*
*       (c) BITWISE SCIENCES 1984 - 2108
*
*       Hardware detection routine
*/

#ifndef _BITWISE_LIB_HARDWARE_
#define _BITWISE_LIB_HARDWARE_

/*
	Find a piece of hardware with the supplied id.
	Returns 0xFFFF if no matching hardware found.
*/
function detect_hardware( id:num32 /* A pointer to a num32 struct */)
{
	local num_hardware = 0;
	asm ( B = &num_hardware )
	{
		HWN [B]
	}

	local n = 0;
	while ( n < num_hardware )
	{
		local hardware_id[2];

		asm ( A = n; I = hardware_id ) 
		{
			HWQ A
			SET [I + 0x0001], A
			SET [I], B
		}

		if ( (hardware_id[0] == id[0]) & (hardware_id[1] == id[1]) )
			return n;

		n += 1;
	}
	
	return 0xFFFF;
} 

#endif

