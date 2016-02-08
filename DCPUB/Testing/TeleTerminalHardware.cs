using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Testing
{
    public class TeleTerminalHardware : HardwareDevice
    {
        public List<ushort> Output = new List<ushort>();

        public uint ManufacturerID { get { return 0xDEADBEEF; } }
        public uint HardwareID {  get { return 0xCAFE0000; } }
        public ushort Version {  get { return 0xF005; } }

        public void OnAttached(Emulator emu)
        {

        }

        public void OnInterrupt(Emulator emu)
        {
            Output.Add(emu.registers[(int)Registers.B]);
        }
    }
}
