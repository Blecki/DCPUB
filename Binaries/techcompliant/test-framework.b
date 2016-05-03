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

#ifndef _BITWISE_LIB_TESTING_
#define _BITWISE_LIB_TESTING_

#include default_environment.b

static pass = 0;
static fail = 0;

clear();
printf("TESTING FRAMEWORK LOADED\n");

function EQUAL(msg, a, b)
{
	printf(msg);

	if (a == b)
	{
		printf(" YES\n");
		pass += 1;
	}
	else
	{
		printf(" NO\n");
		fail += 1;
	}
}

function EQUAL2(msg, a, b, c, d)
{
	printf(msg);

	if ((a == b) & (c == d))
	{
		printf(" YES\n");
		pass += 1;
	}
	else 
	{
		printf(" NO\n");
		fail += 1;
	}
}

function STATS()
{
	printf("PASS %N, FAIL %N", pass, fail);
}

#endif
