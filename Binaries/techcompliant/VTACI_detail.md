Vectored Truster Array Control Interface (The Missing guide)
-----

```
  \ \\ Rin Yu Research Group
  /\ /    凜羽研究小組
```
Rin Yu Research is a small firm that makes experimental propulsion systems.
The name is Chinese, and the spec writing division (for better (cheaper!) or worse) is vague and not completely comprehensable.
Thus, the grammar of the spec is bad on purpose (but should be consistently bad).

|     Item       |   Value    |   Comment
| -------------: | ---------- | ----------------
|    Vendor code | 0xC2200311 | Rin Yu Research
|      Device ID | 0xF7F7EE03 | VTACI
|    Device type | 0xF7F7     | Trust Control Card
|        Version | 0x0400     | Version 4.0

The VTACI device is classified as "non-standard", does not support any standard API.
The device can read and write directly to memory, and can be reset with `SET A,-1`; `HWI n`.

The company uses semver, in a 4.4.8 bit format, effectively actaully making this version 0.4.00
but they don't want people to think it's an alpha product for marketing reasons.

- The device has 3 different interrupt functions that will shutdown all the thrusters:
  - A=0 an explicit safety shutdown/panic, retains any group configuration.
  - A=3 C=0 (Set mode: disable) the mode configuration, mode 0 is the boot up default and will disable all thrust.
  - A=-1 (aka 0xffff) API compatible reset, returns to mode 0, and also deletes any group configuration.

### Coordinates and Calibration

The device follows the notion of "calibration". This semi magic process takes some time
proportional to the number of thrusters connected to it, and basically just records the
location and orientation of all the thrusters connected to the device.
Power cycling, resetting, software crashing, etc. will not invalidate this information,
however, adding new thrusters or when using a new instance of the device will require it.

All thrusters connected to the VTACI controller have a unique index, this can change only
during a calibration request, but generally should remain the same between calibrations.
All indexes should not change if no thrusters were added or removed.

The location and orientation are based on the local coordinates of the space craft,
and assume a right handed, Z up coordinate space. These values are signed
16 bit integers that are measured in millimeters.

The spec requires that all thrusters be connected no farther than **32 meters** from
the VTACI device's center, where the VTACI sensor/controller is assumed to be a physical
device attached to an interface card in a DCPU system with a (propriatary) cable.
This numeric limit comes from the signed 16 bit range of -32768 to 32767.
This isn't actually straight line distance, but as no single XYZ coordinate can exceed this range,
it actually forms an axis aligned cube centered on the device that all thrusters must be within.
Connecting a thruster outside of this cube to the device will cause the device to enter the error state.
Note however, that a thruster within the cube, but more than 32m from the device is still considered valid.

During the "calibration", the device will test if all the thrusters are within the cube as a pass/fail,
then it will test if the thruster is within a similar cube that is instead centered on the space craft's
local center of mass. Any thruster that is within the center of mass cube, has it's location saved as an
offset from the center of mass, it is also flagged as being "center of mass" relative. Any thruster that is
not in the center of mass cube, but still valid, has it's location offset saved relative to the center of
the physical VTACI device, and is flagged as being "VTACI" relative.

If *any* thruster is flagged as being "VTACI" relative, and any attempt is made to enter "force+moment" mode,
then this will cause the device to soft crash (error 0xffff). The mode change obviously fails.

The device will get the normalized vector representing the direction the thruster will exhaust.
This vector is relative to the local coordinate space of the *space craft*, in the same RH Z-up coords.
If a thruster is mounted perpendicular on the right side of a cube space craft, this will be X=1,Y=0,Z=0.

The device also supports optional gimbles on thrusters, the orientation is based on a centered gimble.
The location for thrusters on gimbles, is assumed to be the pivet point of the gimble.
The device does not collect the rotation of gimbles, making them troublesome for software to take advantage
of when using a VTACI to control them. Gimble support at all is considered a "bonus feature" on the device.

Neither the position or orientation change during normal operation (setting gimbles or thrust levels),
all of this information is collected when calibration is requested and should be saved in the device's state.

### Accessing Calibration Data

Interrupt commands A=4 and A=5 are used to write the internal information about the
thruster(s) and gimble(s) respecively, to the DCPU's RAM for a program to evaluate (or choke on)

For thruster information (A=4), an array of 5 word records is written starting at a memory pointer (register B)
Each record represents a single thruster, and consists of:
 - the 3D signed 16 bit integer location vector.
 - the 3D de-normalized orientation vector (encoded)
 - A flag representing if the location vector is offset from the center of mass (0) or VTACI (1) local space.
 - The maximum rated thrust output of the thruster (in a vacuum) represented as a magnitude and exponent.

   The range of trust is 1 micro newton all the way to 4.095 giga newtons (quite a lot!)
   with at least 3 significat digits.

