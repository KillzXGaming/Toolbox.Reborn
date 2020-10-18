using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;
using CTRLibrary;

namespace CTRLibrary.Grezzo
{
    public class CMB_Parser
    {
        public Header FileHeader;

        public CMB_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream)) {
                FileHeader = new Header(reader);
            }
        }

        public enum CMBVersion
        {
            OOT3DS,
            MM3DS,
            LM3DS,
        }

        public class Header
        {
            public string Name { get; set; }

            public CMBVersion Version;

            public uint ChunkCount; //Fixed count per game

            public uint Unknown;

            public SectionData SectionData;

            public Header(FileReader reader)
            {
                string magic = reader.ReadSignature(4, "cmb ");
                uint FileSize = reader.ReadUInt32();
                ChunkCount = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();

                Name = reader.ReadString(0x10).TrimEnd('\0');

                //Check the chunk count used by the game
                if (ChunkCount == 0x0F)
                    Version = CMBVersion.LM3DS;
                else if (ChunkCount == 0x0A)
                    Version = CMBVersion.MM3DS;
                else if (ChunkCount == 0x06)
                    Version = CMBVersion.OOT3DS;
                else
                    throw new Exception("Unexpected chunk count! " + ChunkCount);

                SectionData = new SectionData();
                SectionData.Read(reader, this);
            }

            public void Write(FileWriter writer)
            {
                writer.WriteSignature("cmb ");
                writer.Write(uint.MaxValue); //Reserve space for file size offset
                writer.Write(ChunkCount);
                writer.Write(Unknown);
                writer.WriteString(Name, 0x10);

                SectionData.Write(writer, this);

                //Write the total file size
                using (writer.TemporarySeek(4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)writer.BaseStream.Length);
                }
            }
        }

        public class SectionData
        {
            public SkeletonChunk SkeletonChunk;
            public QuadTreeChunk QuadTreeChunk;
            public MaterialChunk MaterialChunk;

            public TextureChunk TextureChunk;
            public SkeletalMeshChunk SkeletalMeshChunk;
            public LUTSChunk LUTSChunk;
            public VertexAttributesChunk VertexAttributesChunk;

            public void Read(FileReader reader, Header header)
            {
                uint numIndices = reader.ReadUInt32();
                SkeletonChunk = ReadChunkSection<SkeletonChunk>(reader, header);
                if (header.Version >= CMBVersion.MM3DS)
                    QuadTreeChunk = ReadChunkSection<QuadTreeChunk>(reader, header);

                MaterialChunk = ReadChunkSection<MaterialChunk>(reader, header);
                TextureChunk = ReadChunkSection<TextureChunk>(reader, header);
                SkeletalMeshChunk = ReadChunkSection<SkeletalMeshChunk>(reader, header);
                LUTSChunk = ReadChunkSection<LUTSChunk>(reader, header);
                VertexAttributesChunk = ReadChunkSection<VertexAttributesChunk>(reader, header);

                uint indexBufferOffset = reader.ReadUInt32();
                uint textureDataOffset = reader.ReadUInt32();

                if (header.Version >= CMBVersion.MM3DS)
                    reader.ReadUInt32(); //Padding?

                if (VertexAttributesChunk != null)
                {
                    long bufferStart = VertexAttributesChunk.StartPosition;
                    foreach (var shape in SkeletalMeshChunk.ShapeChunk.SeperateShapes)
                    {
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.PositionSlice, shape.Position, 3, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.NormalSlice, shape.Normal, 3, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.TangentSlice, shape.Tangent, 3, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.ColorSlice, shape.Color, 4, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.Texcoord0Slice, shape.TexCoord0, 2, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.Texcoord1Slice, shape.TexCoord1, 2, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.Texcoord2Slice, shape.TexCoord2, 2, bufferStart);

                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.BoneIndicesSlice, shape.BoneIndices, shape.boneDimension, bufferStart);
                        ReadVertexDataFromSlice(reader, VertexAttributesChunk.BoneWeightsSlice, shape.BoneWeights, shape.boneDimension, bufferStart);
                    }
                }

                if (indexBufferOffset != 0)
                {
                    foreach (var shape in SkeletalMeshChunk.ShapeChunk.SeperateShapes)
                    {
                        foreach (var prim in shape.Primatives)
                        {
                            foreach (var subprim in prim.Primatives) //Note 3DS usually only has one sub primative
                            {
                                subprim.Indices = new uint[subprim.IndexCount];

                                reader.SeekBegin(indexBufferOffset + subprim.Offset);

                                switch (subprim.IndexType)
                                {
                                    case CmbDataType.UByte:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            subprim.Indices[i] = reader.ReadByte();
                                        break;
                                    case CmbDataType.UShort:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            subprim.Indices[i] = reader.ReadUInt16();
                                        break;
                                    case CmbDataType.UInt:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            subprim.Indices[i] = reader.ReadUInt32();
                                        break;
                                    default:
                                        throw new Exception("Unsupported index type! " + subprim.IndexType);
                                }
                            }
                        }
                    }
                }

                foreach (var tex in TextureChunk.Textures)
                {
                    reader.SeekBegin(textureDataOffset + tex.DataOffset);
                    tex.ImageData = reader.ReadBytes((int)tex.ImageSize);
                }
            }

            private static void ReadVertexDataFromSlice(FileReader reader, BufferSlice Slice, SepdVertexAttribute VertexAttribute, int elementCount, long bufferStart)
            {
                if (Slice == null || Slice.Size == 0)
                    return;

                reader.SeekBegin(bufferStart + VertexAttribute.StartPosition + Slice.Offset);

                int StrideSize = CalculateStrideSize(VertexAttribute.Type, elementCount);
                int VertexCount = (int)Slice.Size / StrideSize;

                VertexAttribute.VertexData = new Syroot.Maths.Vector4F[VertexCount];
                if (VertexAttribute.Mode == SepdVertexAttribMode.ARRAY)
                {
                    for (int v = 0; v < VertexCount; v++)
                    {
                        VertexAttribute.VertexData[v] = ReadVertexBufferData(reader, VertexAttribute, elementCount);
                    }
                }
                else
                {
                    VertexAttribute.VertexData[0] = new Syroot.Maths.Vector4F(
                        VertexAttribute.Constants[0],
                        VertexAttribute.Constants[1],
                        VertexAttribute.Constants[2],
                        VertexAttribute.Constants[3]);
                }

            }

            private static Syroot.Maths.Vector4F ReadVertexBufferData(FileReader reader, SepdVertexAttribute VertexAttribute, int elementCount)
            {
                float[] values = new float[4];
                for (int i = 0; i < elementCount; i++)
                {
                    if (VertexAttribute.Type == CmbDataType.Byte)
                        values[i] = reader.ReadSByte();
                    else if (VertexAttribute.Type == CmbDataType.Float)
                        values[i] = reader.ReadSingle();
                    else if (VertexAttribute.Type == CmbDataType.Int)
                        values[i] = reader.ReadInt32();
                    else if (VertexAttribute.Type == CmbDataType.Short)
                        values[i] = reader.ReadInt16();
                    else if (VertexAttribute.Type == CmbDataType.UByte)
                        values[i] = reader.ReadByte();
                    else if (VertexAttribute.Type == CmbDataType.UInt)
                        values[i] = reader.ReadUInt32();
                    else if (VertexAttribute.Type == CmbDataType.UShort)
                        values[i] = reader.ReadUInt16();
                }

                return new Syroot.Maths.Vector4F(
                    values[0] * VertexAttribute.Scale,
                    values[1] * VertexAttribute.Scale,
                    values[2] * VertexAttribute.Scale,
                    values[3] * VertexAttribute.Scale);
            }

            private static void WriteVertexBufferData(FileWriter writer, long startPos, BufferSlice Slice,
                SepdVertexAttribute VertexAttribute, int elementCount)
            {
                if (Slice == null || VertexAttribute == null)
                    return;

                writer.SeekBegin(startPos + VertexAttribute.StartPosition + Slice.Offset);
                for (int v = 0; v < VertexAttribute.VertexData?.Length; v++)
                {
                    WriteVertexBufferData(writer, VertexAttribute, VertexAttribute.VertexData[v], elementCount);
                }
            }

            private static void WriteVertexBufferData(FileWriter writer, SepdVertexAttribute VertexAttribute,
                Syroot.Maths.Vector4F value, int elementCount)
            {
                float[] values = new float[4] { value.X, value.Y, value.Z, value.W };

                for (int i = 0; i < elementCount; i++)
                {
                    switch (VertexAttribute.Type)
                    {
                        case CmbDataType.Byte:
                            writer.Write((sbyte)values[i]);
                            break;
                        case CmbDataType.Float:
                            writer.Write(values[i]);
                            break;
                        case CmbDataType.Int:
                            writer.Write((int)values[i]);
                            break;
                        case CmbDataType.Short:
                            writer.Write((short)values[i]);
                            break;
                        case CmbDataType.UByte:
                            writer.Write((byte)values[i]);
                            break;
                        case CmbDataType.UInt:
                            writer.Write((uint)values[i]);
                            break;
                        case CmbDataType.UShort:
                            writer.Write((ushort)values[i]);
                            break;
                        default: throw new Exception("Unknown format! " + VertexAttribute.Type);
                    }
                }
            }

            private static int CalculateStrideSize(CmbDataType type, int elementCount)
            {
                switch (type)
                {
                    case CmbDataType.Byte: return elementCount * sizeof(sbyte);
                    case CmbDataType.Float: return elementCount * sizeof(float);
                    case CmbDataType.Int: return elementCount * sizeof(int);
                    case CmbDataType.Short: return elementCount * sizeof(short);
                    case CmbDataType.UByte: return elementCount * sizeof(byte);
                    case CmbDataType.UInt: return elementCount * sizeof(uint);
                    case CmbDataType.UShort: return elementCount * sizeof(ushort);
                    default: throw new Exception("Unknwon format! " + type);
                }
            }

            private uint GetTotalIndexCount()
            {
                uint total = 0;
                foreach (var shape in SkeletalMeshChunk.ShapeChunk.SeperateShapes)
                {
                    foreach (var prim in shape.Primatives)
                    {
                        foreach (var subprim in prim.Primatives) //Note 3DS usually only has one sub primative
                            total += (uint)subprim.Indices.Length;
                    }
                }
                return total;
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.Write(GetTotalIndexCount());
                //Reserve space for all the offses
                writer.Write(0); //SkeletonChunk
                if (header.Version >= CMBVersion.MM3DS)
                    writer.Write(0); //QuadTreeChunk
                writer.Write(0); //MaterialChunk
                writer.Write(0); //TextureChunk
                writer.Write(0); //SkeletalMeshChunk
                writer.Write(0); //LUTSChunk
                writer.Write(0); //VertexAttributesChunk
                writer.Write(0); //indexBufferOffset
                writer.Write(0); //textureDataOffset

                if (header.Version >= CMBVersion.MM3DS)
                    writer.Write(0); //padding or unknown unused section

                //Write sections and offsets
                int _offsetPos = 4;
                if (SkeletonChunk != null)
                {
                    writer.WriteUint32Offset(pos + _offsetPos);
                    SkeletonChunk.Write(writer, header);
                }

                if (QuadTreeChunk != null && header.Version >= CMBVersion.MM3DS)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos += 4));
                    QuadTreeChunk.Write(writer, header);
                }

                _offsetPos += 4;
                if (MaterialChunk != null)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos));
                    MaterialChunk.Write(writer, header);
                }

                _offsetPos += 4;
                if (TextureChunk != null)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos));
                    TextureChunk.Write(writer, header);
                }

                _offsetPos += 4;
                if (SkeletalMeshChunk != null)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos));
                    SkeletalMeshChunk.Write(writer, header);
                }

                _offsetPos += 4;
                if (LUTSChunk != null)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos));
                    LUTSChunk.Write(writer, header);
                }

                _offsetPos += 4;
                if (VertexAttributesChunk != null)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos));
                    long vatrPos = writer.Position;
                    VertexAttributesChunk.Write(writer, header);
                    foreach (var shape in SkeletalMeshChunk.ShapeChunk.SeperateShapes)
                    {
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.PositionSlice, shape.Position, 3);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.NormalSlice, shape.Normal, 3);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.TangentSlice, shape.Tangent, 3);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.ColorSlice, shape.Color, 4);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.Texcoord0Slice, shape.TexCoord0, 2);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.Texcoord1Slice, shape.TexCoord1, 2);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.Texcoord2Slice, shape.TexCoord2, 2);

                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.BoneIndicesSlice, shape.BoneIndices, shape.boneDimension);
                        WriteVertexBufferData(writer, vatrPos, VertexAttributesChunk.BoneWeightsSlice, shape.BoneWeights, shape.boneDimension);
                    }
                    writer.WriteSectionSizeU32(vatrPos + 4, vatrPos, writer.Position);
                }

                if (SkeletalMeshChunk != null && SkeletalMeshChunk.ShapeChunk.SeperateShapes.Count > 0)
                {
                    writer.WriteUint32Offset(pos + (_offsetPos += 4));

                    long indexBufferPos = writer.Position;
                    foreach (var shape in SkeletalMeshChunk.ShapeChunk.SeperateShapes)
                    {
                        foreach (var prim in shape.Primatives)
                        {
                            foreach (var subprim in prim.Primatives) //Note 3DS usually only has one sub primative
                            {
                                writer.SeekBegin(indexBufferPos + subprim.Offset);

                                switch (subprim.IndexType)
                                {
                                    case CmbDataType.UByte:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            writer.Write((byte)subprim.Indices[i]);
                                        break;
                                    case CmbDataType.UShort:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            writer.Write((ushort)subprim.Indices[i]);
                                        break;
                                    case CmbDataType.UInt:
                                        for (int i = 0; i < subprim.IndexCount; i++)
                                            writer.Write((uint)subprim.Indices[i]);
                                        break;
                                    default:
                                        throw new Exception("Unsupported index type! " + subprim.IndexType);
                                }
                            }
                        }
                    }
                    writer.Align(64);
                }

                if (TextureChunk != null && TextureChunk.Textures.Count > 0)
                {
                    long dataStart = writer.Position;
                    writer.WriteUint32Offset(pos + (_offsetPos += 4));
                    //Save image data
                    foreach (var tex in TextureChunk.Textures)
                        writer.Write(tex.ImageData);
                }
            }
        }

        //Connects all the meshes, vertex attributes, and shape data together
        public class SkeletalMeshChunk : IChunkCommon
        {
            private const string Magic = "sklm";

            public MeshesChunk MeshChunk { get; set; }
            public ShapesChunk ShapeChunk { get; set; }

            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                MeshChunk = ReadChunkSection<MeshesChunk>(reader, header, pos);
                ShapeChunk = ReadChunkSection<ShapesChunk>(reader, header, pos);
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                long _offsetPos = writer.Position;
                writer.Write(uint.MaxValue);//MeshChunk
                writer.Write(uint.MaxValue);//ShapeChunk

                if (MeshChunk != null)
                {
                    writer.WriteUint32Offset(_offsetPos, pos);
                    MeshChunk.Write(writer, header);
                }

                if (ShapeChunk != null)
                {
                    writer.WriteUint32Offset(_offsetPos + 4, pos);
                    ShapeChunk.Write(writer, header);
                }


                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class MeshesChunk : IChunkCommon
        {
            private const string Magic = "mshs";

            public List<Mesh> Meshes = new List<Mesh>();

            public uint Unknown;

            public void Read(FileReader reader, Header header)
            {
                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint meshCount = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();

                long meshPos = reader.Position;
                for (int i = 0; i < meshCount; i++)
                {
                    Mesh mesh = new Mesh();

                    mesh.SepdIndex = reader.ReadUInt16();
                    mesh.MaterialIndex = reader.ReadByte();
                    mesh.ID = reader.ReadByte();
                    Meshes.Add(mesh);

                    if (header.Version == CMBVersion.MM3DS)
                        mesh.unks = reader.ReadBytes(8);
                    else if (header.Version >= CMBVersion.LM3DS)
                        mesh.unks = reader.ReadBytes(84);

                    Console.WriteLine($"SepdIndex {mesh.SepdIndex}");
                    Console.WriteLine($"MaterialIndex { mesh.MaterialIndex}");
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Meshes.Count);
                writer.Write(Unknown);

                for (int i = 0; i < Meshes.Count; i++)
                {
                    writer.Write(Meshes[i].SepdIndex);
                    writer.Write(Meshes[i].MaterialIndex);
                    writer.Write(Meshes[i].ID);
                    if (Meshes[i].unks != null)
                        writer.Write(Meshes[i].unks);
                }

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }

            public class Mesh
            {
                public byte[] unks;

                public ushort SepdIndex { get; set; }
                public byte MaterialIndex { get; set; }
                public byte ID { get; set; }
            }
        }

        public class ShapesChunk : IChunkCommon
        {
            private const string Magic = "shp ";

            public uint Unknown;

            public List<SeperateShape> SeperateShapes = new List<SeperateShape>();

            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint sepdCount = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();
                ushort[] offsets = reader.ReadUInt16s((int)sepdCount);
                for (int i = 0; i < sepdCount; i++)
                {
                    reader.SeekBegin(pos + offsets[i]);
                    var sepd = new SeperateShape();
                    sepd.Read(reader, header);
                    SeperateShapes.Add(sepd);
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(SeperateShapes.Count);
                writer.Write(Unknown);
                long offsetPos = writer.Position;
                writer.Write(new ushort[SeperateShapes.Count]);
                for (int i = 0; i < SeperateShapes.Count; i++)
                {
                    writer.WriteUint16Offset(offsetPos + (i * 2), pos);
                    SeperateShapes[i].Write(writer, header);
                }

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class SeperateShape : IChunkCommon
        {
            private const string Magic = "sepd";

            public SepdVertexAttribute Position { get; set; }
            public SepdVertexAttribute Normal { get; set; }
            public SepdVertexAttribute Tangent { get; set; }
            public SepdVertexAttribute Color { get; set; }
            public SepdVertexAttribute TexCoord0 { get; set; }
            public SepdVertexAttribute TexCoord1 { get; set; }
            public SepdVertexAttribute TexCoord2 { get; set; }
            public SepdVertexAttribute BoneIndices { get; set; }
            public SepdVertexAttribute BoneWeights { get; set; }

            public List<PrimativesChunk> Primatives = new List<PrimativesChunk>();

            public ushort boneDimension;
            public ushort Unknown;

            private byte[] unks;

            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint count = reader.ReadUInt16();

                if (header.Version >= CMBVersion.LM3DS)
                    unks = reader.ReadBytes(50);
                else
                    unks = reader.ReadBytes(26);

                Position = ReadVertexAttrib(reader);
                Normal = ReadVertexAttrib(reader);
                if (header.Version >= CMBVersion.MM3DS)
                    Tangent = ReadVertexAttrib(reader);

                Color = ReadVertexAttrib(reader);
                TexCoord0 = ReadVertexAttrib(reader);
                TexCoord1 = ReadVertexAttrib(reader);
                TexCoord2 = ReadVertexAttrib(reader);
                BoneIndices = ReadVertexAttrib(reader);
                BoneWeights = ReadVertexAttrib(reader);

                boneDimension = reader.ReadUInt16();
                Unknown = reader.ReadUInt16();

                ushort[] Offsets = reader.ReadUInt16s((int)count);

                for (int i = 0; i < count; i++)
                {
                    reader.SeekBegin(pos + Offsets[i]);
                    PrimativesChunk prim = new PrimativesChunk();
                    prim.Read(reader, header);
                    Primatives.Add(prim);
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue); //section size
                writer.Write((ushort)Primatives.Count);
                writer.Write(unks);
                WriteVertexAttrib(writer, Position);
                WriteVertexAttrib(writer, Normal);
                if (header.Version >= CMBVersion.MM3DS)
                    WriteVertexAttrib(writer, Tangent);

                WriteVertexAttrib(writer, Color);
                WriteVertexAttrib(writer, TexCoord0);
                WriteVertexAttrib(writer, TexCoord1);
                WriteVertexAttrib(writer, TexCoord2);
                WriteVertexAttrib(writer, BoneIndices);
                WriteVertexAttrib(writer, BoneWeights);
                writer.Write((ushort)boneDimension);
                writer.Write((ushort)Unknown);

                long offsetPos = writer.Position;
                writer.Write(new ushort[Primatives.Count]);
                writer.Write((ushort)0);
                for (int i = 0; i < Primatives.Count; i++)
                {
                    writer.WriteUint16Offset(offsetPos + (i * 2), pos);
                    Primatives[i].Write(writer, header);
                }

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }

            private SepdVertexAttribute ReadVertexAttrib(FileReader reader)
            {
                long pos = reader.Position;

                SepdVertexAttribute att = new SepdVertexAttribute();
                att.StartPosition = reader.ReadUInt32();
                att.Scale = reader.ReadSingle();
                att.Type = reader.ReadEnum<CmbDataType>(true);
                att.Mode = reader.ReadEnum<SepdVertexAttribMode>(true);
                att.Constants = new float[4];
                att.Constants[0] = reader.ReadSingle();
                att.Constants[1] = reader.ReadSingle();
                att.Constants[2] = reader.ReadSingle();
                att.Constants[3] = reader.ReadSingle();

                reader.SeekBegin(pos + 0x1C);

                return att;
            }

            private void WriteVertexAttrib(FileWriter writer, SepdVertexAttribute att)
            {
                long pos = writer.Position;

                writer.Write(att.StartPosition);
                writer.Write(att.Scale);
                writer.Write(att.Type, true);
                writer.Write(att.Mode, true);
                writer.Write(att.Constants[0]);
                writer.Write(att.Constants[1]);
                writer.Write(att.Constants[2]);
                writer.Write(att.Constants[3]);

                writer.SeekBegin(pos + 0x1C);
            }
        }

        public class SepdVertexAttribute
        {
            public uint StartPosition { get; set; }
            public float Scale { get; set; }
            public CmbDataType Type { get; set; }
            public SepdVertexAttribMode Mode { get; set; }

            public Syroot.Maths.Vector4F[] VertexData { get; set; }

            public float[] Constants { get; set; }
        }

        public class PrimativesChunk : IChunkCommon
        {
            private const string Magic = "prms";

            public SkinningMode SkinningMode;

            public List<SubPrimativeChunk> Primatives = new List<SubPrimativeChunk>();

            public ushort[] BoneIndexTable { get; set; }

            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint count = reader.ReadUInt32();
                SkinningMode = reader.ReadEnum<SkinningMode>(true);
                ushort boneTableCount = reader.ReadUInt16();
                uint boneIndexOffset = reader.ReadUInt32();
                uint primativeOffset = reader.ReadUInt32();

                reader.SeekBegin(pos + boneIndexOffset);
                BoneIndexTable = reader.ReadUInt16s(boneTableCount);

                reader.SeekBegin(pos + primativeOffset);
                for (int i = 0; i < count; i++)
                {
                    SubPrimativeChunk prim = new SubPrimativeChunk();
                    prim.Read(reader, header);
                    Primatives.Add(prim);
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Primatives.Count);
                writer.Write(SkinningMode, true);
                writer.Write((ushort)BoneIndexTable.Length);

                long boneIndexOfsPos = writer.Position;
                writer.Write(uint.MaxValue); //bone index offset

                long primativeOfsPos = writer.Position;
                writer.Write(uint.MaxValue); //primative offset

                writer.WriteUint32Offset(boneIndexOfsPos, pos);
                writer.Write(BoneIndexTable);
                writer.Align(4);

                writer.WriteUint32Offset(primativeOfsPos, pos);
                for (int i = 0; i < Primatives.Count; i++)
                    Primatives[i].Write(writer, header);

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class SubPrimativeChunk : IChunkCommon
        {
            private const string Magic = "prm ";

            public SkinningMode SkinningMode { get; private set; }

            public CmbDataType IndexType { get; private set; }

            public ushort IndexCount { get; private set; }

            public uint Offset { get; private set; }

            private uint[] _indices;
            public uint[] Indices
            {
                get
                {
                    return _indices;
                }
                set
                {
                    _indices = value;
                }
            }

            public uint Unknown;
            public uint Unknown2;

            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt32();
                IndexType = reader.ReadEnum<CmbDataType>(true);
                reader.Seek(2); //padding

                IndexCount = reader.ReadUInt16();

                //This value is the index, so we'll use it as an offset
                //Despite the data type, this is always * 2
                Offset = (uint)reader.ReadUInt16() * sizeof(ushort);
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Unknown);
                writer.Write(Unknown2);
                writer.Write(IndexType, true);
                writer.Seek(2);
                writer.Write(IndexCount);
                writer.Write((ushort)(Offset / sizeof(ushort)));

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class LUTSChunk : IChunkCommon
        {
            private const string Magic = "luts";

            private byte[] data;

            public void Read(FileReader reader, Header header)
            {
                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();

                data = reader.getSection((uint)reader.Position, sectionSize - 8);
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(data);

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class VertexAttributesChunk : IChunkCommon
        {
            private const string Magic = "vatr";

            public BufferSlice PositionSlice;
            public BufferSlice NormalSlice;
            public BufferSlice TangentSlice; //Used in MM3DS and newer
            public BufferSlice ColorSlice;
            public BufferSlice Texcoord0Slice;
            public BufferSlice Texcoord1Slice;
            public BufferSlice Texcoord2Slice;
            public BufferSlice BoneIndicesSlice;
            public BufferSlice BoneWeightsSlice;

            public long StartPosition;

            public uint MaxIndex;

            public void Read(FileReader reader, Header header)
            {
                StartPosition = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                MaxIndex = reader.ReadUInt32();

                PositionSlice = ReadSlice(reader);
                NormalSlice = ReadSlice(reader);
                if (header.Version >= CMBVersion.MM3DS)
                    TangentSlice = ReadSlice(reader);

                ColorSlice = ReadSlice(reader);
                Texcoord0Slice = ReadSlice(reader);
                Texcoord1Slice = ReadSlice(reader);
                Texcoord2Slice = ReadSlice(reader);
                BoneIndicesSlice = ReadSlice(reader);
                BoneWeightsSlice = ReadSlice(reader);
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(MaxIndex);
                WriteSlice(writer, PositionSlice);
                WriteSlice(writer, NormalSlice);
                if (header.Version >= CMBVersion.MM3DS)
                    WriteSlice(writer, TangentSlice);

                WriteSlice(writer, ColorSlice);
                WriteSlice(writer, Texcoord0Slice);
                WriteSlice(writer, Texcoord1Slice);
                WriteSlice(writer, Texcoord2Slice);
                WriteSlice(writer, BoneIndicesSlice);
                WriteSlice(writer, BoneWeightsSlice);

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }

            private void WriteSlice(FileWriter writer, BufferSlice slice)
            {
                writer.Write(slice.Size);
                writer.Write(slice.Offset);
            }

            private BufferSlice ReadSlice(FileReader reader)
            {
                BufferSlice slice = new BufferSlice();
                slice.Size = reader.ReadUInt32();
                slice.Offset = reader.ReadUInt32();
                return slice;
            }
        }

        public class BufferSlice
        {
            public uint Offset;
            public uint Size;
        }

        public class SkeletonChunk : IChunkCommon
        {
            private const string Magic = "skl ";

            public List<BoneChunk> Bones = new List<BoneChunk>();

            public uint Unknown;

            public void Read(FileReader reader, Header header)
            {
                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint boneCount = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();

                for (int i = 0; i < boneCount; i++)
                {
                    BoneChunk bone = new BoneChunk();
                    bone.ID = reader.ReadInt16() & 0xFFFF;
                    bone.ParentIndex = reader.ReadInt16();
                    bone.Scale = reader.ReadVec3SY();
                    bone.Rotation = reader.ReadVec3SY();
                    bone.Translation = reader.ReadVec3SY();
                    if (header.Version >= CMBVersion.MM3DS)
                        bone.Unknown = reader.ReadInt32();

                    Bones.Add(bone);
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Bones.Count);
                writer.Write(Unknown);
                for (int i = 0; i < Bones.Count; i++)
                {
                    writer.Write((ushort)Bones[i].ID);
                    writer.Write((short)Bones[i].ParentIndex);
                    writer.Write(Bones[i].Scale);
                    writer.Write(Bones[i].Rotation);
                    writer.Write(Bones[i].Translation);
                    if (header.Version >= CMBVersion.MM3DS)
                        writer.Write(Bones[i].Unknown);
                }

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class BoneChunk
        {
            public int ID { get; set; }
            public int ParentIndex { get; set; }

            public Syroot.Maths.Vector3F Scale { get; set; }
            public Syroot.Maths.Vector3F Rotation { get; set; }
            public Syroot.Maths.Vector3F Translation { get; set; }

            //An unknown value used in versions MM3DS and newer
            public int Unknown { get; set; }
        }

        public class QuadTreeChunk : IChunkCommon
        {
            private const string Magic = "qtrs";

            byte[] data;

            public void Read(FileReader reader, Header header)
            {
                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();

                data = reader.getSection((uint)reader.Position, sectionSize);
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(data);

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        public class MaterialChunk : IChunkCommon
        {
            private const string Magic = "mats";

            public List<Material> Materials = new List<Material>();

            internal int textureCombinerSettingsTableOffs;
            public void Read(FileReader reader, Header header)
            {
                long pos = reader.Position;

                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint count = reader.ReadUInt32();

                int materialSize = 0x15C;
                if (header.Version >= CMBVersion.MM3DS)
                    materialSize = 0x16C;

                Console.WriteLine($"materialSize {materialSize.ToString("x")}");

                textureCombinerSettingsTableOffs = (int)(pos + 12 + (count * materialSize));

                for (int i = 0; i < count; i++)
                {
                    reader.SeekBegin(pos + 0xC + (i * materialSize));

                    Material mat = new Material();
                    mat.Read(reader, header, this);
                    Materials.Add(mat);
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Materials.Count);

                for (int i = 0; i < Materials.Count; i++)
                    Materials[i].Write(writer, header);
                for (int i = 0; i < Materials.Count; i++)
                {
                    foreach (var combiner in Materials[i].TextureCombiners)
                    {
                        writer.Write(combiner.combineRGB, false);
                        writer.Write(combiner.combineAlpha, false);
                        writer.Write(combiner.scaleRGB, false);
                        writer.Write(combiner.scaleAlpha, false);
                        writer.Write(combiner.bufferInputRGB, false);
                        writer.Write(combiner.bufferInputAlpha, false);
                        writer.Write(combiner.source0RGB, false);
                        writer.Write(combiner.source1RGB, false);
                        writer.Write(combiner.source2RGB, false);
                        writer.Write(combiner.op0RGB, false);
                        writer.Write(combiner.op1RGB, false);
                        writer.Write(combiner.op2RGB, false);
                        writer.Write(combiner.source0Alpha, false);
                        writer.Write(combiner.source1Alpha, false);
                        writer.Write(combiner.source2Alpha, false);
                        writer.Write(combiner.op0Alpha, false);
                        writer.Write(combiner.op1Alpha, false);
                        writer.Write(combiner.op2Alpha, false);
                        writer.Write(combiner.constantIndex);
                    }
                }

                long endPos = writer.Position;
                using (writer.TemporarySeek(pos + 4, System.IO.SeekOrigin.Begin))
                {
                    writer.Write((uint)(endPos - pos));
                }
            }
        }

        //Thanks for noclip for material RE stuff
        //https://github.com/magcius/noclip.website/blob/9270b9e5022c691703689990f9c536cd9058e5cd/src/oot3d/cmb.ts#L232
        public class Material
        {
            private Header CMBHeader;

            public bool IsTransparent = false;

            public bool IsFragmentLightingEnabled;
            public bool IsVertexLightingEnabled;
            public bool IsHemiSphereLightingEnabled;
            public bool IsHemiSphereOcclusionEnabled;

            public CullMode CullMode { get; set; }

            public bool IsPolygonOffsetEnabled { get; set; }
            public ushort PolygonOffset { get; set; }

            public TextureMap[] TextureMaps { get; set; }
            public TextureMatrix[] TextureMaticies { get; set; }

            public uint TotalUsedTextures { get; set; }
            public uint TotalUsedTextureCoords { get; set; }

            public List<TextureCombiner> TextureCombiners { get; set; }

            public STColor8 EmissionColor { get; set; }

            public STColor8 AmbientColor { get; set; }

            public STColor8 Diffuse { get; set; }

            public STColor8 Specular0 { get; set; }

            public STColor8 Specular1 { get; set; }

            public STColor8[] ConstantColors { get; set; }

            public bool AlphaTestEnable { get; set; }
            public float AlphaTestReference { get; set; }

            public bool DepthTestEnable { get; set; }

            public bool DepthWriteEnable { get; set; }

            public AlphaFunction AlphaTestFunction { get; set; }

            public DepthFunction DepthTestFunction { get; set; }

            public bool BlendEnaled { get; set; }

            public BlendingFactor BlendingFactorSrcRGB { get; set; }
            public BlendingFactor BlendingFactorDestRGB { get; set; }
            public BlendEquationMode BlendingEquationRGB { get; set; }

            public BlendingFactor BlendingFactorSrcAlpha { get; set; }
            public BlendingFactor BlendingFactorDestAlpha { get; set; }
            public BlendEquationMode BlendingEquationAlpha { get; set; }


            public float BlendColorR { get; set; }
            public float BlendColorG { get; set; }
            public float BlendColorB { get; set; }
            public float BlendColorA { get; set; }

            public float BufferColorR { get; set; }
            public float BufferColorG { get; set; }
            public float BufferColorB { get; set; }
            public float BufferColorA { get; set; }

            public TextureUnit BumpMapIndex { get; set; }
            public BumpMode BumpMapMode { get; set; }
            public short IsBumpRenormalize { get; set; }

            public LayerConfig LayerConfig { get; set; }

            public FresnelSelector FresnelSelector { get; set; }

            public bool IsClampHighLight { get; set; }
            public bool IsDistribution0Enabled { get; set; }
            public bool IsDistribution1Enabled { get; set; }
            public bool IsGeometricFactor0Enabled { get; set; }
            public bool IsGeometricFactor1Enabled { get; set; }
            public bool IsReflectionEnabled { get; set; }

            private byte[] data;

            private string GetTextureName(int index)
            {
                if (index != -1 && index < CMBHeader.SectionData.TextureChunk?.Textures?.Count)
                    return CMBHeader.SectionData.TextureChunk.Textures[index].Name;
                else
                    return "";
            }

            public LightTable LUTTable;

            public struct LightTable
            {
                public bool reflectanceRSamplerIsAbs;
                public sbyte reflectanceRSamplerIndex;
                public LUTInput reflectanceRSamplerInput;
                public float reflectanceRSamplerScale;

                public bool reflectanceGSamplerIsAbs;
                public sbyte reflectanceGSamplerIndex;
                public LUTInput reflectanceGSamplerInput;
                public float reflectanceGSamplerScale;

                public bool reflectanceBSamplerIsAbs;
                public sbyte reflectanceBSamplerIndex;
                public LUTInput reflectanceBSamplerInput;
                public float reflectanceBSamplerScale;

                public bool reflectance0SamplerIsAbs;
                public sbyte reflectance0SamplerIndex;
                public LUTInput reflectance0SamplerInput;
                public float reflectance0SamplerScale;

                public bool reflectance1SamplerIsAbs;
                public sbyte reflectance1SamplerIndex;
                public LUTInput reflectance1SamplerInput;
                public float reflectance1SamplerScale;

                public bool fresnelSamplerIsAbs;
                public sbyte fresnelSamplerIndex;
                public LUTInput fresnelSamplerInput;
                public float fresnelSamplerScale;
            }

            public void Read(FileReader reader, Header header, MaterialChunk materialChunkParent)
            {
                CMBHeader = header;

                TextureMaps = new TextureMap[3];
                TextureMaticies = new TextureMatrix[3];
                TextureCombiners = new List<TextureCombiner>();

                long pos = reader.Position;

                IsFragmentLightingEnabled = reader.ReadBoolean();
                IsVertexLightingEnabled = reader.ReadBoolean();
                IsHemiSphereLightingEnabled = reader.ReadBoolean();

                //Tip: IsHemiSphereOcclusionEnabled cannot be enabled unless "IsHemiSphereOcclusionEnabled" is enabled first
                IsHemiSphereOcclusionEnabled = reader.ReadBoolean();

                CullMode = reader.ReadEnum<CullMode>(true); //byte
                IsPolygonOffsetEnabled = reader.ReadBoolean(); //byte
                PolygonOffset = reader.ReadUInt16();
                PolygonOffset = IsPolygonOffsetEnabled ? (ushort)((int)PolygonOffset / 0x10000) : (ushort)0;
                TotalUsedTextures = reader.ReadUInt32();
                TotalUsedTextureCoords = reader.ReadUInt32();

                //Texture bind data
                for (int j = 0; j < 3; j++)
                {
                    TextureMaps[j] = new TextureMap();
                    TextureMaps[j].TextureIndex = reader.ReadInt16();
                    reader.ReadInt16(); //padding
                    TextureMaps[j].MinFiler = (TextureFilter)reader.ReadUInt16();
                    TextureMaps[j].MagFiler = (TextureFilter)reader.ReadUInt16();
                    TextureMaps[j].WrapS = (CMBTextureWrapMode)reader.ReadUInt16();
                    TextureMaps[j].WrapT = (CMBTextureWrapMode)reader.ReadUInt16();
                    TextureMaps[j].MinLOD = reader.ReadSingle();
                    TextureMaps[j].LodBias = reader.ReadSingle();
                    TextureMaps[j].borderColorR = reader.ReadByte();
                    TextureMaps[j].borderColorG = reader.ReadByte();
                    TextureMaps[j].borderColorB = reader.ReadByte();
                    TextureMaps[j].borderColorA = reader.ReadByte();
                }

                for (int j = 0; j < 3; j++)
                {
                    TextureMaticies[j] = new TextureMatrix();
                    TextureMaticies[j].MatrixMode = reader.ReadByte();
                    TextureMaticies[j].ReferenceCamera = reader.ReadByte();
                    TextureMaticies[j].MappingMethod = reader.ReadByte();
                    TextureMaticies[j].CoordinateIndex = reader.ReadByte();
                    TextureMaticies[j].Scale = reader.ReadVec2SY();
                    TextureMaticies[j].Rotate = reader.ReadSingle();
                    TextureMaticies[j].Translate = reader.ReadVec2SY();
                }

                EmissionColor = STColor8.FromBytes(reader.ReadBytes(4));
                AmbientColor = STColor8.FromBytes(reader.ReadBytes(4));
                Diffuse = STColor8.FromBytes(reader.ReadBytes(4));
                Specular0 = STColor8.FromBytes(reader.ReadBytes(4));
                Specular1 = STColor8.FromBytes(reader.ReadBytes(4));

                ConstantColors = new STColor8[6];
                ConstantColors[0] = STColor8.FromBytes(reader.ReadBytes(4));
                ConstantColors[1] = STColor8.FromBytes(reader.ReadBytes(4));
                ConstantColors[2] = STColor8.FromBytes(reader.ReadBytes(4));
                ConstantColors[3] = STColor8.FromBytes(reader.ReadBytes(4));
                ConstantColors[4] = STColor8.FromBytes(reader.ReadBytes(4));
                ConstantColors[5] = STColor8.FromBytes(reader.ReadBytes(4));

                BufferColorR = reader.ReadSingle();
                BufferColorG = reader.ReadSingle();
                BufferColorB = reader.ReadSingle();
                BufferColorA = reader.ReadSingle();

                BumpMapIndex = (TextureUnit)reader.ReadUInt16();
                BumpMapMode = (BumpMode)reader.ReadUInt16();
                IsBumpRenormalize = reader.ReadInt16();
                reader.ReadInt16(); //padding
                LayerConfig = (LayerConfig)reader.ReadUInt16();
                reader.ReadInt16(); //padding
                FresnelSelector = (FresnelSelector)reader.ReadUInt16();
                IsClampHighLight = reader.ReadBoolean();
                IsDistribution0Enabled = reader.ReadBoolean();
                IsDistribution1Enabled = reader.ReadBoolean();
                IsGeometricFactor0Enabled = reader.ReadBoolean();
                IsGeometricFactor1Enabled = reader.ReadBoolean();
                IsReflectionEnabled = reader.ReadBoolean();

                // Fragment lighting table.
                LUTTable.reflectanceRSamplerIsAbs = reader.ReadBoolean();
                LUTTable.reflectanceRSamplerIndex = reader.ReadSByte();
                LUTTable.reflectanceRSamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.reflectanceRSamplerScale = reader.ReadSingle();

                LUTTable.reflectanceGSamplerIsAbs = reader.ReadBoolean();
                LUTTable.reflectanceGSamplerIndex = reader.ReadSByte();
                LUTTable.reflectanceGSamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.reflectanceGSamplerScale = reader.ReadSingle();

                LUTTable.reflectanceBSamplerIsAbs = reader.ReadBoolean();
                LUTTable.reflectanceBSamplerIndex = reader.ReadSByte();
                LUTTable.reflectanceBSamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.reflectanceBSamplerScale = reader.ReadSingle();

                LUTTable.reflectance0SamplerIsAbs = reader.ReadBoolean();
                LUTTable.reflectance0SamplerIndex = reader.ReadSByte();
                LUTTable.reflectance0SamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.reflectance0SamplerScale = reader.ReadSingle();

                LUTTable.reflectance1SamplerIsAbs = reader.ReadBoolean();
                LUTTable.reflectance1SamplerIndex = reader.ReadSByte();
                LUTTable.reflectance1SamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.reflectance1SamplerScale = reader.ReadSingle();

                LUTTable.fresnelSamplerIsAbs = reader.ReadBoolean();
                LUTTable.fresnelSamplerIndex = reader.ReadSByte();
                LUTTable.fresnelSamplerInput = (LUTInput)reader.ReadUInt16();
                LUTTable.fresnelSamplerScale = reader.ReadSingle();

                reader.SeekBegin(pos + 0x120);
                uint textureCombinerTableCount = reader.ReadUInt32();
                var skip = reader.Position;
                int textureCombinerTableIdx = (int)pos + 0x124;
                for (int i = 0; i < textureCombinerTableCount; i++)
                {
                    reader.SeekBegin(textureCombinerTableIdx + 0x00);
                    ushort textureCombinerIndex = reader.ReadUInt16();

                    reader.SeekBegin(materialChunkParent.textureCombinerSettingsTableOffs + textureCombinerIndex * 0x28);
                    TextureCombiner combner = new TextureCombiner();
                    combner.combineRGB = reader.ReadEnum<CombineResultOpDMP>(false);
                    combner.combineAlpha = reader.ReadEnum<CombineResultOpDMP>(false);
                    combner.scaleRGB = reader.ReadEnum<CombineScaleDMP>(false);
                    combner.scaleAlpha = reader.ReadEnum<CombineScaleDMP>(false);
                    combner.bufferInputRGB = reader.ReadEnum<CombineBufferInputDMP>(false);
                    combner.bufferInputAlpha = reader.ReadEnum<CombineBufferInputDMP>(false);
                    combner.source0RGB = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.source1RGB = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.source2RGB = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.op0RGB = reader.ReadEnum<CombineOpDMP>(false);
                    combner.op1RGB = reader.ReadEnum<CombineOpDMP>(false);
                    combner.op2RGB = reader.ReadEnum<CombineOpDMP>(false);
                    combner.source0Alpha = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.source1Alpha = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.source2Alpha = reader.ReadEnum<CombineSourceDMP>(false);
                    combner.op0Alpha = reader.ReadEnum<CombineOpDMP>(false);
                    combner.op1Alpha = reader.ReadEnum<CombineOpDMP>(false);
                    combner.op2Alpha = reader.ReadEnum<CombineOpDMP>(false);
                    combner.constantIndex = reader.ReadUInt32();
                    TextureCombiners.Add(combner);

                    textureCombinerTableIdx += 0x2;
                }

                //Skip TexEnvStages indices. (always 0x6)
                reader.SeekBegin(skip + 0x0C);

                AlphaTestEnable = reader.ReadBoolean();
                AlphaTestReference = reader.ReadByte() / 0xFF;
                AlphaTestFunction = (AlphaFunction)reader.ReadUInt16();

                DepthTestEnable = reader.ReadBoolean();
                DepthWriteEnable = reader.ReadBoolean();
                DepthTestFunction = (DepthFunction)reader.ReadUInt16();

                if (!AlphaTestEnable)
                    AlphaTestFunction = AlphaFunction.Always;

                if (!DepthTestEnable)
                    DepthTestFunction = DepthFunction.Always;

                BlendEnaled = reader.ReadBoolean();

                //Unknown. 
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                BlendingFactorSrcAlpha = (BlendingFactor)reader.ReadUInt16();
                BlendingFactorDestAlpha = (BlendingFactor)reader.ReadUInt16();
                BlendingEquationAlpha = (BlendEquationMode)reader.ReadUInt32();

                BlendingFactorSrcRGB = (BlendingFactor)reader.ReadUInt16();
                BlendingFactorDestRGB = (BlendingFactor)reader.ReadUInt16();
                BlendingEquationRGB = (BlendEquationMode)reader.ReadUInt32();

                BlendColorR = reader.ReadSingle();
                BlendColorG = reader.ReadSingle();
                BlendColorB = reader.ReadSingle();
                BlendColorA = reader.ReadSingle();

                IsTransparent = BlendEnaled;

                if (header.Version > CMBVersion.OOT3DS)
                {
                    byte StencilEnabled = reader.ReadByte();
                    byte StencilReferenceValue = reader.ReadByte();
                    byte BufferMask = reader.ReadByte();
                    byte Buffer = reader.ReadByte();
                    ushort StencilFunc = reader.ReadUInt16();
                    ushort FailOP = reader.ReadUInt16();
                    ushort ZFailOP = reader.ReadUInt16();
                    ushort ZPassOP = reader.ReadUInt16();
                    ushort unk6 = reader.ReadUInt16();
                    ushort unk7 = reader.ReadUInt16();
                }
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.Write(IsFragmentLightingEnabled);
                writer.Write(IsVertexLightingEnabled);
                writer.Write(IsHemiSphereLightingEnabled);
                writer.Write(IsHemiSphereOcclusionEnabled);
                writer.Write(CullMode, true);
                writer.Write(IsPolygonOffsetEnabled);
                writer.Write(PolygonOffset);
                writer.Write(TotalUsedTextures);
                writer.Write(TotalUsedTextureCoords);

                for (int j = 0; j < 3; j++)
                {
                    writer.Write(TextureMaps[j].TextureIndex);
                    writer.Write((ushort)0);
                    writer.Write((ushort)TextureMaps[j].MinFiler);
                    writer.Write((ushort)TextureMaps[j].MagFiler);
                    writer.Write((ushort)TextureMaps[j].WrapS);
                    writer.Write((ushort)TextureMaps[j].WrapT);
                    writer.Write(TextureMaps[j].MinLOD);
                    writer.Write(TextureMaps[j].LodBias);
                    writer.Write(TextureMaps[j].borderColorR);
                    writer.Write(TextureMaps[j].borderColorG);
                    writer.Write(TextureMaps[j].borderColorB);
                    writer.Write(TextureMaps[j].borderColorA);
                }

                for (int j = 0; j < 3; j++)
                {
                    writer.Write(TextureMaticies[j].Scale);
                    writer.Write(TextureMaticies[j].Rotate);
                    writer.Write(TextureMaticies[j].Translate);
                    writer.Write(TextureMaticies[j].MatrixMode);
                    writer.Write(TextureMaticies[j].ReferenceCamera);
                    writer.Write(TextureMaticies[j].MappingMethod);
                    writer.Write(TextureMaticies[j].CoordinateIndex);
                }

                writer.Write(data);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"----------------------------------------------------\n");
                sb.Append($"AlphaTest {AlphaTestEnable} {AlphaTestFunction} {AlphaTestReference}\n");
                sb.Append($"DepthTest {DepthTestEnable} {DepthTestFunction} DepthWrite {DepthTestFunction}\n");

                sb.Append($"BlendingFactorSrcRGB {BlendingFactorSrcRGB}\n");
                sb.Append($"BlendingFactorDestRGB {BlendingFactorDestRGB}\n");
                sb.Append($"BlendingFactorSrcAlpha {BlendingFactorSrcAlpha}\n");
                sb.Append($"BlendingFactorDestAlpha {BlendingFactorDestAlpha}\n");
                sb.Append($"BlendEnaled {BlendEnaled}\n");
                sb.Append($"----------------------------------------------------\n");

                sb.AppendLine();

                return sb.ToString();
            }
        }

        public class TextureCombiner
        {
            public CombineResultOpDMP combineRGB;
            public CombineResultOpDMP combineAlpha;
            public CombineScaleDMP scaleRGB;
            public CombineScaleDMP scaleAlpha;
            public CombineBufferInputDMP bufferInputRGB;
            public CombineBufferInputDMP bufferInputAlpha;
            public CombineSourceDMP source0RGB;
            public CombineSourceDMP source1RGB;
            public CombineSourceDMP source2RGB;
            public CombineOpDMP op0RGB;
            public CombineOpDMP op1RGB;
            public CombineOpDMP op2RGB;
            public CombineSourceDMP source0Alpha;
            public CombineSourceDMP source1Alpha;
            public CombineSourceDMP source2Alpha;

            public CombineOpDMP op0Alpha;
            public CombineOpDMP op1Alpha;
            public CombineOpDMP op2Alpha;
            public uint constantIndex;
        }

        public class TextureMatrix
        {
            public Syroot.Maths.Vector2F Scale { get; set; }
            public Syroot.Maths.Vector2F Translate { get; set; }
            public float Rotate { get; set; }

            public byte MatrixMode { get; set; }
            public byte ReferenceCamera { get; set; }
            public byte MappingMethod { get; set; }
            public byte CoordinateIndex { get; set; }
        }

        public class TextureMap
        {
            public short TextureIndex { get; set; }
            public TextureFilter MinFiler { get; set; }
            public TextureFilter MagFiler { get; set; }
            public CMBTextureWrapMode WrapS { get; set; }
            public CMBTextureWrapMode WrapT { get; set; }
            public float MinLOD { get; set; }
            public float LodBias { get; set; }
            public byte borderColorR { get; set; }
            public byte borderColorG { get; set; }
            public byte borderColorB { get; set; }
            public byte borderColorA { get; set; }
        }

        public class TextureChunk : IChunkCommon
        {
            private const string Magic = "tex ";

            public List<CTXB_Parser.Texture> Textures = new List<CTXB_Parser.Texture>();

            public void Read(FileReader reader, Header header)
            {
                reader.ReadSignature(4, Magic);
                uint sectionSize = reader.ReadUInt32();
                uint TextureCount = reader.ReadUInt32();
                for (int i = 0; i < TextureCount; i++)
                    Textures.Add(new CTXB_Parser.Texture(reader));
            }

            public void Write(FileWriter writer, Header header)
            {
                long pos = writer.Position;

                writer.WriteSignature(Magic);
                writer.Write(uint.MaxValue);//SectionSize
                writer.Write(Textures.Count);

                for (int i = 0; i < Textures.Count; i++)
                    Textures[i].Write(writer);

                //Write the total file size
                writer.WriteSectionSizeU32(pos + 4, pos, writer.Position);
            }
        }

        public static T ReadChunkSection<T>(FileReader reader, Header header, long startPos = 0)
             where T : IChunkCommon, new()
        {
            long pos = reader.Position;

            //Read offset and seek it
            uint offset = reader.ReadUInt32();
            reader.SeekBegin(startPos + offset);

            //Create chunk instance
            T chunk = new T();
            chunk.Read(reader, header);

            //Seek back and shift 4 from reading offset
            reader.SeekBegin(pos + 0x4);
            return chunk;
        }

        public interface IChunkCommon
        {
            void Read(FileReader reader, Header header);
            void Write(FileWriter writer, Header header);
        }
    }
}
