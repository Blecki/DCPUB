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

Behaviours
----

VTACI have 3 mode of operation:
 - Disable mode - thrusters off
 - Group Control - memory control of individual thruster or group of thruster
 - Moment+Force Mode - memory control of automatic group of thruster

**IMPORTANT** *VTACI must run calibrate before first use or if thruster added.*

**IMPORTANT** *Thruster should not be attach more than 32m from center of mass*
*for Moment+Force mode. or 32m from VTACI in total.*

VTACI will calibrate for thrusters offset from center of mass if possible,
otherwise calibrate for thrusters offset from center of VTACI.

##### Moment+Force Mode

VTACI will automatically compute what thrusters should fire for moment or force.
Output rate for force and moment are added and clamped to maximum.

VTACI will read 6 words of memory at *base address* for this mode:
 - B+0: X vector of force
 - B+1: Y vector of force
 - B+2: Z vector of force
 - B+3: X vector of moment
 - B+4: Y vector of moment
 - B+5: Z vector of moment

For force: X+ is "right", Y+ is "forward", Z+ is "up".
For moment: vector+ is clockwise towards axis+.

Each force vector will fire all thrusters with opposite axis direction.

Each moment vector will fire all thrusters on all side of axis that will contribute
moment force.

As example: Y+ moment would apply to:
 - X+ thrusters below Z plane
 - X- thrusters above Z plane
 - Z+ thrusters right X plane
 - Z- thrusters left X plane

All vectors can scale thrust output levels from 0% to 100%.
Force+Moment mode will center all gimbles and not use them.


##### Group Control Mode

Mode for larger craft using VTACI, gimble control, or just more direct control.

VTACI will read *max offset* memory words starting at *base address*.
Each thruster will read 1 word at assigned offset, that is scaler output level.
0 = Off, 65535 = Full On. all thruster with same offset get same value.
Each gimble will read 2 words, that is vector direction, X then Y.
Numbers -32767 is full axis minus, 0 is center, 32767 is axis plus.
Number -32768 is also full axis minus.
All gimble with same offset get same vector direction.

**IMPORTANT** *base address is use for both gimble and thrusters. Offset*
*of gimble and thruster overlapping can be undesired.*

**IMPORTANT** *not setting offsets before using this mode can be undesired.*

**IMPORTANT** *Larger max offset will reduce respone time.*

-----

Distributed for VTACI devices by Rin Yu Research Group.

**WARNING** *Use of device for any non-research activity may void any warrenty.*

```
  "Knowledge helps you make a living; wisdom helps you make a life."
     -- Director Rin Yu.
```


#endif