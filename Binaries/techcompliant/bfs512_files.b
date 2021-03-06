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

// Functions for reading files from a disc formatted with the bfs512 file system.

#ifndef _BITWISE_LIB_BFS512_FILES_
#define _BITWISE_LIB_BFS512_FILES_

#include bfs512_raw.b
#include m35fd.b

#define BFS512_MODE_READ 1
#define BFS512_MODE_WRITE 2

#define BFS512_ERR_NONE 0
//#define M35FD_ERROR_BUSY			0x0001  //Avoid conflicting with m35fd error codes.
//#define M35FD_ERROR_NO_MEDIA 		0x0002
//#define M35FD_ERROR_PROTECTED		0x0003
//#define M35FD_ERROR_EJECT			0x0004
//#define M35FD_ERROR_BAD_SECTOR	0x0005
#define BFS512_ERR_WRONG_MODE 		0x0006
#define BFS512_ERR_DISC_FULL 		0x0007
#define BFS512_ERR_EOF 				0x0008
#define BFS512_ERR_UNKNOWN			0x0009

struct bfs512_FILE
{
	device; //The m35fd device this file resides on
	root_sector; //First sector of file.
	file_system:bfs512_SYSTEM_HEADER; //The filesystem this file belongs to 
	sector; //The current sector being referenced
	offset; //
	mode; 
	buffer[M35FD_SECTOR_SIZE]; //Contents of active sector
}

//Get the size of the file.
function bfs512_file_size(file:bfs512_FILE, b)
{
	local sector = file.file_system.link_table[file.root_sector];
	local sectors_scanned = 0;
	while ((sector & BFS512_END_OF_FILE_SENTINEL) == 0) 
	{
		sector = file.file_system.link_table[sector];
		sectors_scanned += 1;
		b();
	}
	return (sectors_scanned * M35FD_SECTOR_SIZE) + (sector & !BFS512_END_OF_FILE_SENTINEL);
}

//Open a file for reading
function bfs512_open_read(file:bfs512_FILE, file_system:bfs512_SYSTEM_HEADER, device, sector)
{
	file.device = device;
	file.file_system = file_system;
	file.sector = sector;
	file.root_sector = sector;
	file.offset = 0;
	file.mode = BFS512_MODE_READ;
	return m35fd_blocking_read(device, sector, file.buffer);
}

//Read size words from file into 'into'. 
function bfs512_read(file:bfs512_FILE, into, size)
{
	if ((file.mode & BFS512_MODE_READ) == 0) return BFS512_ERR_WRONG_MODE;
	local i = 0;
	while (i < size)
	{
		if (file.sector & BFS512_END_OF_FILE_SENTINEL) //This is last sector in file
		{
			local file_size = file.sector & !BFS512_END_OF_FILE_SENTINEL;
			if (file.offset == file_size) return BFS512_ERR_EOF;
		}

		if (file.offset == M35FD_SECTOR_SIZE) //Advance file to next sector.
		{
			local next_sector = file.file_system.link_table[file.sector];
			if (next_sector & BFS512_END_OF_FILE_SENTINEL) return BFS512_ERR_EOF;
			file.sector = next_sector;
			file.offset = 0;
			local err = m35fd_blocking_read(file.device, file.sector, file.buffer);
			if (err != BFS512_ERR_NONE) return err;
		}
		
		into[i] = file.buffer[file.offset];
		i += 1;
		file.offset += 1;
	}
	return BFS512_ERR_NONE;
}

//Creates a new file to write to. 
function bfs512_create_write(file:bfs512_FILE, file_system:bfs512_SYSTEM_HEADER, device)
{
	local first_sector = bfs512_find_free_sector(file_system);
	if (first_sector == 0xFFFF) return BFS512_ERR_DISC_FULL;
	bfs512_allocate_sector(file_system, first_sector);
	file_system.link_table[first_sector] = BFS512_END_OF_FILE_SENTINEL;
	bfs512_save_header(device, file_system); //Save changed header to disc.
	file.device = device;
	file.file_system = file_system;
	file.sector = first_sector;
	file.root_sector = first_sector;
	file.offset = 0;
	file.mode = BFS512_MODE_WRITE;
	return BFS512_ERR_NONE;
}

//Open a file for writing. Over-writes existing file.
function bfs512_open_write(file:bfs512_FILE, file_system:bfs512_SYSTEM_HEADER, device, sector)
{
	file.device = device;
	file.file_system = file_system;
	file.sector = sector;
	file.root_sector = sector;
	file.offset = 0;
	file.mode = BFS512_MODE_WRITE;

	return m35fd_blocking_read(file.device, file.sector, file.buffer);
}

