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

#ifndef _BITWISE_BLIT_
#define _BITWISE_BLIT_

#include bitwise.b

struct surface
{
    width;
    height;
    memory;
}

// Set a single pixel in the surface. Don't use this to do lots of drawing.
function blit_set_pixel(screen:surface, x, y, pixel)
{
    screen.memory[(y * screen.height) + x] = pixel;
}

// Blit from source to dest. Writing off the edge of the dest surface is
//  not a good idea.
function blit(source:surface, sx, sy, sw, sh, dest:surface, x, y)
{
    local source_row = sy;
    local dest_row = y;
    while (source_row < sh)
    {
      asm (
        I = source.memory + (source_row * source.width) + sx;
        B = dest.memory + (dest_row * dest.width) + x;
        C = sw)
      {
        SET PUSH, J
        SET J, B
        ADD C, I
          :__BLIT_BEGIN
        STI [J], [I]
        IFL I, C
          SET PC, __BLIT_BEGIN
        SET J, POP
      }

      source_row += 1;
    }
}

#endif
