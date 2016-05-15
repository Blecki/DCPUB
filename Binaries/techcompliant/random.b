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

#ifndef _BITWISE_RANDOM_
#define _BITWISE_RANDOM_

static random_seed_a = 0x5678;
static random_seed_b = 0x1234;

function random()
{
    asm ( B = &random_seed_a, C = &random_seed_b )
    {
        MUL [B], 0x660D;
        SET A, [B]
        MUL [C], 0x0019
        ADX A, [C]
        ADD [B], 1
        ADD A, EX
        SET [C], A
    }
    // Return A
}

#endif
