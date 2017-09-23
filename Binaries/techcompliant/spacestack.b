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
*                           SPACE STACK
*
*                     A PRIMER ON B FUNDAMENTALS
*/

#include hardware.b
#include lem.b
#include keyboard.b
#include std.b
#include itoa.b
#include random.b

#define screen_width 13
#define screen_height 24
#define screen_size (13 * 24)

{ // Setup LEM display.

    static vram = __endofprogram; 
    local lem = detect_hardware(LEM_HARDWARE_ID); 
    lem_initialize(lem, vram);

    static font[256] = {  
        // Special glyphs for drawing tets - 3 styles, in two positions.
        
        // 0 none           1 solid          2 border         3 round
        // 00000000 0x0000  00001111 0x0F0F  00001111 0x0F09  00000110 0x060F
        // 00000000         00001111         00001001         00001111
        // 00000000 0x0000  00001111 0x0F0F  00001001 0x090F  00001111 0x0F06
        // 00000000         00001111         00001111         00000110

        /* 0000 00 0 */ 0x0000, 0x0000, 
        /* 0001 01 2 */ 0xF0F0, 0xF0F0, 
        /* 0010 02 2 */ 0xF090, 0x90F0,
        /* 0011 03 3 */ 0x60F0, 0xF060, 
        /* 0100 10 4 */ 0x0F0F, 0x0F0F, 
        /* 0101 11 5 */ 0xFFFF, 0xFFFF,
        /* 0110 12 6 */ 0xFF9F, 0x9FFF, 
        /* 0111 13 7 */ 0x6FFF, 0xFF6F, 
        /* 1000 20 8 */ 0x0F09, 0x090F, 
        /* 1001 21 9 */ 0xFFF9, 0xF9FF, 
        /* 1010 22 A */ 0xFF99, 0x99FF, 
        /* 1011 23 B */ 0x6FF9, 0xF96F, 
        /* 1100 30 C */ 0x060F, 0x0F06, 
        /* 1101 31 D */ 0xF6FF, 0xFFF6, 
        /* 1110 32 E */ 0xF69F, 0x9FF6, 
        /* 1111 33 F */ 0x66FF, 0xFF66,

        // vertical bar drawing chars
        // 0x0010       0x0011
        0xFF00, 0xFF00, 0x00FF, 0x00FF,

        // Background 
        // 0x0012       0x0013          0x0014          0x0015
        0xAA55, 0xAA55, 0x0000, 0x0000, 0x0A05, 0x0A05, 0xA050, 0xA050,

        // The rest is just the default lem font.
                                        0xF808, 0xF808, 0xFF08, 0xFF08,
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

// A single 16 bit value can describe a 4x4 tetrimino - more than large enough 
// for them.

static tets[19] = {
// 0 I     1 Z     2 S     3 T     4 L     5 <L    6 O
// 1111    1100    0110    1000    1000    0100    1100
// 0000    0110    1100    1100    1000    0100    1100
// 0000    0000    0000    1000    1100    1100    0000
// 0000    0000    0000    0000    0000    0000    0000
   0xF000, 0xC600, 0x6C00, 0x8C80, 0x88C0, 0x44C0, 0xCC00,
// 7       8       9       10      11      12 
// 0100    0100    1000    0100    0010    1110
// 0100    1100    1100    1110    1110    0010
// 0100    1000    0100    0000    0000    0000
// 0100    0000    0000    0000    0000    0000
   0x4444, 0x4C80, 0x8C40, 0x4E00, 0x2E00, 0xE200, 
//                         13      14      15
//                         0100    1100    1100
//                         1100    0100    1000
//                         0100    0100    1000
//                         0000    0000    0000
                           0x4C40, 0xC440, 0xC880, 
//                         16      17      18
//                         1110    1110    1000
//                         0100    1000    1110
//                         0000    0000    0000
//                         0000    0000    0000
                           0xE400, 0xE800, 0x8E00 };

// Describes which tetrino this one becomes when rotated.
//                       0, 1, 2,  3,  4,  5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
static rot_table[19] = { 7, 8, 9, 10, 11, 12, 6, 0, 1, 2, 13, 14, 15, 16, 17, 18,  3,  4,  5 };

struct falling_block
{
    x;
    y;
    type;
}

local _falling_blocks:falling_block[sizeof falling_block * 4];
static falling_blocks:falling_block;
falling_blocks = _falling_blocks;
static falling_offset_x = 0;
static falling_offset_y = 0;
static falling_tet = 0;
static falling_type = 0;
static next_tet = 0;
static next_type = 0;
static lines_broke = 0;
static gameover = 0;
static level = 0;
static next_level = 16;
static until_next = 16;
static fall_delay = 512;
static board;
local _board[screen_size];
board = _board;

// Need to get keyboard input.
local kb = detect_hardware(GENERIC_KEYBOARD_ID);

memset(board, 0x0000, screen_size);

// Draw background.
memset(vram, 0xF012, 384);

// Prime and draw next-tet
next_tet = random() % 19;
next_type = (random() % 3) + 1;

draw_box(1, 1, 6, 8);
draw_next_tet(tets[next_tet], next_type, 2, 2);

draw_string(vram, 2, 1, "NEXT");


draw_box(24, 1, 7, 6);
draw_string(vram, 25, 1, "SCORE");
draw_box(24, 9, 7, 6);
draw_string(vram, 25, 5, "LEVEL");
draw_box(24, 17, 7, 6);
draw_string(vram, 25, 9, "NEXTL");

draw_score();

// Draw the game column
local i = 0;
while (i < 12)
{
    vram[(i * 32) + 8] = 0xF010;
    vram[(i * 32) + 9 + screen_width] = 0xF011;
    memset(vram + (i * 32) + 9, 0xF013, screen_width);
    i += 1;
}

spawn_tet();
local delay_counter = 0;
while (1)
{
    // Check for input.
    local key = kb_getkey(kb);

    if (key == 'a' && check_falling_blocks(board, falling_blocks, 0xFFFF, 0) == 0)
    {
        move_falling_blocks(board, falling_blocks, 0xFFFF, 0);
        falling_offset_x -= 1;
    }
    else if (key == 'd' && check_falling_blocks(board, falling_blocks, 1, 0) == 0)
    {
        move_falling_blocks(board, falling_blocks, 1, 0);
        falling_offset_x += 1;
    }
    else if (kb_query(kb,'s'))
    {
        delay_counter += 300; 
    }
    else if (key == 'w') // Rotate!
    {
        // Hrm. Going to generate a NEW rotated block.
        local new_falling[4 * sizeof falling_block];
        local rotated = rot_table[falling_tet];
        create_new_falling_tet(new_falling, tets[rotated], falling_type, falling_offset_x, falling_offset_y);
        
        // Check it against the board.
        // Nudge it left or right and check it there too if necessary.
        local offset_x = 0;
        if (check_falling_blocks(board, new_falling, 0, 0) == 1)
        {
            offset_x = 1;
            if (check_falling_blocks(board, new_falling, 1, 0) == 1)
            {
                offset_x = 0xFFFF;
                if (check_falling_blocks(board, new_falling, 0xFFFF, 0) == 1)
                    offset_x = 2;
            }
        }

        if (offset_x != 2)
        {
            // apply offset to blocks.
            local i = 0;
            while (i < 4)
            {
                (new_falling + (i * sizeof falling_block)):falling_block.x += offset_x;
                i += 1;
            }

            falling_offset_x += offset_x;
            falling_tet = rotated;

            unmark_falling(board, falling_blocks);
            memcpy(falling_blocks, new_falling, 4 * sizeof falling_block);
            mark_falling(board, falling_blocks);
        }
    }

    delay_counter += 1;

    if (delay_counter > fall_delay)
    {
        game_update(board, falling_blocks);
        delay_counter = 0;
    }
}

// Map the blocks of the tetrimino to falling blocks.
function create_new_falling_tet(dest:falling_block, tetrimino, type, offset_x, offset_y)
{
    local bit = 0x000F;
    local block = 0;
    while (bit != 0xFFFF)
    {
        if (0x0001 & tetrimino)
        {
            (dest + (block * sizeof falling_block)):falling_block.x = (bit % 4) + offset_x;
            (dest + (block * sizeof falling_block)):falling_block.y = (bit / 4) + offset_y;
            (dest + (block * sizeof falling_block)):falling_block.type = type;
            block += 1;
        }

        tetrimino >>= 1;
        bit -= 1;
    }
}

function draw_next_tet(tetrimino, type, offset_x, offset_y)
{
    local buff[16];
    memset(buff, 0x0000, 16);

    local bit = 0x000F;
    while (bit != 0xFFFF)
    {
        if (0x0001 & tetrimino)
        {
            buff[bit] = 1;
        }

        tetrimino >>= 1;
        bit -= 1;
    }

    local x = 0;
    while (x < 4)
    {
        local y = 0;
        while (y < 2)
        {
            local upper = buff[(y * 8) + x] * next_type;
            local lower = buff[(y * 8) + x + 4] * next_type;
            vram[((offset_y + y) * 32) + offset_x + x] = 0xF000 | choose_glyph(upper, lower);
            y += 1;
        }
        x += 1;
    }
}

function draw_score()
{
    local buffer[5];
    itox(lines_broke, buffer);
    draw_string(vram, 26, 2, buffer);
    itox(level, buffer);
    draw_string(vram, 26, 6, buffer);
    itox(until_next, buffer);
    draw_string(vram, 26, 10, buffer);
}

function check_row(board, row)
{
    local x = 0;
    while (x < screen_width)
    {
        if (board[(row * screen_width) + x] == 0)
            return 0;
        x += 1;
    }
    return 1;
}

function unmark_falling(board, falling_blocks)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = 0;
        update_vram(board, block.x, block.y);
        i += 1;
    }
}

