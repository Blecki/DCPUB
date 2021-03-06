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

// Drivers for m35fd devices

#ifndef _BITWISE_LIB_M35FD_
#define _BITWISE_LIB_M35FD_

#include number32.b

static M35FD_HARDWARE_ID[2] = { 0x4FD5, 0x24C5 };
static M35FD_MANUFACTURER_ID[2] = { 0x1eb3, 0x7e91 };

#define M35FD_SECTOR_SIZE 		512
#define M35FD_SECTOR_COUNT 		1440

#define M35FD_STATE_NO_MEDIA 	0x0000
#define M35FD_READY				0x0001
#define M35FD_READY_WP			0x0002
#define M35FD_BUSY				0x0003

#define M35FD_ERROR_NONE		0x0000
#define M35FD_ERROR_BUSY		0x0001
#define M35FD_ERROR_NO_MEDIA 	0x0002
#define M35FD_ERROR_PROTECTED	0x0003
#define M35FD_ERROR_EJECT		0x0004
#define M35FD_ERROR_BAD_SECTOR	0x0005
#define M35FD_ERROR_BROKEN		0xFFFF

// Find m35fd devices. Returns number of devices found, up to max_drives.
function m35fd_enumerate(m35fd_drives /*Pointer to at least max_drives words*/, max_drives)
{
	local num_hardware = 0;
	local drives_found = 0;
	asm ( B = &num_hardware ) // Passing the address of a local is currently the only way to get data out of an asm block.
	{
		HWN [B]
	}

	local n = 0;
	while ( n < num_hardware )
	{
		local hardware_id[2];

		asm ( A = n; I = hardware_id ) // Fetch the hardware id of hardware n
		{
			HWQ A
			SET [I + 0x0001], A
			SET [I], B
		}

		if ( compare32(hardware_id, M35FD_HARDWARE_ID) != 0 )
		{
			m35fd_drives[drives_found] = n;
			drives_found += 1;
			if (drives_found == max_drives) return drives_found;
		}

		n += 1;
	}
	return drives_found;
}

//0  Poll device. Sets B to the current state (see below) and C to the last error
//   since the last device poll.

function m35fd_poll_state(hardware_id)
{
	asm (Y = hardware_id)
	{
		SET A, 0
		HWI Y
		SET A, B
	}
	//Returns A register.
}

function m35fd_poll_error(hardware_id)
{
	asm (Y = hardware_id)
	{
		SET A, 0
		HWI Y
		SET A, C
	}
	//Returns A register.
}

//2  Read sector. Reads sector X to DCPU ram starting at Y.
//   Sets B to 1 if reading is possible and has been started, anything else if it
//   fails. Reading is only possible if the state is STATE_READY or
//   STATE_READY_WP.
//   Protects against partial reads.
//
// Returns 1 if successfully begins read. If not 1, call m35fd_poll_error for specific error code.
function m35fd_read(hardware_id, sector, buffer)
{
	asm (B = hardware_id; X = sector; Y = buffer)
	{
		SET A, 0x0002
		HWI B
		SET A, B
	}
}
   
//3  Write sector. Writes sector X from DCPU ram starting at Y.
//   Sets B to 1 if writing is possible and has been started, anything else if it
//   fails. Writing is only possible if the state is STATE_READY.
//   Protects against partial writes.
//
// Return 1 if successfully begins write. If not 1, call m35fd_poll_error for specific error code.
function m35fd_write(hardware_id, sector, buffer)
{
	asm (B = hardware_id; X = sector; Y = buffer)
	{
		SET A, 0x0003
		HWI B
		SET A, B
	}
}

// Read a sector from disc. Does not return until sector is finished reading.
function m35fd_blocking_read(drive, sector, into /* Pointer to M35FD_SECTOR_SIZE words of memory*/)
{
	if (m35fd_read(drive, sector, into) != 1)
		return m35fd_poll_error(drive);
	else
	{
		while (m35fd_poll_state(drive) == M35FD_BUSY) {}
		return M35FD_ERROR_NONE;
	}
}

