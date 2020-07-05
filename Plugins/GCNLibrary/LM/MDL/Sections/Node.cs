using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GCNLibrary.LM.MDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Node
    {
        public ushort NodeIndex;
        public ushort ChildIndex;
        public ushort SiblingIndex;
        public ushort Unknown3; //0
        public ushort ShapeCount;
        public ushort ShapeIndex;
        public uint Padding;
    }
}
