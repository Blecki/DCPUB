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
*	KaiComm Hardware Interface Card Driver
*	4/4/06 - AC - Draft
*	7/28/06 - AC - Fix stack corruption on 32 port devices
*/

#ifndef _BITWISE_KAICOMM_HIC_
#define _BITWISE_KAICOMM_HIC_

static KAICOMM_HIC_HARDWARE_ID[2] = { 0xE023, 0x9088 };
static KAICOMM_HIC_MANUFACTURER_ID[2] = { 0xA87C, 0x900E };

#define KAICOMM_HIC_VERSION_8 0x0442
#define KAICOMM_HIC_VERSION_16 0x0444
#define KAICOMM_HIC_VERSION_32 0x0448

#define KAICOMM_HIC_SUCCEED 0x0000
#define KAICOMM_HIC_FAIL 0x0001

#define KAICOMM_HIC_RECEIVE_ERROR_SUCCESS 0x0000
#define KAICOMM_HIC_RECEIVE_ERROR_OVERFLOW 0x0001
#define KAICOMM_HIC_RECEIVE_ERROR_FAIL_ERROR 0x0002
#define KAICOMM_HIC_RECEIVE_ERROR_FAIL_NO_DATA 0x0003

#define KAICOMM_HIC_TRANSMIT_ERROR_NO_ERROR 0x0000
#define KAICOMM_HIC_TRANSMIT_ERROR_PORT_BUSY 0x0001
#define KAICOMM_HIC_TRANSMIT_ERROR_OVERFLOW 0x0002
#define KAICOMM_HIC_TRANSMIT_ERROR_NOT_CONNECTED 0x0003
#define KAICOMM_HIC_TRANSMIT_ERROR_BUSY 0x0004

function kaicomm_hic_query(
	device_id, // Id of device to query
	in_port, // Port number to query
	out_status, // Write address for status word
	out_port_ready ) // Write address for ready port word
	// Returns KAICOMM_HIC_FAIL or KAICOMM_HIC_SUCCEED
{
	asm ( Y = device_id,
		C = in_port,
		I = out_status,
		J = out_port_ready )
	{
		SET A, 0x0000
		HWI Y
		SET [I], A
		SET [J], C
	}

	return KAICOMM_HIC_SUCCEED;
}

function kaicomm_hic_receive(
	device_id, // Id of device to receive from
	in_port, // Port number to read
	out_data, // Write address for data
	out_status ) // Write address for status code
	// Returns KAICOMM_HIC_FAIL or KAICOMM_HIC_SUCCEED
{
	asm ( Y = device_id,
		C = in_port,
		I = out_data,
		J = out_status )
	{
		SET A, 0x0001
		HWI Y
		SET [I], B
		SET [J], C
	}

	return KAICOMM_HIC_SUCCEED;
}


#endif

