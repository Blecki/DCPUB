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

#ifndef _BITWISE_SIMPLE_VTACI_
#define _BITWISE_SIMPLE_VTACI_

#include memory.b
#include hardware.b
#include vtaci.b

/*  AC - Look, I'm sick of dealing with this shity japanese company and their shitty hardware.
	Their API blows and half the time the device doesn't even implement it properly. And it's
	at 4.0? Does that mean there are three even worse versions out there? It's a wonder we 
	aren't all dead. Look, bitwise doesn't want me releasing this. They want you to buy
	their thruster controller instead. But we both know the rinyu one is going to be
	cheaper and the bitwise one is going to suck just as much. So there's this.

	Usage:
		Call svtaci_initialize to setup and automatically calibrate the device.
		Once setup, the globals SVTACI_THRUSTER_COUNT and SVTACI_GIMBLE_COUNT hold what you expect.
		Call svtaci_thrust to set a thruster's... thrust.
*/

static SVTACI_THRUSTER_COUNT = 0;
static SVTACI_GIMBLE_COUNT = 0;
static __svtaci_thrust_data = 0;

function svtaci_initialize(memory_page)
{
	local hardware_id = detect_hardware(RINYU_VTACI_HARDWARE_ID);
	// Probably should check to make sure hardware was found.

	rinyu_vtaci_calibrate(hardware_id);

	local status = 0;
	rinyu_vtaci_get_status(hardware_id, &status, &SVTACI_THRUSTER_COUNT, &SVTACI_GIMBLE_COUNT);

	// Check if device is ready??

	__svtaci_thrust_data = allocate_memory(SVTACI_THRUSTER_COUNT + (2 * SVTACI_GIMBLE_COUNT), memory_page);

	local thruster_groups = allocate_memory(SVTACI_THRUSTER_COUNT, memory_page);
	local i = 0;
	while (i < SVTACI_THRUSTER_COUNT)
	{
		thruster_groups[i] = i;
		i += 1;
	}

	rinyu_vtaci_set_thruster_groups(hardware_id, thruster_groups);
	free_memory(thruster_groups, memory_page);

	local gimble_groups = allocate_memory(SVTACI_GIMBLE_COUNT, memory_page);
	local i = 0;
	while (i < SVTACI_GIMBLE_COUNT)
	{
		gimble_groups[i] = i;
		i += 1;
	}

	rinyu_vtaci_set_gimble_groups(hardware_id, gimble_groups);
	free_memory(gimble_groups, memory_page);

	rinyu_vtaci_set_thrust_mode(hardware_id, RINYU_VTACI_DIRECT_MODE, __svtaci_thrust_data);
}

function svtaci_thrust(thruster_id, thrust)
{
	__svtaci_thrust_data[thruster_id] = thrust;
}

#endif
