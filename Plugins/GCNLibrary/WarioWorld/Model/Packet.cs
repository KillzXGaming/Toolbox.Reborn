using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;

namespace GCNLibrary.WW
{
    public class Packet
    {
        public uint Offset { get; set; }
        public uint Size { get; set; }
        public byte Flags1 { get; set; }
        public byte Flags2 { get; set; }
        public sbyte MaterialColorIndex { get; set; }
        public sbyte TextureIndex { get; set; }
    }
}
