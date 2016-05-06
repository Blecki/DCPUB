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
function blit_set_pixel(screen:blit_screen_descriptor, x, y, pixel)
{
   local bit_index = (y * screen.stride_bits) + x;
   bp_clear(screen.bits_per_pixel, bit_index, screen.video_memory);
   bp_dpack(screen.bits_per_pixel, bit_index, pixel, screen.video_memory);
}

// Blit a whole bunch of pixels. Sure hope that the bits per pixel of 
//  the source and destination match!
function blit(source:blit_screen_descriptor, dest:blit_screen_descriptor, x, y, sx, sy, sw, sh)
{
  // Clip to bounds of destination surface.

  if ((x + sw) > dest.width)
    sw = dest.width - x;
  if ((y + sh) > dest.height)
    sh = dest.height - y;

  // Find row bounds.
  local dest_row_start = x * dest.bits_per_pixel;
  local dest_rs_word = dest_row_start / 16;
  local dest_rs_bit_offset = dest_row_start % 16;

  local dest_row_end = (x + sw) * dest.bits_per_pixel;
  local dest_re_word = dest_row_end / 16;
  local dest_re_bit_offset = dest_row_end % 16;

  local source_row_start = 

  // Loop over rows.
  local row = y;
  while (row < (y + sh))
  {
    dest_row_start = row * dest.stride_bits;

    row += 1;
  }
}

#endif
