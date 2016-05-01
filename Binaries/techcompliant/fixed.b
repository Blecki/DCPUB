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

#ifndef _BITWISE_LIB_FIXED_
#define _BITWISE_LIB_FIXED_

#include sqrt.b

#define fix_to_int(a) ((a) >> 8)
#define fix_from_int(a) ((a) << 8)
#define fix_add(a, b) ((a) + (b))
#define fix_sub(a, b) ((a) - (b))

function fix_mul(a, b)
{
	asm (A = a; B = b, C = &a)
	{
		MLI A, B
		SET B, EX
		SHR A, 0x0008
		SHL B, 0x0008
		ADD A, B
		SET [C], A
	}
	return a;
}

function fix_div(a, b)
{
	asm (A = a; B = b, C = &a)
	{
		DVI A, B
		SET B, EX
		SHL A, 0x0008
		SHR B, 0x0008
		ADD A, B
		SET [C], A
	}
	return a;
}

function fix_sqrt(a)
{
	return sqrt(a) << 4;
}

#endif