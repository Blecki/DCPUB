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

/* bfs512: Bitwise File System 512

	A simple file system for m35fd compatible discs. 
	- Track free sectors
	- Link sectors to form larger files.
*/

#ifndef _BITWISE_LIB_BFS512_RAW_
#define _BITWISE_LIB_BFS512_RAW_

#include m35fd.b

/* The structure of a bfs512 disc.

A) Header [6 words]
B) Free Mask [1440 bits or 90 words]
C) File Allocation Table [ 1440 words]
D) File data

Total filesystem overhead : 1536 words or 3 sectors.

The first sector used is sector 1 - Sector 0 is boot sector.

*/

#define BFS512_VERSION 0xBF55
#define BFS512_SECTOR_WORDS (1440 / 16)

#define BFS512_END_OF_FILE_SENTINEL 0x8000

// File system header. Allocate one of these and keep it for interacting with the filesystem.
struct bfs512_SYSTEM_HEADER
{
	version;
	reserved[5];
	free_mask[BFS512_SECTOR_WORDS];
	link_table[M35FD_SECTOR_COUNT];
}

//Load a disc header from an m35fd device.
function bfs512_load_disc(device, header:bfs512_SYSTEM_HEADER /* Pointer to sizeof(bfs512_SYSTEM_HEADER) words of memory.*/)
{
	m35fd_blocking_read(device, 1, header);
	m35fd_blocking_read(device, 2, header + M35FD_SECTOR_SIZE);
	m35fd_blocking_read(device, 3, header + (M35FD_SECTOR_COUNT * 2));
}

//Find free sector
function bfs512_find_free_sector(header:bfs512_SYSTEM_HEADER)
{
	local word = 0;
	while (word < BFS512_SECTOR_WORDS)
	{
		if (header.free_mask[word] != 0)
		{
			//Which bit is free?
			local bit = 0;
			while (bit < 8)
			{
				local mask = 1 << bit;
				if ((header.free_mask[word] & mask) > 1) return (word * 16) + bit;
				bit += 1;
			}
			//Unreachable
		}
		word += 1;
	}
	return 0xFFFF; //No free space.
}

//Allocate sector
#define bfs512_allocate_sector(header, sector) header[6 + (sector / 16)] &= !(1 << (sector % 16));
#define bfs512_free_sector(header, sector) header[6 + (sector / 16)] |= (1 << (sector % 16));

//Save header back to the device. If files are modified and the disc is removed before this header is saved,
//files on the disc may be corrupted.
function bfs512_save_header(device, header)
{
	m35fd_blocking_write(device, 1, header);
	m35fd_blocking_write(device, 2, header + M35FD_SECTOR_SIZE);
	m35fd_blocking_write(device, 3, header + (M35FD_SECTOR_SIZE * 2));
}

//Format a header block. To format a disc, allocate a header, call this function, and then save the header. 
function bfs512_format_header(header, bootable)
{
	header[0] = BFS512_VERSION;

	// Blank out next five words as reserved.
	local i = 1;
	while (i < 6)
	{
		header[i] = 0x0000;
		i += 1;
	}

	// Mark first 4 sectors as used.
	if (bootable != 0)
		header[i] = 0b1111111111110000;
	else
		header[i] = 0b1111111111110001;
	i += 1;

	//Prime free_mask
	while (i < (6 + BFS512_SECTOR_WORDS))
	{
		header[i] = 0xFFFF;
		i += 1;
	}

	//Prime sector links
	while (i < (M35FD_SECTOR_SIZE * 3))
	{
		header[i] = BFS512_END_OF_FILE_SENTINEL;
		i += 1;
	}
}

#endif
