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

#ifndef _BITWISE_LIB_TOKEN_COMM_
#define _BITWISE_LIB_TOKEN_COMM_

#include rci.b
#include hardware.b

local __token_comm_hardware_id = 0;

#define TOKEN_COMM_SUCCESS		0x0000
#define TOKEN_COMM_NO_HARDWARE  0x0001
#define TOKEN_COMM_FAIL	 0x0003
#define TOKEN_COMM_WAIT 0x0004

/*
	Detect a KAICOMM RCI device.
*/
function token_comm_initialize()
{
	__token_comm_hardware_id = detect_hardware(KAICOMM_RCI_HARDWARE_ID);

	if (__token_comm_hardware_id == 0xFFFF) 
		return TOKEN_COMM_NO_HARDWARE;

	return TOKEN_COMM_SUCCESS;
}

/* 
	Query the status of the device.
*/
function __token_comm_qstatus()
{
	local channel;
	local power;
	local status;
	kaicomm_rci_query_status(__token_comm_hardware_id, &channel, &power, &status);
	return status;
}

/*
	Attempt to send data. This function checks the status of the device 
	first, to avoid destroying incoming datagrams.
*/
function token_comm_safe_send(payload, size)
{
	local status = __token_comm_qstatus();

	// Keep this send from obliterating a receiving packet.
	if (status & KAICOMM_RCI_RECEIVING != 0)
		return TOKEN_COMM_WAIT;

	if (status & KAICOMM_RCI_TRANSMITTING != 0)
		return TOKEN_COMM_WAIT;

	kaicomm_rci_send(
		__token_comm_hardware_id, 
		payload,
		size,
		&status);

	if (status == 0) return TOKEN_COMM_SUCCESS;
	return TOKEN_COMM_FAIL;
}

/*
	Receive data. Returns TOKEN_COMM_WAIT if no datagram is ready
	to be received.
*/
function token_comm_safe_receive(out_payload, out_size)
{
	local status = __token_comm_qstatus();

	if (status & KAICOMM_RCI_RECEIVE_BUFFER_DATA == 0)
		return TOKEN_COMM_WAIT;

	kaicomm_rci_receive(
		__token_comm_hardware_id,
		out_payload,
		out_size,
		&status);

	if (status == 0) return TOKEN_COMM_SUCCESS;
	return TOKEN_COMM_FAIL;
}

#endif
