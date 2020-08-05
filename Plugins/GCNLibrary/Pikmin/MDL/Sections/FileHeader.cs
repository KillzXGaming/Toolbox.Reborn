using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GCNLibrary.Pikmin1.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileHeader
    {
        public ushort Year;
        public byte Month;
        public byte Day;
        public int SystemFlags;
    }
}
