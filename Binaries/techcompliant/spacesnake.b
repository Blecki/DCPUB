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
*/

#include lem.b
#include keyboard.b

/* INITIALIZATION */

static vram = __endofprogram;
local lem = lem_detect();
lem_initialize(lem, vram);
local kb = kb_detect();
local segments = 4;
local game[LEM_VRAM_SIZE * 2];
static _game = 0;
_game = game;
local i = 0;
local sx = 16;
local sy = 6;
#define sw 32
#define sh 24

while (i < (LEM_VRAM_SIZE * 2)) { game[i] = 0x0000; i += 1; }
game[(sy * sw) + sx] = 1;
game[(4 * sw) + 4] = 0xFFFF;


function kb_query(kb, key)
{
	asm ( B = key, C = kb )
	{
		SET A, 2
		HWI C
		SET A, C
	}
}

static font[256] = {  0x0000, 0x0000, 0x0F0F, 0x0F0F, 0xF0F0, 0xF0F0, 0xFFFF, 0xFFFF, 
                     0x0060, 0x6000, 0x0F6F, 0x6F0F, 0x0000, 0x0000, 0x0000, 0x0000,
                     0x0006, 0x0600, 0x0000, 0x0000, 0xF0F6, 0xF6F0, 0x0000, 0x0000,
                     0x0066, 0x6600, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,

                    //0x000F, 0x0808, 0x080F, 0x0808, 0x08F8, 0x0808, 0x00FF, 0x0808,
                    //0x0808, 0x0808, 0x08FF, 0x0808, 0x00FF, 0x1414, 0xFF00, 0xFF08,
                    //0x1F10, 0x1714, 0xFC04, 0xF414, 0x1710, 0x1714, 0xF404, 0xF414,
                    //0xFF00, 0xF714, 0x1414, 0x1414, 0xF700, 0xF714, 0x1417, 0x1414,
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

asm (B = font; C = lem)
{
    SET A, 1
    HWI C
}

function update_lem_cell(x, y, upper, lower)
{
    local game_value = ((upper > 0) & (upper != 0xFFFF)) +
        (((lower > 0) & (lower != 0xFFFF)) * 2);
                
    if ((upper == 0xFFFF) & (lower == 0xFFFF)) game_value += 12;
    else if (upper == 0xFFFF) game_value += 8;
    else if (lower == 0xFFFF) game_value += 4;

    vram[(y * 32) + x] = 0xF000 | game_value;
}

static random_seed_a = 0x5678;
static random_seed_b = 0x1234;

function random()
{
    local a = random_seed_a;
    local b = random_seed_b;
    a *= 0x660D;
    b *= 0x0019;
    random_seed_a *= 0x660D;

    asm ( A = &a, B = b ) { ADX [A], B }

    random_seed_a += 1;

    asm ( A = &a, B = b ) { ADD [A], EX }

    random_seed_b = a;

    return a;
}

// Update the lem cell associated with this game cell.
// This involves two game cells.
function update_lem(game, game_x, game_y)
{
    local lem_y = game_y / 2;
    local base_y = game_y & 0xFFFE;

    update_lem_cell(game_x, lem_y, game[(lem_y * 64) + game_x], game[(lem_y * 64) + game_x + 32]);
}

local x = 0;
while (x < 32)
{
    local y = 0;
    while (y < 12)
    {
        update_lem_cell(x, y, game[(y * 64) + x], game[(y * 64) + x + 32]);
        y += 1;
    }
    x += 1;
}

local playing = 1;

while (playing)
{
    local valid = 0;
    
    if (kb_query(kb, 'w') & (sy > 0)) { sy -= 1; valid = 1; }
    else if (kb_query(kb, 'a') & (sx > 0)) { sx -= 1; valid = 1; }
    else if (kb_query(kb, 's') & (sy < (sh - 1))) { sy += 1; valid = 1; }
    else if (kb_query(kb, 'd') & (sx < (sw - 1))) { sx += 1; valid = 1; }
     
    if (valid == 1)
    {
        if (game[(sy * sw) + sx] == 0xFFFF)
        {
            segments += 1;
            //Position new random food piece.
            local new_spot = random() % (sw * sh);

            while (game[new_spot != 0])
            {
                new_spot += 1;
                if (new_spot == (sw * sh))
                    new_spot = 0;
            }

            game[new_spot] = 0xFFFF;
            update_lem(game, new_spot % 32, new_spot / 32);
        }
        else if (game[(sy * sw) + sx] != 0)
            playing = 0;
        
        game[(sy * sw) + sx] = segments;        
        
        update_lem(game, sx, sy);

        i = 0; while (i < (sw * sh))
        {
            local c = game[i];
            if ((c != 0) & (c != 0xFFFF)) 
            {
                game[i] = c - 1;
                if (c == 1) 
                    update_lem(game, i % 32, i / 32);
            }
            i += 1;
        }
    }
}

static gameover_message = "*GAME OVER*";
local p = vram + (5 * 32) + 12;
i = 0;
while (i < *gameover_message)
{
    *p = 0xF000 | gameover_message[i + 1];
    p += 1;
    i += 1;
}

