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
	while (count > 0)
	{
		*destination = *source;
		destination += 1;
		source += 1;
		count -= 1;
	}
}

#endif


