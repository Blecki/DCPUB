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
#include number32.b

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

function fix_mul32(a:num32, b:num32, out:num32)
{
	asm (A = a, B = b, C = out)
	{
		// The result of 32*32 multiplication is 64 bits. 
		// We want the middle ones.

		// extract 'middle' of a
		SET X, [A]
		SHL X, 0x0008
		SET Y, [A+1]
		SHR Y, 0x0008
		BOR X, Y

		// extract 'middle' of b
		SET Y, [B]
		SHL Y, 0x0008
		SET I, [B+1]
		SHR I, 0x0008
		BOR Y, I

		MUL X, Y
		SET [C], EX
		SET [C+1], X

		// Precise this is not. But wow is it fast!
	}
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

function fix_div32(a:num32, b:num32, out:num32)
{
	asm (
		A = a;
		B = b;
		C = out )
	{
		// We get 'close' by ignoring the low words entirely.
		SET X, [A]
		DIV X, [B]
		SET Y, EX
		SET [C], X
		SET [C+1], Y
	}
}

#endif