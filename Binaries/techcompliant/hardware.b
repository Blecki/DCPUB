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
*	Hardware detection routine
*/

#ifndef _BITWISE_LIB_HARDWARE_
#define _BITWISE_LIB_HARDWARE_

#include number32.dc

// Find a piece of hardware with the supplied id.
// Returns 0xFFFF if no matching hardware found.
function detect_hardware( id:num32 /* A pointer to a num32 struct */)
{
	local num_hardware = 0;
	asm ( B = &num_hardware ) // Passing the address of a local is currently the only way to get data out of an asm block.
	{
		HWN [B]
	}

	local n = 0;
	while ( n < num_hardware )
	{
		local hardware_id[2];

		asm ( A = n; B = 0; I = hardware_id ) // Fetch the hardware id of hardware n. B is mentioned just so it will be preserved.
		{
			HWQ A
			SET [I + 0x0001], A
			SET [I], B
		}

		if ( compare32(hardware_id, id) != 0 )
			return n;

		n += 1;
	}
	return -1; // No matching hardware was found
} 

#endif

