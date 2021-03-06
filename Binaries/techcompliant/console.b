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

// An abstraction to treat a block of memory as a text console.
// Initialize a console and a Lem device with the same memory.

#ifndef _BITWISE_LIB_CONSOLE_
#define _BITWISE_LIB_CONSOLE_

#include std.b
#include vec.b

#define CONSOLE_SIZE 384
#define CONSOLE_WIDTH 32
#define CONSOLE_HEIGHT 12

struct Console
{
	buffer;
	cursor_position;
	color;
	backspace_point;
}

// Initialize a console object.
function console_initialize(
	console:Console /* A pointer to sizeof(Console) words of memory */, 
	vram /* A pointer to 384 words of memory */)
{
	console.buffer = vram;
	console.cursor_position = 0;
	console.color = 0xF000; // Default color is white on black.
	console.backspace_point = 0;
}

// Clear the console buffer
function console_clear(console:Console)
{
	console.cursor_position = 0;
	console.backspace_point = 0;
	memset(console.buffer, 0x0000, 384);
}

// Change the cursor position. Must always be in range (0,CONSOLE_SIZE).
function console_setcursor(console:Console, cursor_position)
{
	console.cursor_position = cursor_position;
}

// Scrolls the contents of video memory by a number of lines.
function console_scroll(console:Console, lines)
{
	local chars = lines * CONSOLE_WIDTH;
	if (chars > CONSOLE_SIZE) chars = CONSOLE_SIZE;
	local copy_maximum = CONSOLE_SIZE - chars;
	memcpy(console.buffer, console.buffer + chars, copy_maximum);
	memset(console.buffer + copy_maximum, console.color, chars);
	console.cursor_position -= chars;
}

// Print a single character to a console.
function console_charout(console:Console, char)
{
	console.buffer[console.cursor_position] = console.color | char;
	console.cursor_position += 1;
	if (console.cursor_position > (CONSOLE_SIZE - 1))
		console_scroll(console, 1);
}

// Print a string to a console. 
function console_stringout(console:Console, string /* A pointer to string data. */)
{
	// Strings are assumed to be in length-prefix format.
	local strlen = (*string) & 0x00FF; // Masking with 0x00FF allows this function to work with vecs as well. See vec.dc
	// Don't need to worry about strings filling the entire screen and then some - length is truncated to 256 words.
	if ((console.cursor_position + strlen) > (CONSOLE_SIZE - 1))
		console_scroll(console, strlen / CONSOLE_WIDTH); // Make enough space on the screen for the string.
	local place = 0;
	while (place < strlen)
	{
		console.buffer[console.cursor_position] = console.color | string[place + 1];
		console.cursor_position += 1;
		place += 1;
	}
}

// Move the cursor back one space, erasing contents of buffer.
// Will not move back further than Console.backspace_point.
function console_backspace(console:Console)
{
	if (console.cursor_position == console.backspace_point) return 0;
	console.cursor_position -= 1;
	console.buffer[console.cursor_position] = console.color;
}

#endif
