// Testing Bleos File System 512

#include m35fd.b
#include bfs512_raw.b
#include bfs512_files.b
#include bfs512_directories.b
#include default_environment.b

#define CERROR(msg) if (err != 0) { printf(msg); printf(": EC %N\n", err); goto ERROR; }


local err = M35FD_ERROR_NONE;
local disc = 0;
local drives_found = m35fd_enumerate(&disc, 1); //Detect first device.
printf("Found %N m35fd devices.\n", drives_found);
if (drives_found == 0)
{
	printf("No drive found.\n");
	goto ERROR;
}

printf("Detected disc with id %N\n", disc);

local raw_buff[M35FD_SECTOR_SIZE];
local i = 0;
while (i < M35FD_SECTOR_SIZE)
{
	raw_buff[i] = i;
	i += 1;
}
err = m35fd_blocking_write(disc, 0, raw_buff);
CERROR("Raw write failed.");

local file_system:bfs512_SYSTEM_HEADER = malloc(sizeof bfs512_SYSTEM_HEADER);
local file:bfs512_FILE = malloc(sizeof bfs512_FILE);
local directory:bfs512_OPEN_DIRECTORY = malloc(sizeof bfs512_OPEN_DIRECTORY);
local entry:bfs512_DIRECTORY_ENTRY = malloc(sizeof bfs512_DIRECTORY_ENTRY);

if ((file_system == 0) | ((file == 0) | ((directory == 0) | (entry == 0))))
{
	printf("Error allocating memory.\n");
	printf("%N, %N, %N, %N\n", file_system, file, directory, entry);
	goto ERROR;
}

local sp = 0;
asm (A = &sp) { SET [A], SP }
printf("Memory used: %N words\n", entry + (sizeof bfs512_DIRECTORY_ENTRY) + (0xFFFF - sp));


bfs512_format_header(file_system, 0);


//Create root directory.
bfs512_allocate_sector(file_system, 4);

err = bfs512_create_directory(file_system, disc, 4);
CERROR("Fail create root");

err = bfs512_save_header(disc, file_system);
CERROR("Fail save header");

err = bfs512_create_write(file, file_system, disc);
CERROR("Fail create file");
printf("WRITE SECTOR %N\n", file.sector);

local _data = "Hello World!";
err = bfs512_write(file, _data, (1 + _data[0]));
CERROR("Fail write file");

err = bfs512_flush(file);
CERROR("Fail flush file");

function b() { printf("*\n"); }

err = bfs512_open_read(file, file_system, disc, file.sector); // Open the file we just wrote.
CERROR("Fail open read");
printf("READ SECTOR %N\n", file.sector);

local file_size = bfs512_file_size(file, &b);
printf("File Size: %N\n", file_size);

entry.type = BFS512_DE_FILE;
entry.sector = file.sector;
bfs512_pack_filename("helloworld!00000" + 1, entry.name);

err = bfs512_append_to_directory(file_system, disc, 4, entry);
CERROR("Fail add to dir");

err = bfs512_open_directory(file_system, disc, 4, directory);
CERROR("Fail open dir");

printf("Root Children: %N\n", directory.children_left);
while (directory.children_left > 0)
{
	err = bfs512_read_directory_entry(directory, entry);
	CERROR("Fail read entry");
	local _buff[17];
	bfs512_unpack_filename(entry.name, _buff + 1);
	_buff[0] = 16;
	printf(_buff);
	printf("\n");
}



printf("Finished tests.\n");

:ERROR

