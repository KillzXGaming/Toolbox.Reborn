using System.Runtime.InteropServices;

namespace GCNLibrary.LM.MDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Shape
    {
        public byte NormalFlags = 0x1;
        public byte Unknown1;
        public byte Unknown2 = 0x26;
        public byte Unknown3 = 0x0;
        public ushort PacketCount;
        public ushort PacketBeginIndex;
    }
}
