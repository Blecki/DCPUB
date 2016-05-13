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
local segments = 2;
local game[LEM_VRAM_SIZE * 2];
local i = 0;
local sx = 16;
local sy = 6;
#define sw 32
#define sh 24

while (i < (LEM_VRAM_SIZE * 2)) { game[i] = 0x0000; i += 1; }
game[(sy * sw) + sx] = 1;
game[(4 * sw) + 4] = 0xFFFF;
game[(5 * sw) + 5] = 0xFFFF;


function kb_query(kb, key)
{
	asm ( B = key, C = kb )
	{
		SET A, 2
		HWI C
		SET A, C
	}
}

static font[24] = {  0x0000, 0x0000, 0x0F0F, 0x0F0F, 0xF0F0, 0xF0F0, 0xFFFF, 0xFFFF, 
                     0x0006, 0x0600, 0x0F6F, 0x6F0F, 0x0000, 0x0000, 0x0000, 0x0000,
                     0x0060, 0x6000, 0x0000, 0x0000, 0xF0F6, 0xF6F0, 0x0000, 0x0000 };

asm (B = font; C = lem)
{
    SET A, 1
    HWI C
}

while (true)
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
        }
        else if (game[(sy * sw) + sx] != 0)
            goto GAMEOVER;
        
        game[(sy * sw) + sx] = segments;        

        local x = 0;
        while (x < 32)
        {
            local y = 0;
            while (y < 12)
            {
                local game_upper = game[(y * 64) + x];
                local game_lower = game[(y * 64) + x + 32];
                local lem_i = (y * 32) + x;
                local game_value = ((game_upper > 0) & (game_upper != 0xFFFF)) +
                    (((game_lower > 0) & (game_lower != 0xFFFF)) * 2);
                if (game_upper == 0xFFFF) game_value += 4;
                if (game_lower == 0xFFFF) game_value += 8;
                vram[lem_i] = 0xF000 | game_value;
                y += 1;
            }
            x += 1;
        }

        //i = 0; while (i < (sw * sh))
        //{
        //    local c = game[i];
        //    if ((c != 0) & (c != 0xFFFF)) game[i] = c - 1;
        //    i += 1;
        //}
    }
}

:GAMEOVER

static gameover_message = "GAME OVER";
local p = vram + (6 * 32) + 4;
local i = 0;
