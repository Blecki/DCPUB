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

// Functions for reading directories on a bsf512 formatted disc

#ifndef _BITWISE_LIB_BFS512_DIRECTORIES_
#define _BITWISE_LIB_BFS512_DIRECTORIES_

#include m35fd.b
#include bfs512_raw.b
#include bfs512_files.b
#include vec.b

#define BFS512_DE_DIRECTORY 0
#define BFS512_DE_FILE 1

struct bfs512_DIRECTORY_ENTRY
{
	type;
	sector;
	name[8];
}

struct bfs512_DIRECTORY_HEADER
{
	version;
	child_count;
}

struct bfs512_OPEN_DIRECTORY
{
	children_left;
	file:bfs512_FILE[sizeof bfs512_FILE];
}

//Given a string of up to 16 characters and a buffer of exactly 8 words, pack the string into the buffer.
function bfs512_pack_filename(filename, into)
{
	local fi = 0;
	while (fi < 8)
	{
		into[fi] = ((filename[fi * 2] & 0x00FF) << 8) + (filename[(fi * 2) + 1] & 0x00FF);
		fi += 1;
	}
}

function bfs512_unpack_filename(filename, into)
{
	local fi = 0;
	while (fi < 8)
	{
		into[fi * 2] = filename[fi] >> 8;
		into[(fi * 2) + 1] = filename[fi] & 0x00FF;
		fi += 1;
	}
}

//Compare two packed filenames.
function bfs512_compare_filenames(d, f)
{
	local i = 0;
	while (i < 8)
	{
		if (d[i] != d[i]) return 0;
		i += 1;
	}
	return 1;
}

//Open a directory so that entrys can be read.
function bfs512_open_directory(file_system:bfs512_SYSTEM_HEADER, drive, sector, out:bfs512_OPEN_DIRECTORY)
{
	local err = bfs512_open_read(out.file, file_system, drive, sector);
	if (err != BFS512_ERR_NONE) return err;
	local header:bfs512_DIRECTORY_HEADER[sizeof bfs512_DIRECTORY_HEADER];
	bfs512_read(out.file, header, sizeof bfs512_DIRECTORY_HEADER);
	out.children_left = header.child_count;
	return BFS512_ERR_NONE;
}

//Create a new directory on disc.
function bfs512_create_directory(file_system:bfs512_SYSTEM_HEADER, drive, sector)
{
	local _file:bfs512_FILE[sizeof bfs512_FILE];
	local err = bfs512_open_write(_file, file_system, drive, sector);
	if (err != BFS512_ERR_NONE) return err;;
	local header:bfs512_DIRECTORY_HEADER[sizeof bfs512_DIRECTORY_HEADER];
	header.child_count = 0;
	err = bfs512_write(_file, header, sizeof bfs512_DIRECTORY_HEADER);
	if (err != BFS512_ERR_NONE) return err;
	err = bfs512_flush(_file);
	if (err != BFS512_ERR_NONE) return err;
	return BFS512_ERR_NONE;
}

//Read the next directory entry in the directory.
function bfs512_read_directory_entry(directory:bfs512_OPEN_DIRECTORY, out:bfs512_DIRECTORY_ENTRY)
{
	if (directory.children_left == 0) return BFS512_ERR_EOF;
	directory.children_left -= 1;
	return bfs512_read(directory.file, out, sizeof bfs512_DIRECTORY_ENTRY);
}

//Add a new entry to a directory file.
function bfs512_append_to_directory(file_system:bfs512_SYSTEM_HEADER, drive, sector, what:bfs512_DIRECTORY_ENTRY)
{
	local _file:bfs512_FILE[sizeof bfs512_FILE];
	bfs512_open_read_write(_file, file_system, drive, sector);

	//Increment child_count
	local child_count = (_file.buffer):bfs512_DIRECTORY_HEADER.child_count;
	(_file.buffer):bfs512_DIRECTORY_HEADER.child_count += 1;

	//Seek to end of directory data. This will flush the first sector if the directory is larger than one sector.
	bfs512_seek(_file, (sizeof bfs512_DIRECTORY_HEADER) + (sizeof bfs512_DIRECTORY_ENTRY * child_count));

	//Write the new entry.
	bfs512_write(_file, what, sizeof bfs512_DIRECTORY_ENTRY);
	bfs512_flush(_file);
}

//Remove an entry from a directory file. If the entry specified is >= child_count, this will explode.
// This takes an entry index. To get it, scanning won't do. Use read_directory_entry and count until
// you find the entry you want to remove.
function bfs512_remove_from_directory(file_system:bfs512_SYSTEM_HEADER, drive, sector, entry_index)
{
	local _file:bfs512_FILE[sizeof bfs512_FILE];
	local temp_entry:bfs512_DIRECTORY_ENTRY[sizeof bfs512_DIRECTORY_ENTRY];
	bfs512_open_read_write(_file, file_system, drive, sector);

	//Decrement child count.
	local child_count = (_file.buffer):bfs512_DIRECTORY_HEADER.child_count;
	(_file.buffer):bfs512_DIRECTORY_HEADER.child_count -= 1;

	//Find and read the last directory entry.
	bfs512_seek(_file, sizeof bfs512_DIRECTORY_HEADER + (sizeof bfs512_DIRECTORY_ENTRY * (child_count - 1)));
	bfs512_read(_file, temp_entry, sizeof bfs512_DIRECTORY_ENTRY);

	//If the file is only one sector long, seeking didn't flush it. Make sure it gets flushed.
	if (child_count > (((M35FD_SECTOR_SIZE - sizeof bfs512_DIRECTORY_HEADER) / sizeof bfs512_DIRECTORY_ENTRY)))
		bfs512_flush(_file);

	//Return to begining of file.
	bfs512_open_write(_file, file_system, drive, sector);
	//Write the last entry over the entry to be removed.
	bfs512_seek(_file, sizeof bfs512_DIRECTORY_HEADER + (sizeof bfs512_DIRECTORY_ENTRY * entry_index));
	bfs512_write(_file, temp_entry, sizeof bfs512_DIRECTORY_ENTRY);
	bfs512_flush(_file); //Wrote to end of file. It -must- be flushed.

	//If removing the last entry shrunk the directory enough to discard a sector; discard it.
	local original_size = (sizeof bfs512_DIRECTORY_HEADER + (sizeof bfs512_DIRECTORY_ENTRY * child_count)) 
		/ M35FD_SECTOR_SIZE;
	local new_size = (sizeof bfs512_DIRECTORY_HEADER + (sizeof bfs512_DIRECTORY_ENTRY * (child_count - 1))) 
		/ M35FD_SECTOR_SIZE;
	if (new_size < original_size)
	{
		//Find the end of the file.
		local previous_sector = sector;
		local current_sector = sector;
		while ((current_sector & BFS512_END_OF_FILE_SENTINEL) == 0)
		{
			//If current_sector is linked to 0xFFFF, it is the last sector. Link the previous sector to 0xFFFF.
			local next_sector = file_system.link_table[current_sector];
			if (next_sector & BFS512_END_OF_FILE_SENTINEL)
				file_system.link_table[previous_sector] = BFS512_END_OF_FILE_SENTINEL; //This loses the size value 
			previous_sector = current_sector;
			current_sector = next_sector;
		}
	}

}



#endif
