// Implements KaiComm Radiofrequency Communication Interface

#ifndef TECHCOMPLIANT_LIB_RCI
#define TECHCOMPLIANT_LIB_RCI

static RCI_HARDWARE_ID[2] = { 0xD005, 0x90A5 };
static RCI_MANUFACTURER_ID[2] = { 0xA87C, 0x900E };

#define RCI_RECEIVE_BUFFER_EMPTY 	0x0000
#define RCI_RECEIVE_BUFFER_DATA 	0x0001
#define RCI_RADIO_IDLE			0x0000
#define RCI_RADIO_RECEIVING 		0x0002
#define RCI_RADIO_TRANSMITTING		0x0004
#define RCI_RADIO_COLLISION		0x0006
#define RCI_ANTENNA_FAILURE		0xFFE0
#define RCI_HARDWARE_FAILURE		0xFFF0

#define RCI_SEND_SUCCEED		0x0000
#define RCI_SEND_FAIL			0x0001

#define RCI_SETTINGS_VALID		0x0000
#define RCI_SETTINGS_INVALID		0x0001

function rci_enumerate(
	rci_transmitters, // Pointer to at least max_transmitters words of memory.
	max_transmitters)
{
	local num_hardware = 0;
	local count_found = 0;

	asm ( B = &num_hardware )
	{
		HWN [B]
	}

	local n = 0;
	while ( n < num_hardware )
	{
		local hardware_id[2];

		asm ( A = n; I = hardware_id )
		{
			HWQ A
			SET [I + 0x0001], A
			SET [I], B
		}

		if ( hardware_id[0] == RCI_HARDWARE_ID[0] && 
			hardware_id[1] == RCI_HARDWARE_ID[1] )
		{
			rci_transmitters[count_found] = n;
			count_found += 1;
			if ( count_found == max_transmitters )
					return count_found;
		}

		n += 1;
	}

	return count_found;
}

function rci_query_status( 
	device_id,   // Id of the rci device
	out_channel, // Pointer where current channel is written.
	out_power )  // Pointer where current power is written.
	// Returns status code of device.
{
	local status = 0x0000;

	asm ( Y = device_id,
		I = out_channel,
		J = out_power,
		X = &status )
	{
		SET A, 0
		HWI Y
		SET [I], A
		SET [J], B
		SET [X], C
	}

	return status;
}

function rci_receive(
	device_id, // Id of the rci device
	out_buffer, // Pointer to 256 words where data is written
	out_size ) // Pointer to word where size read is written
	// Returns status code of device.
{
	local status = 0x0000;

	asm ( Y = device_id,
		B = out_buffer,
		I = out_size,
		J = &status )
	{
		SET A, 1
		HWI Y
		SET [I], B
		set [J], C
	}

	return status;
}

function rci_send(
	device_id, // Id of the rci device
	in_buffer, // Pointer to 256 words of data to transmit
	in_size )  // Size of data to transmit
{
	local status = 0x0000;

	asm ( Y = device_id,
		B = in_buffer,
		C = in_size,
		I = &status)
	{
		SET A, 2
		HWI Y
		SET [I], C
	}

	return status;
}

function rci_tune(
	device_id, // Id of the rci device
	in_channel, // Channel to tune to
	in_power ) // Power to tune to
{
	local status = 0x0000;

	asm ( Y = device_id,
		B = in_channel,
		C = in_power,
		I = &status )
	{
		SET A, 3
		HWI Y
		SET [I], C
	}

	return status;
}

function rci_configure_interrupts(
	device_id, // Id of the rci device
	in_receive_message,
	in_send_message )
{
	asm ( Y = device_id,
		B = in_receive_message,
		C = in_send_message )
	{
		SET A, 4
		HWI Y
	}

	return 0;
}

#endif

