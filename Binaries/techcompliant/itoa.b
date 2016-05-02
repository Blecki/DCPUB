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

function itox(number, buffer)
{
	buffer[0] = 4;

	if ((number % 16) < 10)
		buffer[4] = '0' + (number % 16);
	else
		buffer[4] = 'A' + ((number % 16) - 10);

	number /= 16;
	if ((number % 16) < 10)
		buffer[3] = '0' + (number % 16);
	else
		buffer[3] = 'A' + ((number % 16) - 10);

	number /= 16;
	if ((number % 16) < 10)
		buffer[2] = '0' + (number % 16);
	else
		buffer[2] = 'A' + ((number % 16) - 10);

	number /= 16;
	if ((number % 16) < 10)
		buffer[1] = '0' + (number % 16);
	else
		buffer[1] = 'A' + ((number % 16) - 10);

}

#endif