//Open a file for reading and writing. Over-writes existing file.
function bfs512_open_read_write(file:bfs512_FILE, file_system:bfs512_SYSTEM_HEADER, device, sector)
{
	file.device = device;
	file.file_system = file_system;
	file.sector = sector;
	file.root_sector = sector;
	file.offset = 0;
	file.mode = BFS512_MODE_WRITE | BFS512_MODE_READ;
	return m35fd_blocking_read(file.device, file.sector, file.buffer);
}

//Write data to a file.
function bfs512_write(file:bfs512_FILE, what, size)
{
	if ((file.mode & BFS512_MODE_WRITE) != BFS512_MODE_WRITE) return BFS512_ERR_WRONG_MODE;
	local i = 0;
	while (i < size)
	{
		if (file.offset == M35FD_SECTOR_SIZE) //Buffer is full.
		{
			local err = m35fd_blocking_write(file.device, file.sector, file.buffer);
			if (err != BFS512_ERR_NONE) return err;
			local next_sector = file.file_system.link_table[file.sector];
			if (next_sector & BFS512_END_OF_FILE_SENTINEL)
			{
				next_sector = bfs512_find_free_sector(file.file_system);
				if (next_sector & BFS512_END_OF_FILE_SENTINEL) return BFS512_ERR_DISC_FULL; //Out of space
				bfs512_allocate_sector(file.file_system, next_sector);
				file.file_system.link_table[file.sector] = next_sector;
				file.file_system.link_table[next_sector] = BFS512_END_OF_FILE_SENTINEL;
				err = bfs512_save_header(file.device, file.file_system);
				if (err != BFS512_ERR_NONE) return err;
				//No need to load the new sector.
			}
			else
			{
				//Load next sector from disc.
				err = m35fd_blocking_read(file.device, file.sector, file.buffer);
				if (err != BFS512_ERR_NONE) return err;
			}
			file.sector = next_sector;
			file.offset =0;
		}

		file.buffer[file.offset] = what[i];
		i += 1;
		file.offset += 1;
	}

	// Write file size, if we've expanded the file.
	local next_sector = file.file_system.link_table[file.sector];
	if (next_sector & BFS512_END_OF_FILE_SENTINEL)
	{
		next_sector = BFS512_END_OF_FILE_SENTINEL | file.offset;
		file.file_system.link_table[file.sector] = next_sector;
		local err = bfs512_save_header(file.device, file.file_system);
		if (err != BFS512_ERR_NONE) return err;
	}

	return BFS512_ERR_NONE;
}

//Skip ahead in a write file.
function bfs512_seek(file:bfs512_FILE, distance)
{
	// By not checking the mode, this function can be used to seek in read files as well.
	//if (file.mode != BFS512_MODE_WRITE) return BFS512_ERR_WRONG_MODE;
	local crossed_sector = 0;
	while ((file.offset + distance) > M35FD_SECTOR_SIZE)
	{
		//If this is the first sector boundary crossed, and the file is a write file, it must be saved.
		if ( (crossed_sector == 0) & ( (file.mode & BFS512_MODE_WRITE) != 0) )
		{
			local err = m35fd_blocking_write(file.device, file.sector, file.buffer);
			if (err != BFS512_ERR_NONE) return err;
		}
		local next_sector = file.file_system.link_table[file.sector];
		if (next_sector & BFS512_END_OF_FILE_SENTINEL) return BFS512_ERR_EOF;
		file.sector = next_sector;
		distance -= M35FD_SECTOR_SIZE;
		crossed_sector = 1;
	}

	if (file.sector & BFS512_END_OF_FILE_SENTINEL) //This is last sector in file
	{
		local file_size = file.sector & !BFS512_END_OF_FILE_SENTINEL;
		if ((file.offset + distance) > file_size) return BFS512_ERR_EOF;
	}

	file.offset += distance;
	//If a sector boundary was crossed, the file buffer needs to be reloaded from disc.
	if (crossed_sector == 1) return m35fd_blocking_read(file.device, file.sector, file.buffer);
	return BFS512_ERR_NONE;
}

//Flush a write file
function bfs512_flush(file:bfs512_FILE)
{
	if ((file.mode & BFS512_MODE_WRITE) == 0) return BFS512_ERR_WRONG_MODE;
	return m35fd_blocking_write(file.device, file.sector, file.buffer);
}

//Delete a file. This doesn't actually modify the file; it just sets it's sectors as free
// in the free_mask and unlinks them.
function bfs512_free_file_chain(file_system:bfs512_SYSTEM_HEADER, device, sector)
{
	while ((sector & BFS512_END_OF_FILE_SENTINEL) == 0)
	{
		bfs512_free_sector(file_system, sector);
		local next_sector = file_system.link_table[sector];
		file_system.link_table[sector] = BFS512_END_OF_FILE_SENTINEL;
		sector = next_sector;
	}
	return bfs512_save_header(device, file_system);
}

#endif
