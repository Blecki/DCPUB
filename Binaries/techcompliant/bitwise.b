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
	asm ( A = value, B = size )
	{
		SET C, 0x0010
		SUB C, B
		SET B, 0xFFFF

		:BP_TRIM_TOP
		IFE C, 0x0000
		SET PC, BP_TRIM_END

		SHR B, 0x0001
		AND A, B
		SUB C, 0x0001

		SET PC, BP_TRIM_TOP

		:BP_TRIM_END

		// Leave return value in A
	}
}

#define BIT_MASK_RIGHT(size) (0xFFFF >> (16 - size))
#define BIT_MASK_LEFT(size) (0xFFFF << (16 - size))
#define BIT_MASK(size, offset) ((0xFFFF << (16 - size)) >> offset)

// Pack the value into the destination buffer, begining at bit offset
//  and using bits size.
// Destination should already be zeroed.
// Value should be 'trimmed' to size. Use bp_trim.
function bp_dpack(size, offset, value, destination)
{
	local value_interior_offset = 16 - size; // Assume value is in last size bits.
	local words_deep = offset / 16;
	local first_offset = offset % 16;

	// We need to align the value_interior_offset bit of value with the first_offset bit. Offset by difference does it.
	if (first_offset > value_interior_offset) // >>
	{
		destination[words_deep] |= (value >> (first_offset - value_interior_offset));
	}
	else // <<
	{ 
		destination[words_deep] |= (value << (value_interior_offset - first_offset));
	}

	if ((first_offset + size) > 15) // need to write post-value
	{
		local post_value = value << (16 - (size - (16 - first_offset)));
		destination[words_deep + 1] |= post_value;
	}
}

function bp_upack(size, offset, buffer)
{
	local value_interior_offset = 16 - size;
	local words_deep = offset / 16;
	local first_offset = offset % 16;

	local result = 0;
	if (first_offset > value_interior_offset)
	{
		result |= (buffer[words_deep] << (first_offset - value_interior_offset));
	}
	else
	{
		result |= (buffer[words_deep] >> (value_interior_offset - first_offset));
	}
	
	if ((first_offset + size) > 15)
	{
		result |= (buffer[words_deep + 1] >> (16 - (size - (16 - first_offset))));
	}

	return result;
}


// Clear the bits that would be occupied to zero.
function bp_dclear(size, offset, destination) 
{ 
 	local words_deep = offset / 16; 
 	local first_offset = offset % 16; 

       local mask = 0xFFFF >> (16 - size);

       if (first_offset > (16 - size)) 
          destination[words_deep] |= !(mask >> (16 - (size - first_offset)));
      else
         destination[words_deep] |= !(mask << (16 - (size - first_offset)));

      if ((first_offset + size) > 15)
         destination[words_deep + 1] |= !(value << (16 - (size - (16 - first_offset)));
} 

#endif
