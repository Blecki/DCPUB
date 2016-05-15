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
*		                     SPACE SNAKE
*
*                     A PRIMER ON B FUNDAMENTALS
*/

#include hardware.b
#include lem.b
#include keyboard.b
#include random.b

#define screen_width 32
#define screen_height 24
#define screen_size 768

{ // Setup LEM display.

    static vram = __endofprogram;
    local lem = detect_hardware(LEM_HARDWARE_ID);
    lem_initialize(lem, vram);

    static font[256] = {  
        // Special glyphs for drawing the snake and food.
        0x0000, 0x0000, 0x0F0F, 0x0F0F, 0xF0F0, 0xF0F0, 0xFFFF, 0xFFFF, 
        0x0060, 0x6000, 0x0F6F, 0x6F0F, 0x0000, 0x0000, 0x0000, 0x0000,
        0x0006, 0x0600, 0x0000, 0x0000, 0xF0F6, 0xF6F0, 0x0000, 0x0000,
        0x0066, 0x6600, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,

        // The rest is just the default lem font.
        0x0F08, 0x0F08, 0x14F4, 0x1414, 0xF808, 0xF808, 0x0F08, 0x0F08,
        0x001F, 0x1414, 0x00FC, 0x1414, 0xF808, 0xF808, 0xFF08, 0xFF08,
        0x14FF, 0x1414, 0x080F, 0x0000, 0x00F8, 0x0808, 0xFFFF, 0xFFFF,
        0xF0F0, 0xF0F0, 0xFFFF, 0x0000, 0x0000, 0xFFFF, 0x0F0F, 0x0F0F,
        0x0000, 0x0000, 0x005f, 0x0000, 0x0300, 0x0300, 0x3e14, 0x3e00,
        0x266b, 0x3200, 0x611c, 0x4300, 0x3629, 0x7650, 0x0002, 0x0100,
        0x1c22, 0x4100, 0x4122, 0x1c00, 0x1408, 0x1400, 0x081c, 0x0800,
        0x4020, 0x0000, 0x0808, 0x0800, 0x0040, 0x0000, 0x601c, 0x0300,
        0x3e49, 0x3e00, 0x427f, 0x4000, 0x6259, 0x4600, 0x2249, 0x3600,
        0x0f08, 0x7f00, 0x2745, 0x3900, 0x3e49, 0x3200, 0x6119, 0x0700,
        0x3649, 0x3600, 0x2649, 0x3e00, 0x0024, 0x0000, 0x4024, 0x0000,
        0x0814, 0x2200, 0x1414, 0x1400, 0x2214, 0x0800, 0x0259, 0x0600,
        0x3e59, 0x5e00, 0x7e09, 0x7e00, 0x7f49, 0x3600, 0x3e41, 0x2200,
        0x7f41, 0x3e00, 0x7f49, 0x4100, 0x7f09, 0x0100, 0x3e41, 0x7a00,
        0x7f08, 0x7f00, 0x417f, 0x4100, 0x2040, 0x3f00, 0x7f08, 0x7700,
        0x7f40, 0x4000, 0x7f06, 0x7f00, 0x7f01, 0x7e00, 0x3e41, 0x3e00,
        0x7f09, 0x0600, 0x3e61, 0x7e00, 0x7f09, 0x7600, 0x2649, 0x3200,
        0x017f, 0x0100, 0x3f40, 0x7f00, 0x1f60, 0x1f00, 0x7f30, 0x7f00,
        0x7708, 0x7700, 0x0778, 0x0700, 0x7149, 0x4700, 0x007f, 0x4100,
        0x031c, 0x6000, 0x417f, 0x0000, 0x0201, 0x0200, 0x8080, 0x8000,
        0x0001, 0x0200, 0x2454, 0x7800, 0x7f44, 0x3800, 0x3844, 0x2800,
        0x3844, 0x7f00, 0x3854, 0x5800, 0x087e, 0x0900, 0x4854, 0x3c00,
        0x7f04, 0x7800, 0x047d, 0x0000, 0x2040, 0x3d00, 0x7f10, 0x6c00,
        0x017f, 0x0000, 0x7c18, 0x7c00, 0x7c04, 0x7800, 0x3844, 0x3800,
        0x7c14, 0x0800, 0x0814, 0x7c00, 0x7c04, 0x0800, 0x4854, 0x2400,
        0x043e, 0x4400, 0x3c40, 0x7c00, 0x1c60, 0x1c00, 0x7c30, 0x7c00,
        0x6c10, 0x6c00, 0x4c50, 0x3c00, 0x6454, 0x4c00, 0x0836, 0x4100,
        0x0077, 0x0000, 0x4136, 0x0800, 0x0201, 0x0201, 0x0205, 0x0200 };

    // Set the font.
    asm (B = font; C = lem)
    {
        SET A, 1
        HWI C
    }

}

