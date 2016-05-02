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
*/

#ifndef _BITWISE_LIB_BITWISE_
#define _BITWISE_LIB_BITWISE_

// I couldn't resist the name. Deal with it - AC

// Set all bits over SIZE bits to 0.
function bp_trim(size, value)
{
	local to_trim = 16 - size;
	local mask = 0xFFFF;

	while (to_trim != 0)
	{
		mask >>= 1;
		value &= mask;
		to_trim -= 1;
	}

	return value;
}

// Pack the value into the destination buffer, begining at bit offset
//  and using bits size.
// Destination should already be zeroed.
function bp_dpack(size, offset, value, destination)
{
	local value_interior_offset = 16 - size; // Assume value is in last size bits.
	local words_deep = offset / 16;
	local first_offset = offset % 16;

	// We need to align the value_interior_offset bit of value with the first_offset bit. Offset by difference does it.
	local pre_value = value >> (first_offset - value_interior_offset);
	destination[words_deep] |= pre_value;

	if ((first_offset + size) > 15) // need to write post-value
	{
		local post_value = value << (16 - (size - (first_offset - value_interior_offset)));
		destination[words_deep + 1] |= post_value;
	}
}

#endif
