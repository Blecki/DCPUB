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

//A 'friendly' interface to the bfs512 file system.

#ifndef _BITWISE_LIB_BFS512_
#define _BITWISE_LIB_BFS512_

#include m35fd.b
#include bfs512_raw.b
#include bfs512_files.b
#include bfs512_directories.b
#include vec.b
#include memory.b

// This stuff rightly belongs to an OS. The filesystem really should only worry about a single disc at a time.
static bfs512_drives[4] = { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF };
static bfs512_num_drives = 0;

function bfs512_initialize(allocator)
{
	bfs512_num_drives = m35fd_enumerate(bfs512_drives, 4);
	local i = 0;
	while (i < bfs512_num_drives)
	{
		local drive = bfs512_drives[i];
		bfs512_drives[i] = allocate_memory(sizeof bfs512_SYSTEM_HEADER, allocator);
		bfs512_load_disc(drive, bfs512_drives[i]);
		bfs512_drives[i][1] = drive; //Store drive ID in unused portion of file system header.
	}
}

function bfs512_search_path(path)
{
	local name_buffer[16];
	local packed_name[8];
	local bi = 0;
	local i = 0;
	local pl = veclen(path);
	local directory:bfs512_OPEN_DIRECTORY[sizeof bfs512_OPEN_DIRECTORY];
	local entry:bfs512_DIRECTORY_ENTRY[sizeof bfs512_DIRECTORY_ENTRY];
	entry.sector = 4;

	//Detect drive letter to setup disc/file_system.
	local file_system = bfs512_drives[0];
	local disc = file_system[1];

	bfs512_open_directory(file_system, disc, 3, directory);

	while (i < pl)
	{
		bi = 0;
		while ((i < pl) & (bi < 16) & (path[i + 1] != '\\'))
		{
			name_buffer[bi] = path[i + 1];
			bi += 1;
			i += 1;
		}

		if (bi == 0) goto END_SCAN; //Found an empty chunk.
		if ((i < pl) & (path[i + 1] != '\\')) return 0xFFFF;
		i += 1; //Skip \\

		bfs512_pack_filename(name_buffer, packed_name);

		while (directory.children_left > 0)
		{
			bfs512_read_directory_entry(directory, entry);
			if (bfs512_compare_filenames(packed_name, entry.name))
			{
				if (i < pl) 
				{
					if (entry.type != BFS512_DE_DIRECTORY) return 0xFFFF;
					bfs512_open_directory(file_system, disc, entry.sector, directory);
					goto END_SCAN;
				}
				else
				{
					return entry.sector;
				}
			}
		}
		:END_SCAN
	}

	return 0xFFFF;
}



#endif
