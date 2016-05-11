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

#ifndef _BITWISE_KAICOMM_RCI_
#define _BITWISE_KAICOMM_RCI_

static KAICOMM_RCI_HARDWARE_ID[2] = { 0xD005, 0x90A5 };
static KAICOMM_RCI_MANUFACTURER_ID[2] = { 0xA87C, 0x900E };

#define KAICOMM_RCI_RECEIVE_BUFFER_EMPTY 	0x0000
#define KAICOMM_RCI_RECEIVE_BUFFER_DATA 	0x0001
#define KAICOMM_RCI_RADIO_IDLE			0x0000
#define KAICOMM_RCI_RADIO_RECEIVING 		0x0002
#define KAICOMM_RCI_RADIO_TRANSMITTING		0x0004
#define KAICOMM_RCI_RADIO_COLLISION		0x0006
#define KAICOMM_RCI_ANTENNA_FAILURE		0xFFE0
#define KAICOMM_RCI_HARDWARE_FAILURE		0xFFF0

#define KAICOMM_RCI_SEND_SUCCEED		0x0000
#define KAICOMM_RCI_SEND_FAIL			0x0001

#define KAICOMM_RCI_SETTINGS_VALID		0x0000
#define KAICOMM_RCI_SETTINGS_INVALID		0x0001

#define KAICOMM_RCI_BUFFER_SIZE 256

function kaicomm_rci_query_status( 
	device_id,   // Id of the rci device
	out_channel, // Pointer where current channel is written.
	out_power,
	out_status )  
{
	asm ( Y = device_id;
		I = out_channel;
		J = out_power;
		X = out_status )
	{
		SET A, 0
		HWI Y
		SET [I], A
		SET [J], B
		SET [X], C
	}
}

function kaicomm_rci_receive(
	device_id, // Id of the rci device
	out_buffer, // Pointer to 256 words where data is written
	out_size,
	out_status )
{
	asm ( Y = device_id;
		B = out_buffer;
		I = out_size;
		J = out_status )
	{
		SET A, 1
		HWI Y
		SET [I], B
		set [J], C
	}
}

function kaicomm_rci_send(
	device_id, // Id of the rci device
	in_buffer, // Pointer to 256 words of data to transmit
	in_size,
	out_status ) 
{
	asm ( Y = device_id;
		B = in_buffer;
		C = in_size;
		I = out_status)
	{
		SET A, 2
		HWI Y
		SET [I], C
	}
}

function kaicomm_rci_tune(
	device_id, // Id of the rci device
	in_channel, // Channel to tune to
	in_power,
	out_status )
{
	asm ( Y = device_id;
		B = in_channel;
		C = in_power;
		I = out_status )
	{
		SET A, 3
		HWI Y
		SET [I], C
	}
}

function kaicomm_rci_configure_interrupts(
	device_id, // Id of the rci device
	in_receive_message,
	in_send_message )
{
	asm ( Y = device_id;
		B = in_receive_message;
		C = in_send_message )
	{
		SET A, 4
		HWI Y
	}
}

#endif

