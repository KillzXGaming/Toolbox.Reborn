using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core.GX;
using OpenTK;

namespace GCNLibrary.LM.BIN
{
    public class ShapeBatch 
    {
        public ushort TriangleCount { get; set; }
        public ushort DisplayListSize { get; set; }
        public GXAttributes AttributeFlags { get; set; }

        public byte NormalsFlag { get; set; }
        public byte PositionsFlag { get; set; }
        public byte TexCoordFlag { get; set; }
        public byte NbtFlag { get; set; }

        public List<PacketData> Packets = new List<PacketData>();

        public ShapeBatch() { }

        public ShapeBatch(FileReader reader, BIN_Parser header)
        {
            TriangleCount = reader.ReadUInt16();
            DisplayListSize = reader.ReadUInt16();
            AttributeFlags = (GXAttributes)reader.ReadUInt32();
            NormalsFlag = reader.ReadByte();
            PositionsFlag = reader.ReadByte();
            TexCoordFlag = reader.ReadByte();
            NbtFlag = reader.ReadByte();
            uint dataOffset = reader.ReadUInt32();

            reader.SeekBegin(header.ShapeBatchOffset + dataOffset);
            ParsePacketData(reader, header);
        }

        private void ParsePacketData(FileReader reader, BIN_Parser header)
        {
            long pos = reader.Position;
            while (reader.Position < pos + (DisplayListSize * 0x20))
            {
                byte opCode = reader.ReadByte();
                if (opCode == 0)
                    continue;

                PacketData packet = new PacketData();
                packet.OpCode = opCode;
                Packets.Add(packet);

                ushort vertexCount = reader.ReadUInt16();
                packet.Vertices = new VertexGroup[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    VertexGroup group = new VertexGroup();
                    group.PositionIndex = reader.ReadInt16();
                    if (AttributeFlags.HasFlag(GXAttributes.Normal))
                    {
                        group.NormalIndex = reader.ReadInt16();
                        if (NbtFlag != 0)
                        {
                            group.BinormalIndex = reader.ReadInt16();
                            group.TangentIndex = reader.ReadInt16();
                        }
                    }

                    if (AttributeFlags.HasFlag(GXAttributes.Color0))
                        group.Color0Index = reader.ReadInt16();

                    if (AttributeFlags.HasFlag(GXAttributes.Color1))
                        group.Color1Index = reader.ReadInt16();

                    for (int t = 0; t < TexCoordFlag; t++)
                    {
                        if (AttributeFlags.HasFlag((GXAttributes)(1 << (13 + t))))
                            group.TexCoordIndex[t] = reader.ReadInt16();
                    }

                    //Parse data

                    using (reader.TemporarySeek(header.PositionOffset + (group.PositionIndex * 6), System.IO.SeekOrigin.Begin))
                    {
                        group.Position = new Vector3(
                            (float)reader.ReadInt16(),
                            (float)reader.ReadInt16(),
                            (float)reader.ReadInt16());
                    }

                    if (group.NormalIndex != -1)
                    {
                        using (reader.TemporarySeek(header.NormalOffset + (group.NormalIndex * 12), System.IO.SeekOrigin.Begin))
                        {
                            group.Normal = new Vector3(
                                (float)reader.ReadSingle(),
                                (float)reader.ReadSingle(),
                                (float)reader.ReadSingle());
                        }
                    }

                    if (group.TangentIndex != -1)
                    {
                        using (reader.TemporarySeek(header.NormalOffset + (group.TangentIndex * 12), System.IO.SeekOrigin.Begin))
                        {
                            group.Tangent = new Vector3(
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle());
                        }
                    }

                    if (group.BinormalIndex != -1)
                    {
                        using (reader.TemporarySeek(header.NormalOffset + (group.BinormalIndex * 12), System.IO.SeekOrigin.Begin))
                        {
                            group.Binormal = new Vector3(
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle());
                        }
                    }

                    if (group.TexCoordIndex[0] != -1)
                    {
                        using (reader.TemporarySeek(header.TexCoordOffset + (group.TexCoordIndex[0] * 8), System.IO.SeekOrigin.Begin))
                        {
                            //Flip UVs 
                            group.Texcoord = new Vector2(
                                (float)reader.ReadSingle(),
                                (float)reader.ReadSingle());
                        }
                    }

                    packet.Vertices[i] = group;
                }
            }
        }

        public void Write(FileWriter writer, BIN_Parser header)
        {
            writer.Write(TriangleCount);
            writer.Write(DisplayListSize);
            writer.Write((uint)AttributeFlags);
            writer.Write(NormalsFlag);
            writer.Write(PositionsFlag);
            writer.Write(TexCoordFlag);
            writer.Write(NbtFlag);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
        }

        public class PacketData
        {
            public byte OpCode = 0x98;
            public VertexGroup[] Vertices;

            public void Write(ShapeBatch batch, FileWriter writer, BIN_Parser header)
            {
                writer.Write(OpCode);
                writer.Write((ushort)Vertices.Length);
                for (int i = 0; i < Vertices.Length; i++)
                {
                    writer.Write(Vertices[i].PositionIndex);
                    if (batch.AttributeFlags.HasFlag(GXAttributes.Normal))
                    {
                        writer.Write(Vertices[i].NormalIndex);
                        if (batch.NbtFlag != 0)
                        {
                            writer.Write(Vertices[i].BinormalIndex);
                            writer.Write(Vertices[i].TangentIndex);
                        }
                    }

                    if (batch.AttributeFlags.HasFlag(GXAttributes.Color0))
                        writer.Write(Vertices[i].Color0Index);

                    if (batch.AttributeFlags.HasFlag(GXAttributes.Color1))
                        writer.Write(Vertices[i].Color1Index);

                    for (int t = 0; t < batch.TexCoordFlag; t++)
                    {
                        if (batch.AttributeFlags.HasFlag((GXAttributes)(1 << (13 + t))))
                            writer.Write(Vertices[i].TexCoordIndex[t]);
                    }
                }
            }
        }

        public class VertexGroup
        {
            public short PositionIndex;
            public short NormalIndex = -1;
            public short TangentIndex = -1;
            public short BinormalIndex = -1;
            public short[] TexCoordIndex = new short[8] { -1,-1,-1,-1,-1,-1,-1,-1 };

            public short Color0Index = -1;
            public short Color1Index = -1;

            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector3 Tangent { get; set; }
            public Vector3 Bitangent { get; set; }
            public Vector3 Binormal { get; set; }
            public Vector2 Texcoord { get; set; }

            public Vector4 Color0 { get; set; }
            public Vector4 Color1 { get; set; }
        }
    }
}
