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

#ifndef _BITWISE_LIB_SQRT_
#define _BITWISE_LIB_SQRT_

function sqrt(n)
{
	local r = 0;
	local b = 1 << 14;

	while (n < b) b >>= 2;

	while (b != 0)
	{
		if (n < r + b) r >>= 1;
		else
		{
			n -= r + b;
			r = (r >> 1) + b;
		}

		b >>= 2;
	}

	return r;
}

#endif
