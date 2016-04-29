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

// Functions for interfacing with Lem1802 devices.

#ifndef _BITWISE_LIB_LEM_
#define _BITWISE_LIB_LEM_

#include hardware.b

static LEM_HARDWARE_ID[2] = { 0x7349, 0xf615 };
#define LEM_VRAM_SIZE 384

// Find a Lem device. Returns 0xFFFF if no Lem device is connected.
function lem_detect()
{
	return detect_hardware(LEM_HARDWARE_ID);
}

// Assign video memory to a Lem device. vram should be a pointer to
// a memory block of size LEM_VRAM_SIZE.
// Disregard return value.
function lem_initialize(id, vram)
{
	asm (A = 0; B = vram; C = id)
	{
		HWI C
	}
}

#endif
