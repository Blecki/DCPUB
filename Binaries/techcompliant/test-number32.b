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
#include number32.b

clear();
printf("Testing 32 bit operations.\n");

local a:num32 = malloc(sizeof num32);
local b:num32 = malloc(sizeof num32);

a.high = 0x0001;
a.low = 0x0002;

b.high = 0x0000;
b.low = 0xFFFF;

printf("A     = %-%\n", a.high, a.low);
add32(a, b, a);
printf("A + B = %-%\n", a.high, a.low);
sub32(a, b, a);
printf("A - B = %-%\n", a.high, a.low);

b.low = 0x0004;
mul32(a, b, a);
printf("A * B = %-%\n", a.high, a.low);

a.high = 32;
a.low = 32;
b.high = 4;
b.low = 8;
div32(a, b, a);
printf("A / B = %-%\n", a.high, a.low);
