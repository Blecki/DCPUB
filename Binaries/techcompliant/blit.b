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

struct blit_screen_descriptor
{
   width;
   height;
   bits_per_pixel;
   stride_bits;
   packed;
   video_memory;
}

// Zero the bits this value will overlap.
function blit_clear_bits(size, offset, destination) 
{ 
 	local words_deep = offset / 16; 
 	local first_offset = offset % 16; 

       local mask = 0xFFFF >> (16 - size);

       if (first_offset > (16 - size)) 
          destination[words_deep] |= !(mask >> (16 - (size - first_offset)));
      else
         destination[words_deep] |= !(mask << (16 - (size - first_offset)));

      if ((first_offset + size) > 15)
         destination[words_deep + 1] |= !(value << (16 - (size - (16 - first_offset)));
} 

// Set a single pixel in the screen. Don't use this to do lots of drawing.
function blit_set_pixel(screen:blit_screen_descriptor, x, y, pixel)
{
   local bit_index = (y * screen.stride) + x;
   blit_clear_bits(screen.bits_per_pixel, bit_index, screen.video_memory);
   bp_dpack(screen.bits_per_pixel, bit_index, pixel, screen.video_memory);
}

#endif
