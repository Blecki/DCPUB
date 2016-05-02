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
*
* 		3d vectors based on fixed-point values.
*		Basic operations are supplied: Addition, subtraction,
*		 normalization, dot and cross products.
*
*		Precision is not a property of this abstraction.
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

function vec3_normalize(a:vec3)
{
	local len = vec3_length(a);
	a.x = fix_div(a.x, len);
	a.y = fix_div(a.y, len);
	a.z = fix_div(a.z, len);
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

function vec3_dot(a:vec3, b:vec3, out)
{
	*out = fix_mul(a.x, b.x) + fix_mul(a.y, b.y);
}

function vec3_cross(a:vec3, b:vec3, out:vec3)
{
	out.x = fix_mul(a.y, b.z) - fix_mul(a.z, b.y);
	out.y = fix_mul(a.z, b.x) - fix_mul(a.x, b.z);
	out.z = fix_mul(a.x, b.y) - fix_mul(a.y, b.x);
}

#endif
