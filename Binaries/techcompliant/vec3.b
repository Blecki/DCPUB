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

struct vector3
{
	x;
	y;
	z;
}

function vec3_lengthsquared(a:vec3)
{
	return (a.x * a.x) + (a.y * a.y) + (a.z * a.z);
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
	return (a.x * b.x) + (a.y * b.y);
}