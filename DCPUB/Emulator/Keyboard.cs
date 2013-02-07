﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Emulator
{
    /*
     Name: Generic Keyboard (compatible)
ID: 0x30cf7406
Version: 1

Interrupts do different things depending on contents of the A register:

 A | BEHAVIOR
---+----------------------------------------------------------------------------
 0 | Clear keyboard buffer
 1 | Store next key typed in C register, or 0 if the buffer is empty
 2 | Set C register to 1 if the key specified by the B register is pressed, or
   | 0 if it's not pressed
 3 | If register B is non-zero, turn on interrupts with message B. If B is zero,
   | disable interrupts
---+----------------------------------------------------------------------------

When interrupts are enabled, the keyboard will trigger an interrupt when one or
more keys have been pressed, released, or typed.

Key numbers are:
	0x10: Backspace
	0x11: Return
	0x12: Insert
	0x13: Delete
	0x20-0x7f: ASCII characters
	0x80: Arrow up
	0x81: Arrow down
	0x82: Arrow left
	0x83: Arrow right
	0x90: Shift
	0x91: Control

     */

    public class Keyboard : HardwareDevice
    {
        public uint ManufacturerID { get { return 0; } }
        public uint HardwareID { get { return 0x30cf7406; } }
        public ushort Version { get { return 1; } }

        public KeyboardWindow window = null;

        public bool[] keyState = new bool[0x92];

        public Keyboard()
        {
            window = new KeyboardWindow();
            window.Show();
        }

        public void OnAttached(Emulator emu)
        {

        }

        public void OnInterrupt(Emulator emu)
        {
            throw new NotImplementedException();
        }
    }
}
