using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.GX
{
    public class GXVertexLayout
    {
        public GXAttributes Attribute { get; set; }
        public GXComponentType CompType { get; set; }
        public GXAttributeType AttType { get; set; }

        public uint DataOffset { get; set; }

        public float Divisor { get; set; }

        public GXVertexLayout() { }

        public GXVertexLayout(GXAttributes attribute,
            GXComponentType comptype, GXAttributeType attType, uint offset)
        {
            Attribute = attribute;
            CompType = comptype;
            AttType = attType;
            DataOffset = offset;
        }
    }
}
