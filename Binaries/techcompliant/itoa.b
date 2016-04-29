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

#ifndef _BITWISE_LIB_ITOA_
#define _BITWISE_LIB_ITOA_

function __itoa(number, buffer)
{
	if (number > 9) __itoa(number / 10, buffer);
	buffer[0] += 1;
	buffer[buffer[0]] = '0' + (number % 10);
}

function itoa(number, buffer)
{
	buffer[0] = 0;
	__itoa(number, buffer);
}

#endif
