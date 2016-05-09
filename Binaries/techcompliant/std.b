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
	asm (	A = destination;
		B = value;
		C = count)
	{
		ADD C, A
			:__MEMSET_BEGIN_LOOP
		IFL A, C
			SET PC, __MEMSET_END_LOOP
		SET [A], B
		ADD A, 1
		SET PC, __MEMSET_BEGIN_LOOP
			:__MEMSET_END_LOOP
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
		IFL I, C
			SET PC, __MEMCPY_END_LOOP
		STI [J], [I]
		SET PC, __MEMCPY_BEGIN_LOOP
			:__MEMCPY_END_LOOP
		SET J, POP
	}
}

#endif


