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

// Functions for interfacing with generic keyboard devices.
 
#ifndef _BITWISE_LIB_KEYBOARD_
#define _BITWISE_LIB_KEYBOARD_
 
#include hardware.b

/*
Name: Generic Keyboard (compatible)
ID: 0x30cf7406
Version: 1

Interrupts do different things depending on contents of the A register:

 A | BEHAVIOR
---+----------------------------------------------------------------------------
 0 | Clear keyboard buffer
 1 | Store next key typed in C register, or 0 if the buffer is empty
 2 | Set C register to 1 if the key specified by the B register is pressed, or
   | 0 if it's not pressed
 3 | If register B is non-zero, turn on interrupts with message B. If B is zero,
   | disable interrupts
---+----------------------------------------------------------------------------

When interrupts are enabled, the keyboard will trigger an interrupt when one or
more keys have been pressed, released, or typed.

Key numbers are:
  0x10: Backspace
  0x11: Return
  0x12: Insert
  0x13: Delete
  0x20-0x7f: ASCII characters
  0x80: Arrow up
  0x81: Arrow down
  0x82: Arrow left
  0x83: Arrow right
  0x90: Shift
  0x91: Control
*/
 
static GENERIC_KEYBOARD_ID[2] = { 0x30cf, 0x7406 };

// Get the last key pressed.
// Returns 0 if no key data present.
function kb_getkey(id)
{
	asm ( B = id )
	{
		SET A, 1
		HWI B
		SET A, C
	}
}

function kb_query(kb, key)
{
  asm ( B = key, C = kb )
  {
    SET A, 2
    HWI C
    SET A, C
  }
}

#endif