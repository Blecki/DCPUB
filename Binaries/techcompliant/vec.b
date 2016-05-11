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

#ifndef _BITWISE_LIB_VEC_
#define _BITWISE_LIB_VEC_

#include memory.b

//Vecs have a one-word header with the available capacity and the used space stored in 8 bits each.

struct vec
{
	header;
	data;
}

function veclen(string:vec) 
{ 
	return string.header & 0x00FF;
}

function veccap(string:vec)
{
	return (string.header & 0xFF00) >> 8;
}

function veccpy(from:vec, to:vec)
{
	local i;
	while (i < veccap(to) && i < veclen(from))
	{
		to[1 + i] = from[1 + i];
		i += 1;
	}
	local to_cap = veccap(to);
	if (veclen(from) > to_cap)
		to.header = ((to_cap & 0x00FF) << 8) + (to_cap & 0x00FF);
	else
		to.header = ((to_cap & 0x00FF) << 8) + (veclen(from) & 0x00FF);
}

function veccat(from:vec, to:vec)
{
	local total_length = veclen(from) + veclen(to);
	local i = 0;
	while (i + veclen(to) < veccap(to) && i < veclen(from))
	{
		to[1 + veclen(to) + i] = from[1 + i];
		i += 1;
	}
	local to_cap = veccap(to);
	if (total_length > to_cap)
		to.header = ((to_cap & 0x00FF) << 8) + (to_cap & 0x00FF);
	else
		to.header = ((to_cap & 0x00FF) << 8) + (total_length & 0x00FF);
}
	
function vecalo(capacity, page)
{
	local mem_block:vec = allocate_memory(capacity + 1, page);
	if (mem_block != 0)
		mem_block.header = (capacity & 0x00FF) << 8;
	return mem_block;
}


#endif