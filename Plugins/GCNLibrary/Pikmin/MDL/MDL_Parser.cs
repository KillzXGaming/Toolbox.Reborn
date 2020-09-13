using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Toolbox.Core.IO;
using Toolbox.Core;
using OpenTK;
using Toolbox.Core.GX;

namespace GCNLibrary.Pikmin1.Model
{
    public class MOD_Parser
    {
        public FileHeader Header { get; set; }
        public List<Shape> Shapes { get; set; }
        public Joint[] Joints { get; set; }
        public TXE[] Textures { get; set; }
        public TextureAttribute[] TextureAttributes { get; set; }
        public string[] JointNames { get; set; } = new string[0];
        public ushort[] RigidSkinningIndices { get; set; }
        public ushort[] SmoothSkinningIndices { get; set; }
        public MaterialList MaterialData { get; set; }

        public Envelope[] Envelopes { get; set; }

        public MOD_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                Read(reader);
            }
        }

        BufferData buffer = new BufferData();

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            while (!reader.EndOfStream)
            {
                long chunkStart = reader.Position;
                int opcode = reader.ReadInt32();
                uint sectionSize = reader.ReadUInt32();

                if ((chunkStart & 0x1F) != 0)
                    throw new Exception($"Chunk start ({chunkStart}) not on boundary!");

                ParseSection(reader, (ChunkOperation)opcode, sectionSize);
                reader.SeekBegin(chunkStart + sectionSize + 8);

                if (opcode == 0xFFFF)
                    break;
            }
        }

        private void ParseSection(FileReader reader, ChunkOperation opCode, uint sectionSize)
        {
            Console.WriteLine($"OPCODE {opCode}");
            switch (opCode)
            {
                case ChunkOperation.Header:
                    reader.ReadBytes(0x18);
                    Header = reader.ReadStruct<FileHeader>();
                    break;
                case ChunkOperation.VertexPosition: buffer.PositionsOffset = GetVertexOffset(reader); break;
                case ChunkOperation.VertexNormal: buffer.NormalsOffset = GetVertexOffset(reader); break;
                case ChunkOperation.VertexNBT: buffer.NBTOffset = GetVertexOffset(reader); break;
                case ChunkOperation.VertexUV0:
                case ChunkOperation.VertexUV1:
                case ChunkOperation.VertexUV2:
                case ChunkOperation.VertexUV3:
                case ChunkOperation.VertexUV4:
                case ChunkOperation.VertexUV5:
                case ChunkOperation.VertexUV6:
                case ChunkOperation.VertexUV7:
                    buffer.TexCoordsOffset.Add(GetVertexOffset(reader)); break;
                case ChunkOperation.VertexColor: buffer.ColorsOffset = GetVertexOffset(reader); break;
                case ChunkOperation.Texture:
                    Textures = ReadTextures(reader);
                    break;
                case ChunkOperation.TextureAttribute:
                    TextureAttributes = reader.ParseStructs<TextureAttribute>();
                    break;
                case ChunkOperation.Material:
                    MaterialData = new MaterialList(reader);
                    break;
                case ChunkOperation.SkinningIndices:
                    ushort[] indices = reader.ParsePrimitive<ushort>();
                    ParseSkinningIndices(indices);
                    break;
                case ChunkOperation.Envelope:
                    Envelopes = reader.ParseArray<Envelope>();
                    break;
                case ChunkOperation.Mesh:
                    Shapes = ReadShapes(reader, buffer).ToList();
                    break;
                case ChunkOperation.Joint:
                    Joints = reader.ParseArray<Joint>();
                    break;
                case ChunkOperation.JointName:
                    JointNames = reader.ParsePrimitive<string>();
                    break;
            }
        }

        private uint GetVertexOffset(FileReader reader)
        {
            reader.ReadUInt32(); //count
            reader.AlignPadding(0x20);
            return (uint)reader.Position;
        }

        private void ParseSkinningIndices(ushort[] indices)
        {
            List<ushort> smoothSkinningIndices = new List<ushort>();
            List<ushort> rigidSkinningIndices = new List<ushort>();
            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] == ushort.MaxValue || smoothSkinningIndices.Count > 0)
                    smoothSkinningIndices.Add((ushort)(ushort.MaxValue - indices[i]));
                else
                    rigidSkinningIndices.Add(indices[i]);
            }
            SmoothSkinningIndices = smoothSkinningIndices.ToArray();
            RigidSkinningIndices = rigidSkinningIndices.ToArray();
        }

        private TXE[] ReadTextures(FileReader reader)
        {
            uint count = reader.ReadUInt32(); //count
            reader.Align(32);

            TXE[] textures = new TXE[count];
            for (int i = 0; i < count; i++) {
                textures[i] = new TXE();
                textures[i].Read(reader);
                textures[i].Name = $"Texture{i}";
            }
            return textures;
        }

        private Shape[] ReadShapes(FileReader reader, BufferData buffer)
        {
            uint numMeshes = reader.ReadUInt32();
            reader.AlignPadding(0x20);

            Shape[] meshes = new Shape[numMeshes];

            for (int mIdx = 0; mIdx < numMeshes; mIdx++)
            {
                int boneIndex = reader.ReadInt32();
                int vtxDescriptor = reader.ReadInt32();
                int numPackets = reader.ReadInt32();

                Console.WriteLine($"boneIndex {boneIndex}");

                meshes[mIdx] = new Shape();
                meshes[mIdx].BoneIndex = boneIndex;

                ushort[] matrixIndices = new ushort[10];
                for (int pIdx = 0; pIdx < numPackets; pIdx++)
                    ReadPacket(reader, meshes[mIdx], buffer, matrixIndices, vtxDescriptor);
            }
            return meshes;
        }

        private void ReadPacket(FileReader reader, Shape mesh, BufferData buffer, ushort[] matrixIndices, int vtxDescriptor)
        {
            int numMatrices = reader.ReadInt32();
            ushort[] indices = reader.ReadUInt16s(numMatrices);
            for (int i = 0; i < numMatrices; i++) {
                if (indices[i] == ushort.MaxValue) continue;
                matrixIndices[i] = indices[i];

                Console.WriteLine($"matrixIndices {matrixIndices[i]}");
            }

            var NBT = (vtxDescriptor >> 4);
            bool hasNBT = NBT > 0;

            uint numDisplayLists = reader.ReadUInt32();
            for (int dlIdx = 0; dlIdx < numDisplayLists; dlIdx++)
            {
                byte[] flags = reader.ReadBytes(4);
                int unk1 = reader.ReadInt32();
                int dataSize = reader.ReadInt32();
                reader.AlignPadding(0x20);
                long displayListStart = reader.Position;
                var dlData = new SubStream(reader.BaseStream, displayListStart, dataSize);

                reader.SeekBegin(displayListStart + dataSize);

                List<GXVertexLayout> layouts = new List<GXVertexLayout>();
                if ((vtxDescriptor & 1) == 1)
                    layouts.Add(new GXVertexLayout(GXAttributes.PosNormMatrix, GXComponentType.U8, GXAttributeType.INDEX8, 0));
                if ((vtxDescriptor & 2) == 2)
                    layouts.Add(new GXVertexLayout(GXAttributes.Tex0Matrix, GXComponentType.S8, GXAttributeType.INDEX8, 0));

                layouts.Add(new GXVertexLayout(GXAttributes.Position, GXComponentType.F32, GXAttributeType.INDEX16, buffer.PositionsOffset));

                if (hasNBT && false)
                    layouts.Add(new GXVertexLayout(GXAttributes.NormalBinormalTangent, GXComponentType.F32, GXAttributeType.INDEX16, buffer.NBTOffset));
                else if (buffer.NormalsOffset != 0)
                    layouts.Add(new GXVertexLayout(GXAttributes.Normal, GXComponentType.F32, GXAttributeType.INDEX16, buffer.NormalsOffset));

                if ((vtxDescriptor & 4) == 4)
                    layouts.Add(new GXVertexLayout(GXAttributes.Color0, GXComponentType.RGBA8, GXAttributeType.INDEX16, buffer.ColorsOffset));

                int txCoordDescriptor = vtxDescriptor >> 3;
                for (int tcoordIdx = 0; tcoordIdx < 8; tcoordIdx++)
                {
                    if ((txCoordDescriptor & 1) == 0x1)
                    {
                        // Only read for the first texcoord
                        layouts.Add(new GXVertexLayout((GXAttributes)(1 << 13 + tcoordIdx), GXComponentType.F32,
                            GXAttributeType.INDEX16, buffer.TexCoordsOffset[tcoordIdx]));

                        txCoordDescriptor >>= 1;
                    }
                }

                for (int l = 0; l < layouts.Count; l++)
                    Console.WriteLine($"layouts {layouts[l].Attribute} {layouts[l].AttType}");

                List<STVertex> vertices = DisplayListHelper.ReadDisplayLists(dlData, reader.BaseStream, layouts.ToArray(), null, matrixIndices);
                List<STVertex> dupeVertices = new List<STVertex>();
                for (int v = 0; v < vertices.Count; v++)
                {
                    if (vertices[v].BoneIndices.Count > 0 && !dupeVertices.Contains(vertices[v]))
                    {
                        dupeVertices.Add(vertices[v]);
                        var boneIndex = vertices[v].BoneIndices[0];
                        if (boneIndex >= RigidSkinningIndices.Length)
                        {
                            boneIndex = SmoothSkinningIndices[boneIndex - RigidSkinningIndices.Length];
                            vertices[v].BoneIndices.Clear();
                            vertices[v].BoneWeights.Clear();

                            var envelop = Envelopes[boneIndex];
                            for (int j = 0; j < envelop.Indices.Length; j++)
                            {
                                vertices[v].BoneIndices.Add(envelop.Indices[j]);
                                vertices[v].BoneWeights.Add(envelop.Weights[j]);

                                Console.WriteLine($"env {envelop.Indices[j]} w {envelop.Weights[j]}");
                            }
                        }
                    }
                }
                mesh.Vertices.AddRange(vertices);
            }
        }

        public class Shape
        {
            public int BoneIndex { get; set; }
            public List<STVertex> Vertices = new List<STVertex>();
        }

        //Placement data to parse GX display list
        private class BufferData
        {
            public uint PositionsOffset { get; set; }
            public uint NormalsOffset { get; set; }
            public uint NBTOffset { get; set; }
            public List<uint> TexCoordsOffset = new List<uint>();
            public uint ColorsOffset { get; set; }
        }
    }
}