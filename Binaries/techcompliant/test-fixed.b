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
#include fixed.b
#include sqrt.b

clear();
printf("TESTING FIXED POINT\n");

printf("2 * 2 = %\n", fix_to_int(fix_mul(fix_from_int(2), fix_from_int(2))));
printf("SQRT 9 = %\n", sqrt(fix_from_int(9)) >> 4);
printf("SQRT 16 = %\n", sqrt(fix_from_int(16)) >> 4);
printf("SQRT 121 = %\n", sqrt(fix_from_int(121)) >> 4);
printf("SQRT 0x0240 = %\n", sqrt(0x0240) << 4);

local a:num32 = malloc(sizeof num32);
local b:num32 = malloc(sizeof num32);

a.high = 32;
b.high = 4;
fix_div32(a, b, a);
printf("A / B = %-%\n", a.high, a.low);

a.high = 32;
a.low = 0;
b.high = 0;
b.low = 0x8000;
fix_mul32(a, b, a);
printf("A * B = %-%\n", a.high, a.low);
