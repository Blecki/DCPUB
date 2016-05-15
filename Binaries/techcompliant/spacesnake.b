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
*                           SPACE SNAKE
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

    static vram = __endofprogram; // __endofprogram is defined as the first word after the program's
                                    // compiled code. Any memory to be used must be allocated from 
                                    // this point to the stack, or you may overwrite parts of your 
                                    // program.

    // Lets find the hardware number for the display.
    local lem = detect_hardware(LEM_HARDWARE_ID); 

    // And go ahead and point it at our vram.
    lem_initialize(lem, vram);

    // We'll use special characters in the font for drawing the snake and the food. This will let
    // the game work on a 32*24 grid with square glyphs instead of the 32*12 lem grid with rectangular
    // glyphs. We will have to pack two game 'tiles' into a single lem glyph.
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

    // Set the font. No function for this in lem.b apparently?
    asm (B = font; C = lem)
    {
        SET A, 1
        HWI C
    }
}

// Need to get keyboard input.
local kb = detect_hardware(GENERIC_KEYBOARD_ID);

// This is our data for the game.
local game[screen_size];            // The game board. 0 = empty, 1 = snake, 0xFFFF = food.
local segment_buffer[screen_size];  // We will implement a circular buffer of x,y coordinates
                                    // of snake tail pieces. This will let us move the head
                                    // and remove the oldest tail piece in constant time.
local snake_head_segment_index = 0; // Index into segment_buffer where we've stored the snake's head.
local snake_x = 16;                 // X position of snake
local snake_y = 11;                 // Y position of snake
local heading_x = 0;                // X direction of snake
local heading_y = 0;                // y direction of snake
local segments = 0;                 // How many segments has our snake?

// Initialize game board and snake segment buffer.
// Locals are NOT initialized to 0 by B automatically.
local i = 0;
while (i < screen_size) 
{ 
    game[i] = 0x0000; 
    segment_buffer[i] = 0x0000;
    i += 1; 
}

// Now lets setup the initial game configuration.
game[(snake_y * screen_width) + snake_x] = 1;
game[(4 * screen_width) + 4] = 0xFFFF;
segment_buffer[0] = (snake_x << 8) + snake_y;

// Update the lem cell at x,y by choosing the appropriate glyph based on upper and lower.
function update_lem_cell(x, y, upper, lower)
{
    // Operator == always results in 0 or 1.
    // We shift lower's result < by 1 and add them to turn the value into an index
    // into our special font.
    local game_value = (upper == 1) +
        ((lower == 1) << 1);
                
    // Our font is laid out very logically. The first 4 glyphs are all possible combinations
    // within a lem cell of snake pieces. The next three rows add all possible permutations
    // containing a food piece. We must choose the appropriate glyph.
    if ((upper == 0xFFFF) & (lower == 0xFFFF)) game_value += 12;
    else if (upper == 0xFFFF) game_value += 8;
    else if (lower == 0xFFFF) game_value += 4;

    // Once we have the glyph, writing it into vram is straightforward.
    vram[(y * 32) + x] = 0xF000 | game_value;
}

// Update the lem cell associated with this game cell.
// This involves two game cells.
function update_lem(game, game_x, game_y)
{
    // First, convert the y coordinate from game-space to lem-space.
    // Game-space is exactly twice the size of lem-space on the y axis.
    local lem_y = game_y / 2;

    // Look up the game cells covered by this lem cell and pass them to update_lem_cell.
    update_lem_cell(
        game_x, 
        lem_y, 
        game[(lem_y * screen_width * 2) + game_x], 
        game[(lem_y * screen_width * 2) + game_x + screen_width]);
}

// Code exists at top scope in B. The entire file becomes a 'main' function. Functions defined within
// are pulled out and compiled separately. 
// Draw the board for the first time. After this, we will only draw what changes.
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

// Draw WASD instructions on the screen.
vram[(5 * 32) + 16] = 0xF000 | 'W';
vram[(6 * 32) + 14] = 0xF000 | 'A';
vram[(7 * 32) + 16] = 0xF000 | 'S';
vram[(6 * 32) + 18] = 0xF000 | 'D';

local playing = 1;

while (playing)
{
    // Check for input.
    local key = kb_getkey(kb);

    // Adjust the snake's heading based on input.
    if (key == 'w') { heading_x = 0; heading_y = -1; }
    else if (key == 'a') { heading_x = -1; heading_y = 0; }
    else if (key == 's') { heading_x = 0; heading_y = 1; }
    else if (key == 'd') { heading_x = 1; heading_y = 0; }
    
    // Move snake.
    snake_x += heading_x;
    snake_y += heading_y;

    // Wrap the snake when it goes off screen.
    if (snake_x == 0xFFFF) snake_x = (screen_width - 1);
    if (snake_x == screen_width) snake_x = 0;
    if (snake_y == 0xFFFF) snake_y = screen_height - 1;
    if (snake_y == screen_height) snake_y = 0;

    // Only process if we aren't still waiting for the first player input.
    if (heading_x | heading_y)
    {
        // Do we want to remove the last tail segment? If we just ate some food, we do not.
        local truncate_tail = 1;

        if (game[(snake_y * screen_width) + snake_x] == 0xFFFF) // Snake found some food.
        {
            truncate_tail = 0;
            segments += 1;

            //Position new random food piece.
            local new_spot = random() % screen_size;
            // Scan game starting at new_spot until we find an open spot.
            while (game[new_spot] != 0)
            {
                // Theoretically this could loop forever, but the player would have to fill the entire screen with snake..
                // which means he's about to lose anyway.
                new_spot += 1;
                if (new_spot == screen_size)
                    new_spot = 0;
            }
            game[new_spot] = 0xFFFF;

            // Draw our new food piece to the screen.
            update_lem(game, new_spot % 32, new_spot / 32);
        }
        else if (game[(snake_y * screen_width) + snake_x] != 0)
        {
            // Snake hit itself! Game over!
            playing = 0;
        }
        
        if (truncate_tail) // Remove last tail segment.
        {
            // Calculate tail position in circular buffer.
            local tail_pos = snake_head_segment_index - segments;
            if (snake_head_segment_index < segments) // If we've wrapped over the end of the buffer.
                tail_pos += screen_size;

            // Un-pack coordinate from segment buffer. It's stored in 8.8 format.
            local tx = segment_buffer[tail_pos] >> 8;
            local ty = segment_buffer[tail_pos] & 0x00FF;

            // Clear the tail from the gameboard..
            game[(ty * screen_width) + tx] = 0x0000;
            // ..and from the screen.
            update_lem(game, tx, ty);
        }

        // Add new head.
        snake_head_segment_index += 1;
        // We're storing the segment locations in a circular buffer, so we need to wrap if we've run out
        // of room.
        if (snake_head_segment_index == screen_size) snake_head_segment_index = 0;

        // Pack the coordinates into 8.8 format and store in the buffer.
        segment_buffer[snake_head_segment_index] = (snake_x << 8) + snake_y;

        // Now lets place the new head on the board..
        game[(snake_y * screen_width) + snake_x] = 1;
        // ..and on the screen.        
        update_lem(game, snake_x, snake_y);

        // Add a delay so the game is playable.
        local d = 0;
        while (d < 1000) d += 1;
    }
}

// Display gameover message.
draw_string(vram, 12, 5, "*GAME OVER*");

function draw_string(vram, x, y, string)
{
    local p = vram + (y * 32) + x;
    local e = string + *string + 1;     // B stores string literals as length - prefixed.
    string += 1; // Skip the length word.
    while (string != e)
    {
        *p = 0xF000 | *string;
        p += 1;
        string += 1;
    }
}

