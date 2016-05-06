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

#ifndef _BITWISE_LIB_STD_
#define _BITWISE_LIB_STD_

function memset(
	destination,
	value,
	count)
{
	local end = destination + count;

	while (destination < end)
	{
		*destination = value;
		destination += 1;
	}
}

function memcpy(
	destination,
	source,
	count)
{
	asm (
		I = destination;
		A = source;
		C = count)
	{
		SET PUSH, J
		SET J, A
		ADD C, I
			:__MEMCPY_BEGIN_LOOP
		STI [J], [I]
		IFL I, C
			SET PC, __MEMCPY_BEGIN_LOOP
		SET J, POP
	}
}

#endif


