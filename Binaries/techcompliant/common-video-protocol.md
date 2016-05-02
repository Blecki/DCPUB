
    _________.________________.      __.___  ____________________
    \______   \   \__    ___/  \    /  \   |/   _____/\_   _____/
     |    |  _/   | |    |  \   \/\/   /   |\_____  \  |    __)_ 
     |    |   \   | |    |   \        /|   |/        \ |        \
     |______  /___| |____|    \__/\  / |___/_______  //_______  /
            \/                     \/              \/         \/ 

       (c) BITWISE SCIENCES 1984 - 2108


BITWISE COMMON VIDEO PROTOCOL

Presented here is a Video Protocol designed to be backwards compatible
with the ever popular NE LEM1802. LEM functions should not collide with
this specification, and a compatible device should identify itself as a
LEM1802 device. The device will report additional capabilities using 
interrupt 0x0004. 

In addition to LEM modes, a compatible device will implement these 
interrupts.

This interface supports any device of a resolution up to 1024*1024 pixels
at up to 32 bits per pixel in two modes. In pixel mode, each pixel on the
screen can be set individually. In cell mode, the device operates the
same as a LEM1802 device and can use the same font glyph system. However,
a device may support far higher resolutions and number of possible glyphs.

0x0004: Report Capabilities
	B should contain the address in DCPU memory to begin writing display
		mode descriptors.
	C should contain the maximum number of descriptors to write. If the
		program does not provide enough memory to record all modes the 
		display is capable of, some modes may not be detected.

	Up to C descriptors are written to memory begining at B. A descriptor 
	is two words long. The first 10 bits describe the width of the display
	in pixels or cells. The next 10 bits describe the height. Five bits describe the number of bits per pixel. 1 bit describes the type of 
	mode, either pixel or cell. The final 6 bits are reserved for future
	expansion. The display modes must be written in order and are indexed
	from zero.

0x0005: Set display mode
	B should contain the first word of the display mode descriptor to set.
	C should contain the second word.

	The display mode determines the number of words needed for mapped 
	memory. Use the memory map interrupt from the LEM spec to assign
	memory to the display device.

