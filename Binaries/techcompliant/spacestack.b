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

#define screen_width 12
#define screen_height 24
#define screen_size (12 * 24)

{ // Setup LEM display.

    static vram = __endofprogram; 
    local lem = detect_hardware(LEM_HARDWARE_ID); 
    lem_initialize(lem, vram);

    static font[256] = {  
        // Special glyphs for drawing tetriminos - 3 styles, in two positions.
        
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

// A single 16 bit value can describe a 4x4 tetrimino - more than large enough 
// for them.

static tetriminos[19] = {
// 0 I     1 Z     2 S     3 T     4 L     5 <L    6 O
// 1111    1100    0110    1000    1000    0100    1100
// 0000    0110    1100    1100    1000    0100    1100
// 0000    0000    0000    1000    1100    1100    0000
// 0000    0000    0000    0000    0000    0000    0000
   0xF000, 0xC600, 0x6C00, 0x8C80, 0x88C0, 0x44C0, 0xCC00,
// 7       8       9       10      11      12 
// 1000    0100    1000    0100    0010    1110
// 1000    1100    1100    1110    1110    0010
// 1000    1000    0100    0000    0000    0000
// 1000    0000    0000    0000    0000    0000
   0x8888, 0x4C80, 0x8C40, 0x4E00, 0x2E00, 0xE200, 
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

local falling_blocks:falling_block[sizeof falling_block * 4];

static debug_tet[12] = { 0, 0, 1, 1, 0, 1, 2, 0, 1, 3, 0, 1 };
memcpy(falling_blocks, debug_tet, 12);
    
// Map the blocks of the specified tetrimino to falling blocks.
function map_new_tetrimino(dest:falling_block, tetrimino, type, offset_x, offset_y)
{
    //memcpy(dest, debug_tet, 12);
    //return;


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

local board[screen_size];
memset(board, 0x0000, screen_size);

function check_bottom_row(board)
{
    return 0;
    local x = 0;
    local r = 1;
    while (x < screen_width)
    {
        r &= board[((screen_height - 1) * screen_width) + x];
        x += 1;
    }
    return r;
}

function scroll_board_down(board, falling_blocks)
{    
    // Unmark falling blocks.
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = 0;
        i += 1;
    }

    local y = screen_height - 1;
    while (y != 0)
    {
        memcpy(board + (y * screen_width), board + ((y - 1) * screen_width), screen_width);
        y -= 1;
    }
    memset(board, 0, screen_width);

    // Remark falling blocks.
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = block.type + 4;
        i += 1;
    }

}

function choose_glyph(upper, lower)
{
    return ((upper % 4) << 2) + (lower % 4);
}

function update_vram(board, game_x, game_y)
{
    local lem_y = game_y / 2;
    vram[(lem_y * 32) + game_x] = 0xF000 | choose_glyph(
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
        local lem_row = vram + (y * 32);
        while (x < screen_width)
        {
            lem_row[x] = 0xF000 | choose_glyph(board_row[x], board_row[x + screen_width]);
            x += 1;
        }
        y += 1;
    }
}
    
// Return 1 if any of the blocks are blocked from falling by a solid block below.
function check_falling_blocks(board, falling_blocks)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        if (block.y == (screen_height - 1)) return 1;
        local world_block = board[((block.y + 1) * screen_width) + block.x];
        if ((world_block > 0) && (world_block < 4)) return 1;
        i += 1;
    }
    return 0;
}

// Move each falling block down by 1, updating the screen as needed.
function move_falling_blocks(board, falling_blocks)
{
    local i = 0;
    while (i < 4)
    {
        local block:falling_block = falling_blocks + (i * sizeof falling_block);
        board[(block.y * screen_width) + block.x] = 0;
        update_vram(board, block.x, block.y);
        block.y += 1;
        board[(block.y * screen_width) + block.x] = block.type + 4;
        update_vram(board, block.x, block.y);
        i += 1;
    }
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
    if (check_falling_blocks(board, falling_blocks) == 1)  
    {
        lock_falling_blocks(board, falling_blocks);
        map_new_tetrimino(falling_blocks, tetriminos[random() % 19], 1, 0, 1);
    }
    else
    {
        move_falling_blocks(board, falling_blocks);
    }

    if (check_bottom_row(board))
    {
        scroll_board_down(board, falling_blocks);
        draw_whole_screen(board);
    }
}

map_new_tetrimino(falling_blocks, tetriminos[16], 2, 0, 1);
while (1)
{       
    game_update(board, falling_blocks);

    itox(board[screen_width + 1], vram);
    local x = 1;
    while (x < 5)
    {
        vram[x] |= 0xF000;
        x += 1;
    }
    vram[0] = 0;

    local i = 0;
    while (i != 1000)
        i += 1;
}

