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

// Create a default environment to make writing basic programs easier.

#ifndef _BITWISE_LIB_DEFAULT_ENVIRONMENT_
#define _BITWISE_LIB_DEFAULT_ENVIRONMENT_

#include lem.b
#include console.b
#include itoa.b
#include keyboard.b

local allocatable_memory = __endofprogram;		//Reserve half of available memory as the heap. This means a larger
initialize_memory_page(allocatable_memory, 0x8000);	//program makes for a smaller stack.
local lem_device = lem_detect();
local video_memory = allocate_memory(LEM_VRAM_SIZE, allocatable_memory);
lem_initialize(lem_device, video_memory);
static console:Console[sizeof Console];
console_initialize(console, video_memory);
local keyboard = kb_detect();

#define malloc(size) allocate_memory(size, allocatable_memory)
#define free(block) free_memory(block, allocatable_memory)
#define VARARG(which, into) asm (A = which; B = &into; C = 0) { SET C, J; ADD C, A; SET [B], [C] }

function clear()
{
	console_clear(console);
}

static printf = &__printf;
	// Alias printf. DCPUB checks argument counts for directly referenced functions but can't check
	// them for aliased ones. This is a hack to support varidic arguments to printf.

// Prints a string to the console, replacing each instance of '%' with a successive argument.
function __printf(string /* Followed by some number of arguments */)
{
	local index = 0;
	local strlen = *string;
	local parameter_index = 3; //The first parameter is at J + 2; We want parameter 3 and onwards.
	local number_buffer[32];
	while (index < strlen)
	{
		if (string[index + 1] == '%') //Found a key code.
		{
			local parameter = 0;
			VARARG(parameter_index, parameter);
			parameter_index += 1;
			
			index += 1;

			if (string[index + 1] == 'N')
				itoa(parameter, number_buffer);
			else if (string[index + 1] == 'X')
				itox(parameter, number_buffer);

			console_stringout(console, number_buffer);
		}
		else
		{
			if (string[index + 1] == '\n')
			{
				while ((console.cursor_position % 32) != 0) console_charout(console, ' ');
			}
			else
				console_charout(console, string[index + 1]);
		}
		index += 1;
	}
}

#endif