function mark_falling(board, falling_blocks)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = block.type + 4;
        update_vram(board, block.x, block.y);
        i += 1;
    }
}

function scroll_board(board, row)
{    
    while (row != 0)
    {
        memcpy(board + (row * screen_width), board + ((row - 1) * screen_width), screen_width);
        row -= 1;
    }
    memset(board, 0, screen_width);
}

function choose_glyph(upper, lower)
{
    return ((upper % 4) << 2) + (lower % 4);
}

function update_vram(board, game_x, game_y)
{
    local lem_y = game_y / 2;
    vram[(lem_y * 32) + game_x + 9] = 0xF000 | choose_glyph(
        board[(lem_y * screen_width * 2) + game_x], 
        board[(lem_y * screen_width * 2) + game_x + screen_width]);
}

function draw_whole_screen(board)
{
    local y = 0;
    while (y != screen_height)
    {
        local x = 0;
        local board_row = board + (y * screen_width * 2);
        local lem_row = vram + (y * 32) + 9;
        while (x < screen_width)
        {
            lem_row[x] = 0xF000 | choose_glyph(board_row[x], board_row[x + screen_width]);
            x += 1;
        }
        y += 1;
    }
}
    
// Return 1 if any of the blocks are blocked from sliding to offset by solid block.
function check_falling_blocks(board, falling_blocks, offset_x, offset_y)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        if ((block.y + offset_y) == screen_height) return 1;
        if ((block.x + offset_x) == 0xFFFF || (block.x + offset_x) == screen_width) return 1;
        local world_block = board[((block.y + offset_y) * screen_width) + block.x + offset_x];
        if ((world_block > 0) && (world_block < 4)) return 1;
        i += 1;
    }
    return 0;
}

