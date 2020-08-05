using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1.Model
{
    public enum ChunkOperation
    {
        Header,

        VertexPosition = 0x0010,
        VertexNormal = 0x0011,
        VertexNBT = 0x0012,
        VertexColor = 0x0013,

        VertexUV0 = 0x0018,
        VertexUV1 = 0x0019,
        VertexUV2 = 0x001A,
        VertexUV3 = 0x001B,
        VertexUV4 = 0x001C,
        VertexUV5 = 0x001D,
        VertexUV6 = 0x001E,
        VertexUV7 = 0x001F,

        Texture = 0x0020,
        TextureAttribute = 0x0022,
        Material = 0x0030,

        SkinningIndices = 0x0040,

        Envelope = 0x0041,

        Mesh = 0x0050,

        Joint = 0x0060,
        JointName = 0x0061,

        CollisionPrism = 0x0100,
        CollisionGrid = 0x0110,

        EoF = 0xFFFF
    }

    public interface IModChunk
    {
        void Read(FileReader reader);
        void Write(FileWriter writer);
    }

    public class ChunkSection
    {
        public long Position { get; set; }
        public uint Size { get; set; }
        public ChunkOperation OpCode { get; set; }

        public ChunkSection(long pos, uint size, int opcode) {
            Position = pos;
            Size = size;
            OpCode = (ChunkOperation)opcode;
        }
    }
}
