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


#ifndef _BITWISE_LIB_VEC3_
#define _BITWISE_LIB_VEC3_

#include fixed.b

struct vec3
{
	x;
	y;
	z;
}

function vec3_lengthsquared(a:vec3)
{
	return fix_mul(a.x, a.x) + fix_mul(a.y, a.y) + fix_mul(a.z, a.z);
}

function vec3_length(a:vec3)
{
	return fix_sqrt(vec3_lengthsquared(a));
}

function vec3_add(a:vec3, b:vec3, out:vec3)
{
	out.x = a.x + b.x;
	out.y = a.y + b.y;
	out.z = a.z + b.z;
}

function vec3_sub(a:vec3, b:vec3, out:vec3)
{
	out.x = a.x - b.x;
	out.y = a.y - b.y;
	out.z = a.z - b.z;
}

function vec3_dot(a:vec3, b:vec3)
{
	return fix_mul(a.x, b.x) + fix_mul(a.y, b.y);
}