function move_falling_blocks(board, falling_blocks, offset_x, offset_y)
{
    unmark_falling(board, falling_blocks);

    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        block.x += offset_x;
        block.y += offset_y;
        i += 1;
    }

    mark_falling(board, falling_blocks);
}

// Lock each falling block to the board.
function lock_falling_blocks(board, falling_blocks)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = block.type;
        i += 1;
    }
}

function game_update(board, falling_blocks)
{
    if (check_falling_blocks(board, falling_blocks, 0, 1) == 1)  
    {
        lock_falling_blocks(board, falling_blocks);
        
        // Check rows occupied by tetrimino. They might now be complete.
        local rows_cleared[4];
        memset(rows_cleared, 0xFFFF, 4);
        local i = 0;
        while (i < 4)
        {
            local row = (falling_blocks + (i * sizeof falling_block)):falling_block.y;
            if (check_row(board, row))
            {
                // Find a free entry in rows cleared - avoiding duplicates.
                local j = 3;
                while (j != 0xFFFF)
                {
                    if (rows_cleared[j] == 0xFFFF)
                    {
                        rows_cleared[j] = row;
                        j = 0;
                    }
                    else if (rows_cleared[j] == row)
                        j = 0;
                    j -= 1;
                }
            }
            i += 1;
        }

        // rows_cleared now contains each solid row exactly once. They should also be sorted top to bottom,
        // since we stored them backwards. Maybe.

        i = 0;
        while (i < 4)
        {
            if (rows_cleared[i] != 0xFFFF)
            {
                lines_broke += 1;
                scroll_board(board, rows_cleared[i]);

                until_next -= 1;
                if (until_next == 0)
                {
                    next_level *= 2;
                    until_next = next_level;
                    if (fall_delay > 64) fall_delay -= 64;
                    level += 1;
                }

                draw_score();
            }
            i += 1;
        }

        draw_whole_screen(board);

        spawn_tet();

        if (gameover)
        {
            draw_string(vram, 16 - 8, 6, "**GAME OVER**");
            while (1) {}
        }
    }
    else
    {
        move_falling_blocks(board, falling_blocks, 0, 1);
        falling_offset_y += 1;
    }

   
}

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

// y is in GAME coordinates. Meaning the top and bottom rows might be half rows (0x14 and 0x15)
function draw_box(x, y, w, h)
{
    local lem_y = y / 2;
    local lem_e = (y + h) / 2;
    if ((y % 2) == 1) // Starts on odd row.
    {
        memset(vram + (lem_y * 32) + x, 0xF014, w);
        lem_y += 1;
    }

    local end_y = (y + h);
    if ((end_y % 2) == 1) // Ends on odd row.
    {
        memset(vram + ((end_y / 2) * 32) + x, 0xF015, w);
        lem_e -= 1;
    }

    local _y = lem_y;
    while (_y < (lem_e + 1))
    {
        memset(vram + (_y * 32) + x, 0xF013, w);
        _y += 1;
    }
}


function spawn_tet()
{
    falling_tet = next_tet;
    next_tet = random() % 19;
    falling_type = next_type;
    next_type = (random() % 3) + 1;
    falling_offset_x = 4;
    falling_offset_y = 0;
    create_new_falling_tet(falling_blocks, tets[falling_tet], falling_type, falling_offset_x, falling_offset_y);
    if (check_falling_blocks(board, falling_blocks, 0, 0))
        gameover = 1;
    mark_falling(board, falling_blocks);
    draw_next_tet(tets[next_tet], next_type, 2, 2);
}