local kb = detect_hardware(GENERIC_KEYBOARD_ID);

local game[screen_size];
local segment_buffer[screen_size];
local snake_head_segment_index = 0;
local snake_x = 16;
local snake_y = 11;
local heading_x = 0;
local heading_y = 0;
local segments = 0;

local i = 0;

// Initialize game board and snake segment buffer.
while (i < screen_size) 
{ 
    game[i] = 0x0000; 
    segment_buffer[i] = 0x0000;
    i += 1; 
}

game[(snake_y * screen_width) + snake_x] = 1;
game[(4 * screen_width) + 4] = 0xFFFF;
segment_buffer[0] = (snake_x << 8) + snake_y;

function update_lem_cell(x, y, upper, lower)
{
    local game_value = ((upper > 0) & (upper != 0xFFFF)) +
        (((lower > 0) & (lower != 0xFFFF)) * 2);
                
    if ((upper == 0xFFFF) & (lower == 0xFFFF)) game_value += 12;
    else if (upper == 0xFFFF) game_value += 8;
    else if (lower == 0xFFFF) game_value += 4;

    vram[(y * 32) + x] = 0xF000 | game_value;
}

// Update the lem cell associated with this game cell.
// This involves two game cells.
function update_lem(game, game_x, game_y)
{
    local lem_y = game_y / 2;

    update_lem_cell(
        game_x, 
        lem_y, 
        game[(lem_y * screen_width * 2) + game_x], 
        game[(lem_y * screen_width * 2) + game_x + screen_width]);
}

// Initial board render
local x = 0;
while (x < 32)
{
    local y = 0;
    while (y < 12)
    {
        update_lem_cell(x, y, game[(y * screen_width * 2) + x], game[(y * screen_width * 2) + x + screen_width]);
        y += 1;
    }
    x += 1;
}

local playing = 1;

while (playing)
{
    // Check for input.
    if (kb_query(kb, 'w')) { heading_x = 0; heading_y = -1; }
    else if (kb_query(kb, 'a')) { heading_x = -1; heading_y = 0; }
    else if (kb_query(kb, 's')) { heading_x = 0; heading_y = 1; }
    else if (kb_query(kb, 'd')) { heading_x = 1; heading_y = 0; }
    
    // Move snake.
    snake_x += heading_x;
    snake_y += heading_y;

    if (snake_x == 0xFFFF) snake_x = (screen_width - 1);
    if (snake_x == screen_width) snake_x = 0;
    if (snake_y == 0xFFFF) snake_y = screen_height - 1;
    if (snake_y == screen_height) snake_y = 0;

    // Only process if we aren't still waiting for the first player input.
    if (heading_x | heading_y)
    {

        local truncate_tail = 1;

        if (game[(snake_y * screen_width) + snake_x] == 0xFFFF) // Snake found some food.
        {
            truncate_tail = 0;
            segments += 1;

            //Position new random food piece.
            local new_spot = random() % screen_size;
            while (game[new_spot] != 0)
            {
                new_spot += 1;
                if (new_spot == screen_size)
                    new_spot = 0;
            }
            game[new_spot] = 0xFFFF;
            update_lem(game, new_spot % 32, new_spot / 32);
        }
        else if (game[(snake_y * screen_width) + snake_x] != 0) // Snake hit itself.
            playing = 0;
        
        if (truncate_tail) // Remove last tail segment.
        {
            // Calculate tail position in circular buffer.
            local tail_pos = snake_head_segment_index - segments;
            if (snake_head_segment_index < segments) // If we've wrapped over the end of the buffer.
                tail_pos += screen_size;
            local tx = segment_buffer[tail_pos] >> 8;
            local ty = segment_buffer[tail_pos] & 0x00FF;
            game[(ty * screen_width) + tx] = 0x0000;
            update_lem(game, tx, ty);
        }

        // Add new head.
        snake_head_segment_index += 1;
        if (snake_head_segment_index == screen_size) snake_head_segment_index = 0;
        segment_buffer[snake_head_segment_index] = (snake_x << 8) + snake_y;
        game[(snake_y * screen_width) + snake_x] = 1;        
        update_lem(game, snake_x, snake_y);

        // Add a delay so the game is playable.
        local d = 0;
        while (d < 1000) d += 1;
    }
}

// Display gameover message.
static gameover_message = "*GAME OVER*";
local p = vram + (5 * 32) + 12;
i = 0;
while (i < *gameover_message)
{
    *p = 0xF000 | gameover_message[i + 1];
    p += 1;
    i += 1;
}

