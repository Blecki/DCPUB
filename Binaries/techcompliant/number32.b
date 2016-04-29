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

#ifndef _BITWISE_LIB_NUMBER32_
#define _BITWISE_LIB_NUMBER32_

struct num32
{
	high;
	low;
}

function compare32(a:num32, b:num32)
{
	if (a.high != b.high) return 0;
	if (a.low != b.low) return 0;
	return 1;
}

function add32(a:num32, b:num32, out:num32)
{
	asm (
		A = a;
		B = b;
		C = out )
	{
		SET X, [A+1]
		ADD X, [B+1]
		SET [C+1], X
		SET X, [A]
		ADX X, [B]
		SET [C], X
	}
}

#endif
