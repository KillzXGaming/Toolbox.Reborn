using System.Runtime.InteropServices;
using System.Collections.Generic;
using Toolbox.Core.IO;

namespace GCNLibrary.LM.MDL
{
    public class ShapePacket : ISection
    {
        public uint DataOffset;
        public uint DataSize;
        public ushort Unknown = 2;
        public ushort MatrixIndicesCount;
        public ushort[] MatrixIndices;

        public List<DrawList> DrawLists = new List<DrawList>();

        public byte[] Data;

        public ShapePacket()
        {
            MatrixIndices = new ushort[10];
        }

        public void Read(FileReader reader)
        {
            DataOffset = reader.ReadUInt32();
            DataSize = reader.ReadUInt32();
            Unknown = reader.ReadUInt16();
            MatrixIndicesCount = reader.ReadUInt16();
            MatrixIndices = reader.ReadUInt16s(10);

            using (reader.TemporarySeek(DataOffset, System.IO.SeekOrigin.Begin))
            {
                Data = reader.ReadBytes((int)DataSize);
            }
        }

        public void Write(FileWriter writer)
        {
            writer.Write(DataOffset);
            writer.Write(DataSize);
            writer.Write(Unknown);
            writer.Write(MatrixIndicesCount);
            writer.Write(MatrixIndices);
        }

        public byte[] CreateDrawList(List<DrawList> drawLists, bool IsLOD, bool hasNormals, bool hasTexCoords, bool hasColors)
        {
            var mem = new System.IO.MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                writer.SetByteOrder(true);
                foreach (var drawList in drawLists)
                {
                    writer.Write(drawList.OpCode);
                    writer.Write((ushort)drawList.Vertices.Count);
                    for (int v = 0; v < drawList.Vertices.Count; v++)
                    {
                        var vertex = drawList.Vertices[v];

                        if (IsLOD)
                        {
                            writer.Write(vertex.MatrixIndex);
                            writer.Write(vertex.PositionIndex);
                            writer.Write((byte)0);
                        }
                        else
                        {
                            writer.Write(vertex.MatrixIndex);
                            writer.Write(vertex.Tex0MatrixIndex);
                            writer.Write(vertex.Tex1MatrixIndex);
                            writer.Write(vertex.PositionIndex);
                            if (hasNormals)
                                writer.Write(vertex.NormalIndex);
                            if (hasColors)
                                writer.Write(vertex.ColorIndex);
                            if (hasTexCoords)
                                writer.Write(vertex.TexCoordIndex);
                        }
                    }
                }
            }
            return mem.ToArray();
        }

        public class DrawList
        {
            public List<VertexGroup> Vertices = new List<VertexGroup>();
            public byte OpCode { get; set; } = 0x98;
        }

        public class VertexGroup
        {
            public ushort MatrixDataIndex { get; set; }

            public sbyte MatrixIndex { get; set; } = -1;
            public sbyte Tex0MatrixIndex { get; set; } = -1;
            public sbyte Tex1MatrixIndex { get; set; } = -1;

            public short PositionIndex { get; set; } = -1;
            public short NormalIndex { get; set; } = -1;
            public short TangentIndex { get; set; } = -1;
            public short BinormalIndex { get; set; } = -1;
            public short ColorIndex { get; set; } = -1;
            public short TexCoordIndex { get; set; } = -1;

            public VertexGroup() { }

            public VertexGroup(VertexGroup src)
            {
                MatrixDataIndex = src.MatrixDataIndex;
                MatrixIndex = src.MatrixIndex;
                Tex0MatrixIndex = src.Tex0MatrixIndex;
                Tex1MatrixIndex = src.Tex1MatrixIndex;
                PositionIndex = src.PositionIndex;
                NormalIndex = src.NormalIndex;
                TangentIndex = src.TangentIndex;
                BinormalIndex = src.BinormalIndex;
                ColorIndex = src.ColorIndex;
                TexCoordIndex = src.TexCoordIndex;
            }
        }
    }
}