the de-normalized orientation is calculated by taking each normallized vector component, multiplying by 8, adding 8,
then clamping within a 4 bit range. The three resulting XYZ components are then combined with the location flag,
and the resulting word is saved to the RAM location.

### Memory Refresh

In both "group control mode" and "moment+force mode" the device continuously reads it's assigned segment of
memory and then updates all thrusters at once. The refresh time is proportional to the number of words
that would have to be read when starting at the base address, continuing to the highest offset.
The minimum is 2 words in group control mode, and is always 6 words in force mode.
The maximum number of words that would be read is the max of:
 - the highest thruster group offset
 - or, the highest gimble group offset plus one.

The update rate should probably be 15 milliseconds plus about 0.02 milliseconds per word for group mode.
For moment+force mode, about 20 milliseconds plus 0.10 milliseconds per thruster for an update cycle.
Obviously, idle mode (mode 0) should not automatically access RAM at all.

Every thruster has an assigned offset, this defaults to zero at reset. In group mode, an array from
memory starting at the set base address is read, all thrusters use their offset to get a setting
from the array, so thrusters with the same offset read the same location and get the same setting.

### "Force" and "Moment" control mode

Force and Moment control - sounds likes a fancy term with some complex algorithm backing it, right? well, no.
The cheap integer processors used in the VTACI make actual estimation of any real unit of force or moment
far to computationally expensive to be practical.

### The Algorithm

The "Force" and "Moment" are computed seperately, the resulting thruster levels are then added together.

*side note here, this is completely untested and could very well be wrong*

Both modes use an orientation comparison function, this compares a normal to the orientation and
applies a falloff based on the angle. The applied falloff is based on the dot product of the
input normal and the orientation normal. The result is scaled so 1.0 is 100% and 0.4 is 0% thrust.
`Scaled = (x - 0.4) * 1.667`

The Force vector is handled in two seperate stages. In the first stage, the 3D signed integer vector
input is negated and normalized (if not zero), this is compared against thruster orientation (above)
for falloff. The magnitude of the force vector is clamped to 32767 equalling 100% thruster output.
In the second stage, each component is evaluated seperately, applying to a single axis, the normal
effectively being that axis. The axis normals are (again) evaluated for falloff and applied to
thrusters. The resulting output thrust levels from these stages are added together. Actual "force"
is completely ignored, having thrusters of different thrust ratings will make no difference in the
applied output thrust levels.

The "Moment" vector is also not quite as complex as it might imply. This vector, is first broken
into the 3 seperate components, and each component is evaluated seperately. The sign of each
component determines which 3D half planes will activate, the magnitude of the component is
normallized to a base percent. Thruster orientations are compared against the normals of the fixed
half planes and those that have a non-zero falloff are considered "active", using the orientation
compare function, the "half planes" have a fixed normal so this can be precomputed and cached.
Additionally a linear fall-off is applied over each axis, the center (0) being 0%, and the absolute
axis coordinate of the farthest "active" thruster, is considered 100%. The normalized megnitude
of the component is combined with the contribution and falloff scales. Each of the X, Y, and Z
components perform these steps seperately, and the resulting final outputs are added together
to get the total for the "moment" output.

- The falloff ramps can be pre-computed during calibration and cached.
- The orientation contribution for planes can be pre-computed and cached.

### Notes From IRC
```
setting Moment+Force mode when thrusters are not near the center of mass will crash the device (error 0xffff)
n+0 thru n+2 are relative coordinates
n+3 is the direction the thruster is mounted and the origin flags
> 0xWZYX
W is the origin flag (Center-of-Mass or VTACI) is only for n+0 through n+2 to tell them apart.
ZYX is the direction the thruster is pointed, all calibration assumes centered gimble, and is always relative to the craft.
only position has different origins.
thrusters may or may not have gimbles.
Q: if you turn the gimble does the n+3 (A=0x0004) change?
A: no, thruster information is considered "calibration data" it does not change during operation. (except during calibration)
Q: is the VTACI automatically updating thruster levels from memory all the time in group control mode?
A: yes (subject to the refresh rate) - more groups/offsets = slower refresh/update rate.
fuel levels have to be checked via seperate sensors.
(recently updated) The device reports overheating, binary (ok/bad) thruster damage, and gimble malfuction.
(TTT x 10 ^ ( -6 + E )) is the psudo floating point thrust rating, TTT and E are bit field values from the memory word.
you can unmap the VTACI from RAM when it's not being used if you need more free RAM.
X and Y gimble are relative the gimble base, so ... just hope it's installed the "right way up"
  (aka. gimbles on the VTACI are meant to be unpredicatable in software)
  you can't determine a gimble's orientation, the VTACI doesn't supply that information.
The majority of thrusters are assumed to not have a gimble, as the VTACI's focus is more on RCS type systems.
```