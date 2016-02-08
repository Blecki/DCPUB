
namespace DCPUB.Testing
{
    public interface HardwareDevice
    {
        uint ManufacturerID { get; }
        uint HardwareID { get; }
        ushort Version { get; }

        void OnAttached(Emulator emu);
        void OnInterrupt(Emulator emu);
    }
}
