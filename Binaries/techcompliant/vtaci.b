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
*
*
*
*	    Vectored Truster Array Control Interface
*
*       \ \\ Rin Yu Research Group
*       /\ /    凜羽研究小組
*/

#ifndef _BITWISE_RINYU_VTACI_
#define _BITWISE_RINYU_VTACI_

static RINYU_VTACI_HARDWARE_ID[2] = { 0xF7F7, 0xEE03 };
static RINYU_VTACI_MANUFACTURER_ID[2] = { 0xC220, 0x0311 };

#define RINYU_VTACI_OFF_MODE 0x0000
#define RINYU_VTACI_DIRECT_MODE 0x0001
#define RINYU_VTACI_VECTOR_MODE 0x0002

#define RINYU_VTACI_STATUS_READY 0x0000
#define RINYU_VTACI_STATUS_BUSY 0x0001
#define RINYU_VTACI_STATUS_CALIBRATE 0x0002
#define RINYU_VTACI_STATUS_DAMAGE 0xFFFF

#define RINYU_VTACI_THRUSTER_OFF 0x0000
#define RINYU_VTACI_THRUSTER_ON 0x0001
#define RINYU_VTACI_THRUSTER_NOT_RESPONDING 0x0100
#define RINYU_VTACI_THRUSTER_OVERHEAT 0x0101
#define RINYU_VTACI_THRUSTER_BAD_GIMBLE 0x0102


struct rinyu_vtaci_thruster_info
{
	x;  // Installed position relative some reference point on the ship.
	y;
	z;
	direction;
	thrust;
}

struct rinyu_vtaci_gimble_info
{
	range;
	thruster;
}


function rinyu_vtaci_emergency_stop(device_id)
{
	asm ( Y = device_id )
	{
		SET A, 0x0000
		HWI Y
	}
}

function rinyu_vtaci_calibrate(device_id)
{
	asm ( Y = device_id )
	{
		SET A, 0x0001
		HWI Y
	}
}

function rinyu_vtaci_get_status(
	device_id,
	out_status,
	out_thrusters,
	out_gimbles)
{	
	asm ( Y = device_id,
		I = out_status,
		X = out_thrusters,
		Z = out_gimbles )
	{
		SET A, 0x0002
		HWI Y
		SET [I], A
		SET [X], B
		SET [Z], C
	}
}

function rinyu_vtaci_set_thrust_mode(
	device_id,
	in_thrust_mode,
	in_control_data)
{
	asm ( Y = device_id,
		B = in_control_data,
		C = in_thrust_mode )
	{
		SET A, 0x0003
		HWI Y
	}
}

function rinyu_vtaci_query_thrusters(
	device_id,
	out_thruster_data,
	out_thruster_count,
	out_status)
{
	asm ( Y = device_id;
		B = out_thruster_data;
		I = out_thruster_count;
		X = out_status )
	{
		SET A, 0x0004
		HWI Y
		SET [I], A
		SET [X], C
	}
}
 
function rinyu_vtaci_query_gimbles(
	device_id,
	out_gimble_data)
{
	asm ( Y = device_id;
		B = out_gimble_data)
	{
		SET A, 0x0005
		HWI Y
	}
}

function rinyu_vtaci_set_thruster_groups(
	device_id,
	in_group_data)
{
	asm ( Y = device_id;
		B = in_group_data )
	{
		SET A, 0x0006
		HWI Y
	}
}

function rinyu_vtaci_set_gimble_groups(
	device_id,
	in_group_data)
{
	asm ( Y = device_id;
		B = in_group_data )
	{
		SET A, 0x0007
		HWI Y
	}
}

function rinyu_vtaci_query_thruster_status(
	device_id,
	out_thruster_status,
	out_error_thrusters)
{
	asm ( Y = device_id;
		B = out_thruster_status;
		I = out_error_thrusters )
	{
		SET A, 0x0008
		HWI Y
		SET [I], A
	}
}

function rinyu_vtaci_reset(
	device_id)
{
	asm ( Y = device_id )
	{
		SET A, 0xFFFF
		HWI Y
	}
}

struct rinyu_vtaci_force_data
{
	force_x;
	force_y;
	force_z;
	moment_x;
	moment_y;
	moment_z;
}

#endif
