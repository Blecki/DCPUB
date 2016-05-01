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
#include vec3.b

clear();
printf("TESTING VEC3\n");

local a:vec3[sizeof vec3];
local b:vec3[sizeof vec3];

a.x = fix_from_int(1);
a.y = fix_from_int(1);
a.z = fix_from_int(1);

printf("LSQARD A = %\n", fix_to_int(vec3_lengthsquared(a)));
printf("LENGTH A = %\n", fix_to_int(vec3_length(a)));

