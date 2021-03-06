
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

#ifndef _BITWISE_LIB_MEMORY_
#define _BITWISE_LIB_MEMORY_

//Uses a simple free list to track available memory.

struct free_block
{
	size;
	next_free_block;
}

//Initialize a section of memory to be used as a page.	
function initialize_memory_page(start, size)
{
	local free_list_head:free_block = start + 1;
	*start = start + 1;
	free_list_head.size = size - 1;
	free_list_head.next_free_block = 0;
}

//Allocate memory from a page.
function allocate_memory(size, page)
{
	if (size == 0) return 0;
	local current_block:free_block = *page;
	local previous_block:free_block = 0;
	local final_size = size + 1;
	while ((current_block != 0) & (current_block.size < final_size))
	{
		previous_block = current_block;
		current_block = current_block.next_free_block;
	}
	if (current_block == 0) return 0; //No block big enough found.
	if (current_block.size < (final_size + 2)) //Not enough space left to split the block - waste the last word.
	{
		if (previous_block == 0) *page = current_block.next_free_block;
		else previous_block.next_free_block = current_block.next_free_block;
		return current_block + 1;
	}
	local new_free_block:free_block = current_block + final_size; //Don't need to worry about overwriting the current block since final_size is always >= 2.
	new_free_block.size = current_block.size - final_size;
	new_free_block.next_free_block = current_block.next_free_block;
	if (previous_block == 0) *page = new_free_block;
	else previous_block.next_free_block = new_free_block;
	current_block.size = final_size;
	return current_block + 1;
}

//Free memory allocated from a page.
function free_memory(block, page)
{
	//Assume the block has been returned to the correct page. If not - kaboom.
	local memory_block:free_block = block - 1;
	memory_block.next_free_block = *page;
	*page = memory_block;
}

#endif
