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

#include test-framework.b
#include bitwise.b

printf("TESTING BITWISE.B\n");

EQUAL("FFFF TRIM 8 = 00FF", 0x00FF, bp_trim(8, 0xFFFF));
EQUAL("FFFF TRIM 4 = 000F", 0x000F, bp_trim(4, 0xFFFF));

static a[2];
a[0] = 0;
a[1] = 0;

function cleara()
{
	a[0] = 0;
	a[1] = 0;
}

bp_dpack(16, 4, 0xFFFF, a);
EQUAL2("FFFF PACKED 8 = 0FFF:F000", 0x0FFF, a[0], 0xF000, a[1]);
EQUAL("UNPACKED = FFFF", 0xFFFF, bp_upack(16, 4, a));

cleara();

bp_dpack(8, 12, 0x00FF, a);
EQUAL2("00FF PACKED 12 = 000F:F000", 0x000F, a[0], 0xF000, a[1]);

STATS();

