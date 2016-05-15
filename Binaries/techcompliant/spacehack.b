/*
*    _________.________________.      __.___  ____________________
*    \______   \   \__    ___/  \    /  \   |/   _____/\_   _____/
*     |    |  _/   | |    |  \   \/\/   /   |\_____  \  |    __)_ 
*     |    |   \   | |    |   \        /|   |/        \ |        \ 
*     |______  /___| |____|    \__/\  / |___/_______  //_______  /
*            \/                     \/              \/         \/ 
*
*                (c) BITWISE SCIENCES 1984 - 2108
*
*		                     SPACE HACK
*/

#include lem.b
#include keyboard.b

// Initialize memory.

// Give us 3 screens worth of world to start.
// Using static variables will improve access times slightly.
static world_size = LEM_VRAM_SIZE * 3;
static world = __endofprogram;
static available_memory_start;
available_memory_start = world + world_size;

static player_x = 16;
static player_y = 6;

static lem;
lem = lem_detect();

function position_camera()
{
	// Center the player on the screen.
	local offset_row = 0;
	if (player_y > 6)
		offset_row = player_y - 6;
	if (player_y > 30)
		offset_row = 24;

	// Hacky McHackface.
	lem_initialize(lem, world + (offset_row * 32));
}

function set_character(x, y, c, color)
{
	world[(y * 32) + x] = color | c;
}


// Initialize input.
static kb;
kb = kb_detect();

while (true)
{
	set_character(player_x, player_y, ' ', 0xF000);

	local input = kb_getkey(kb);

	if ((input == 'w') & (player_y > 0))
		player_y -= 1;
	if ((input == 'a') & (player_x > 0))
		player_x -= 1;
	if ((input == 'd') & (player_x < 31))
		player_x += 1;
	if ((input == 's') & (player_y < 35))
		player_y += 1;

	set_character(player_x, player_y, '@', 0xF000);
	position_camera();
}

