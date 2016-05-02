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

#include default_environment.b
#include bitwise.b

clear();
printf("TESTING BITWISE\n");

printf("FFFF TRIMED = %X\n", bp_trim(8, 0xFFFF));

local a[2];
a[0] = 0;
a[1] = 0;

bp_dpack(16, 8, 0xFFFF, a);
printf("FFFF PACKED = %X:%X\n", a[0], a[1]);
