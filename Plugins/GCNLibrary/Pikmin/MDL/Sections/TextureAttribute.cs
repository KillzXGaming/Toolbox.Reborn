using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GCNLibrary.Pikmin1.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TextureAttribute
    {
        public ushort TextureIndex;
        public ushort PaletteIndex;
        public byte WrapS; //Repeat, Clamp, Mirror
        public byte WrapT; //Repeat, Clamp, Mirror
        public ushort Unknown;
        public uint Unknown2;
    }
}