// Write a sector to disc. Does not return until sector is finished writing.
function m35fd_blocking_write(drive, sector, from /* Pointer to M35FD_SECTOR_SIZE words of memory*/)
{
	if (m35fd_write(drive, sector, from) != 1)
		return m35fd_poll_error(drive);
	else
	{
		while (m35fd_poll_state(drive) == M35FD_BUSY) {}
		return M35FD_ERROR_NONE;
	}
	return M35FD_ERROR_NONE;
}

#endif

/*




                                      .!.
                                     !!!!!. 
                                  .   '!!!!!. 
                                .!!!.   '!!!!!.
                              .!!!!!!!.   '!!!!!.
                            .!!!!!!!!!'   .!!!!!!!.
                            '!!!!!!!'   .!!!!!!!!!'
                              '!!!!!.   '!!!!!!!' 
                                '!!!!!.   '!!!'
                                  '!!!!!.   '
                                    '!!!!! 
                                      '!'   


                          M A C K A P A R    M E D I A                          






    .---------------------.
----! DCPU-16 INFORMATION !----------------------------------------------------- 
    '---------------------' 

Name: Mackapar 3.5" Floppy Drive (M35FD) 
ID: 0x4fd524c5, version: 0x000b
Manufacturer: 0x1eb37e91 (MACKAPAR)



    .-------------.
----! DESCRIPTION !------------------------------------------------------------- 
    '-------------'

The Mackapar 3.5" Floppy Drive is compatible with all standard 3.5" 1440 KB
floppy disks. The floppies need to be formatted in 16 bit mode, for a total of
737,280 words of storage. Data is saved on 80 tracks with 18 sectors per track,
for a total of 1440 sectors containing 512 words each.
The M35FD works is asynchronous, and has a raw read/write speed of 30.7kw/s.
Track seeking time is about 2.4 ms per track.



    .--------------------.
----! INTERRUPT BEHAVIOR !------------------------------------------------------
    '--------------------'
    
A, B, C, X, Y, Z, I, J below refer to the registers on the DCPU
    
A: Behavior:

0  Poll device. Sets B to the current state (see below) and C to the last error
   since the last device poll.
   
1  Set interrupt. Enables interrupts and sets the message to X if X is anything
   other than 0, disables interrupts if X is 0. When interrupts are enabled,
   the M35FD will trigger an interrupt on the DCPU-16 whenever the state or
   error message changes.

2  Read sector. Reads sector X to DCPU ram starting at Y.
   Sets B to 1 if reading is possible and has been started, anything else if it
   fails. Reading is only possible if the state is STATE_READY or
   STATE_READY_WP.
   Protects against partial reads.
   
3  Write sector. Writes sector X from DCPU ram starting at Y.
   Sets B to 1 if writing is possible and has been started, anything else if it
   fails. Writing is only possible if the state is STATE_READY.
   Protects against partial writes.


    .-------------.
----! STATE CODES !-------------------------------------------------------------
    '-------------'
  
0x0000 STATE_NO_MEDIA   There's no floppy in the drive.
0x0001 STATE_READY      The drive is ready to accept commands.
0x0002 STATE_READY_WP   Same as ready, except the floppy is write protected.
0x0003 STATE_BUSY       The drive is busy either reading or writing a sector.



    .-------------.
----! ERROR CODES !-------------------------------------------------------------
    '-------------'
    
0x0000 ERROR_NONE       There's been no error since the last poll.
0x0001 ERROR_BUSY       Drive is busy performing an action
0x0002 ERROR_NO_MEDIA   Attempted to read or write with no floppy inserted.
0x0003 ERROR_PROTECTED  Attempted to write to write protected floppy.
0x0004 ERROR_EJECT      The floppy was removed while reading or writing.
0x0005 ERROR_BAD_SECTOR The requested sector is broken, the data on it is lost.
0xffff ERROR_BROKEN     There's been some major software or hardware problem,
                        try turning off and turning on the device again.



   COPYRIGHT 1987 MACKAPAR MEDIA    ALL RIGHTS RESERVED    DO NOT DISTRIBUTE 

*/
