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

function sub32(a:num32, b:num32, out:num32)
{
	asm (
		A = a;
		B = b;
		C = out )
	{
		SET X, [A+1]
		SUB X, [B+1]
		SET [C+1], X
		SET X, [A]
		SBX X, [B]
		SET [C], X
	}
}

function mul32(a:num32, b:num32, out:num32)
{
	asm (
		A = a;
		B = b;
		C = out )
	{
		// The result of a 32 bit multiply is a 
		//	64 bit number. But we only want the
		//  lower 32 bits.
		
		// Calculate low word
		SET X, [A+1]
		MUL X, [B+1]
		SET I, EX 		// Save overflow to add to high word.

		// Calculate high word. We can safely 
		//   ignore the overflow of each sub mul.
		SET Y, [A]
		MUL Y, [B+1]
		SET Z, [A+1]
		MUL Z, [B]
		ADD Y, Z
		ADD Y, I
		SET [C], Y
		SET [C+1], X

	}
}

function div32(a:num32, b:num32, out:num32)
{
	asm (
		A = a;
		B = b;
		C = out )
	{
		// We get 'close' by ignoring the low words entirely.
		SET X, [A]
		DIV X, [B]
		SET [C], 0x0000
		SET [C+1], X
	}
}

#endif
