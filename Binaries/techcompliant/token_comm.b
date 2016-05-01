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
#include memory.b
#include std.b

local __token_comm_receive_buffer = 0;
local __token_comm_send_buffer = 0;
local __token_comm_hardware_id = 0;

#define TOKEN_COMM_SUCCESS		0x0000
#define TOKEN_COMM_NO_HARDWARE  0x0001
#define TOKEN_COMM_OUT_OF_MEMORY 0x0002
#define TOKEN_COMM_SEND_FAIL	 0x0003

#define __TC_MSG_GREET 	0x0000
#define __TC_MSG_DATA 	0x0001
#define __TC_MSG_ACK 	0x0002

local __token_comm_address = 0;

function token_comm_initialize(memory_page, address)
{
	__token_comm_address = address;

	__token_comm_hardware_id = detect_hardware(KAICOMM_RCI_HARDWARE_ID);

	if (__token_comm_hardware_id == 0xFFFF) 
		return TOKEN_COMM_NO_HARDWARE;

	__token_comm_receive_buffer = allocate_memory(KAICOMM_RCI_BUFFER_SIZE, memory_page);

	if (__token_comm_receive_buffer == 0)
		return TOKEN_COMM_OUT_OF_MEMORY;

	__token_comm_send_buffer = allocate_memory(KAICOMM_RCI_BUFFER_SIZE, memory_page);

	if (__token_comm_send_buffer == 0)
		return TOKEN_COMM_OUT_OF_MEMORY;

	while (__token_comm_send(__TC_MSG_GREET, &__token_comm_address, 1) != TOKEN_COMM_SUCCESS)
	{

	}
}

function __token_comm_qstatus()
{
	local channel;
	local power;
	local status;
	kaicomm_rci_query_status(__token_comm_hardware_id, &channel, &power, &status);
	return status;
}

function __token_comm_send(type, payload, size)
{
	local status = __token_comm_qstatus();

	// Keep this send from obliterating a receiving packet.
	if (status & KAICOMM_RCI_RECEIVING != 0)
		return TOKEN_COMM_WAIT;

	if (status & KAICOMM_RCI_TRANSMITTING != 0)
		return TOKEN_COMM_WAIT;

	*__token_comm_send_buffer = type;
	memcpy(__token_comm_send_buffer + 1, payload, size);

	kaicomm_rci_send(
		__token_comm_hardware_id, 
		__token_comm_send_buffer,
		size,
		&status);

	if (status == 0) return TOKEN_COMM_SUCCESS;
	return TOKEN_COMM_SEND_FAIL;
}

#endif
