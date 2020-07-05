using System.Runtime.InteropServices;

namespace GCNLibrary.LM.MDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Sampler
    {
        public ushort TextureIndex = ushort.MaxValue;
        public ushort Unknown2 = ushort.MaxValue; //Palette index? Unsure if any are used
        public byte WrapModeU = 1;
        public byte WrapModeV = 2;
        public byte MinFilter;
        public byte MagFilter;  
    }
}
