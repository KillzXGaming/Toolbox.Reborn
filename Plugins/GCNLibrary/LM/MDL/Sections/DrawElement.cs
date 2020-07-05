using System.Runtime.InteropServices;

namespace GCNLibrary.LM.MDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class DrawElement 
    {
        public ushort MaterialIndex;
        public ushort ShapeIndex;
    }
}
