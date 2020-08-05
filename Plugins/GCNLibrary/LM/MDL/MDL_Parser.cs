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

namespace GCNLibrary.LM.MDL
{
    public class MDL_Parser
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public uint Magic = 0x04B40000;
            public ushort FaceCount;
            public ushort Padding;
            public ushort NodeCount;
            public ushort ShapePacketCount;
            public ushort WeightCount;
            public ushort JointCount;
            public ushort VertexCount;
            public ushort NormalCount;
            public ushort ColorCount;
            public ushort TexcoordCount;
            public ushort Padding2;
            public ushort Padding3;
            public ushort Padding4;
            public ushort Padding5;
            public ushort TextureCount;
            public ushort Padding6;
            public ushort TextureObjectCount;
            public ushort DrawElementsCount;
            public ushort MaterialCount;
            public ushort ShapeCount;
            public uint Padding7;
            public uint NodeOffset;
            public uint ShapePacketOffset;
            public uint MatrixOffset;
            public uint WeightOffset;
            public uint JointIndexOffset;
            public uint WeightCountTableOffset;
            public uint VertexOffset;
            public uint NormalOffset;
            public uint ColorOffset;
            public uint TexcoordOffset;
            public uint Padding8;
            public uint Padding9;
            public uint TextureOffset;
            public uint Padding10;
            public uint MaterialOffset;
            public uint TextureObjectOffset;
            public uint ShapeOffset;
            public uint DrawElementOffset;
            public uint Padding11;
            public uint Padding12;
        }

        public class Mesh
        {
            public DrawElement DrawElement { get; set; }
            public Shape Shape { get; set; }
            public ShapePacket[] Packets { get; set; }

            public List<DrawPacket> DrawPackets = new List<DrawPacket>();
            public List<DrawPacket> DrawLODPackets = new List<DrawPacket>();
        }

        public class DrawPacket
        {
      
        }

        public class DrawList
        {
     
        }

        public Header FileHeader { get; set; }

        public TextureHeader[] Textures { get; set; }
        public Sampler[] Samplers { get; set; }
        public Shape[] Shapes { get; set; }
        public ShapePacket[] ShapePackets { get; set; }
        public DrawElement[] DrawElements { get; set; }

        public Vector3[] Positions { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector2[] TexCoords { get; set; }
        public Vector4[] Colors { get; set; }

        public Matrix4[] Matrix4Table { get; set; }
        public Material[] Materials { get; set; }
        public Node[] Nodes { get; set; }

        public Vector3[] LODPositions { get; set; }
        public Vector3[] LODNormals { get; set; }

        public List<Mesh> Meshes = new List<Mesh>();

        public Weight[] Weights { get; set; }

        public MDL_Parser() {
            FileHeader = new Header();
            LODPositions = new Vector3[0];
            LODNormals = new Vector3[0];
        }

        public MDL_Parser(Stream stream)
        {
            int MAX_LOD = 2;

            using (var reader = new FileReader(stream)) {
                reader.SetByteOrder(true);
                FileHeader = reader.ReadStruct<Header>();
                Textures = ReadList<TextureHeader>(reader, FileHeader.TextureOffset, FileHeader.TextureCount, true);
                Samplers = ReadList<Sampler>(reader, FileHeader.TextureObjectOffset, FileHeader.TextureObjectCount);
                Shapes = ReadList<Shape>(reader, FileHeader.ShapeOffset, FileHeader.ShapeCount);
                ShapePackets = ReadList<ShapePacket>(reader, FileHeader.ShapePacketOffset, (ushort)(FileHeader.ShapePacketCount * MAX_LOD));
                DrawElements = ReadList<DrawElement>(reader, FileHeader.DrawElementOffset, FileHeader.DrawElementsCount);
                Materials = ReadList<Material>(reader, FileHeader.MaterialOffset, FileHeader.MaterialCount);
                Nodes = ReadList<Node>(reader, FileHeader.NodeOffset, FileHeader.NodeCount);
                Positions = new Vector3[FileHeader.VertexCount];
                Normals = new Vector3[FileHeader.NormalCount];
                TexCoords = new Vector2[FileHeader.TexcoordCount];
                Colors = new Vector4[FileHeader.ColorCount];
                Matrix4Table = new Matrix4[FileHeader.JointCount + FileHeader.WeightCount];
                for (int i = 0; i < Matrix4Table.Length; i++)
                    Matrix4Table[i] = Matrix4.Identity;

                reader.SeekBegin(FileHeader.VertexOffset);
                for (int i = 0; i < FileHeader.VertexCount; i++)
                    Positions[i] = reader.ReadVec3();
                
                //Weird hacky way to find lod positions
                uint numLodPositions = (FileHeader.NormalOffset - FileHeader.VertexOffset) / 12;
                numLodPositions -= FileHeader.VertexCount;

                LODPositions = new Vector3[numLodPositions];
                for (int i = 0; i < numLodPositions; i++)
                    LODPositions[i] = reader.ReadVec3();

                reader.SeekBegin(FileHeader.NormalOffset);
                for (int i = 0; i < FileHeader.NormalCount; i++)
                    Normals[i] = reader.ReadVec3();

                reader.SeekBegin(FileHeader.TexcoordOffset);
                for (int i = 0; i < FileHeader.TexcoordCount; i++)
                    TexCoords[i] = reader.ReadVec2();

                reader.SeekBegin(FileHeader.ColorOffset);
                for (int i = 0; i < FileHeader.ColorCount; i++)
                    Colors[i] = new Vector4(
                        reader.ReadByte() / 255f,
                        reader.ReadByte() / 255f,
                        reader.ReadByte() / 255f,
                        reader.ReadByte() / 255f);

                reader.SeekBegin(FileHeader.MatrixOffset);
                for (int i = 0; i < FileHeader.JointCount; i++)
                {
                    float[] matrix = reader.ReadSingles(12);

                    //Matrix 3x4 turn into matrix 4x4
                    Matrix4Table[i] = new Matrix4()
                    {
                        Row0 = new Vector4(matrix[0], matrix[1], matrix[2], matrix[3]),
                        Row1 = new Vector4(matrix[4], matrix[5], matrix[6], matrix[7]),
                        Row2 = new Vector4(matrix[8], matrix[9], matrix[10], matrix[11]),
                        Row3 = new Vector4(0, 0, 0, 1),
                    };
                }

                //Create a table to store weights
                Weights = new Weight[FileHeader.WeightCount];
                for (int i = 0; i < FileHeader.WeightCount; i++)
                    Weights[i] = new Weight();

                if (FileHeader.WeightCountTableOffset != 0)
                {
                    //For weights get the weight count table first
                    reader.SeekBegin(FileHeader.WeightCountTableOffset);
                    byte[] weightCounters = reader.ReadBytes(FileHeader.WeightCount);

                    //Then loop through counters and get our weights
                    reader.SeekBegin(FileHeader.WeightOffset);
                    for (int i = 0; i < weightCounters.Length; i++) {
                        for (int j = 0; j < weightCounters[i]; j++) {
                            Weights[i].Weights.Add(reader.ReadSingle());
                        }
                    }

                    //Then loop through counters and get our joint indices
                    reader.SeekBegin(FileHeader.JointIndexOffset);
                    for (int i = 0; i < weightCounters.Length; i++) {
                        for (int j = 0; j < weightCounters[i]; j++) {
                            Weights[i].JointIndices.Add(reader.ReadUInt16());
                        }
                    }
                }

                foreach (var elem in DrawElements)
                    Meshes.Add(ParsePackets(reader, elem));
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                writer.SetByteOrder(true);

                FileHeader.VertexCount = (ushort)Positions.Length;
                FileHeader.NormalCount = (ushort)Normals.Length;
                FileHeader.TexcoordCount = (ushort)TexCoords.Length;
                FileHeader.ColorCount = (ushort)Colors.Length;
                FileHeader.TextureCount = (ushort)Textures.Length;
                FileHeader.TextureObjectCount = (ushort)Samplers.Length;
                FileHeader.ShapeCount = (ushort)Shapes.Length;
                FileHeader.ShapePacketCount = (ushort)(ShapePackets.Length / 2);
                FileHeader.WeightCount = (ushort)Weights.Length;
                FileHeader.NodeCount = (ushort)Nodes.Length;
                FileHeader.JointCount = (ushort)Nodes.Length;
                FileHeader.DrawElementsCount = (ushort)DrawElements.Length;
                FileHeader.MaterialCount = (ushort)Materials.Length;
                writer.WriteStruct(FileHeader);

                writer.SeekBegin(128);
                for (int i = 0; i < ShapePackets.Length; i++) {
                    ShapePackets[i].DataOffset = (uint)writer.Position;
                    ShapePackets[i].DataSize = (uint)ShapePackets[i].Data.Length;

                    writer.Write(ShapePackets[i].Data);
                }

                writer.Align(32);

                //Save texture headers. Offset will save later
                uint[] textureOffsets = new uint[Textures.Length];
                for (int i = 0; i < Textures.Length; i++)
                {
                    textureOffsets[i] = (uint)writer.Position;
                    Textures[i].Write(writer);
                }

                writer.WriteUint32Offset(104);
                for (int i = 0; i < Materials.Length; i++)
                    Materials[i].Write(writer);

                writer.WriteUint32Offset(108);
                for (int i = 0; i < Samplers.Length; i++)
                    writer.WriteStruct(Samplers[i]);

                writer.WriteUint32Offset(112);
                for (int i = 0; i < Shapes.Length; i++)
                    writer.WriteStruct(Shapes[i]);

                writer.WriteUint32Offset(116);
                for (int i = 0; i < DrawElements.Length; i++)
                    writer.WriteStruct(DrawElements[i]);

                writer.WriteUint32Offset(52);
                for (int i = 0; i < ShapePackets.Length; i++)
                    ShapePackets[i].Write(writer);

                writer.WriteUint32Offset(96);
                writer.Write(textureOffsets);

                writer.WriteUint32Offset(72);
                for (int i = 0; i < Positions.Length; i++)
                    writer.Write(Positions[i]);
                for (int i = 0; i < LODPositions.Length; i++)
                    writer.Write(LODPositions[i]);

                writer.WriteUint32Offset(76);
                for (int i = 0; i < Normals.Length; i++)
                    writer.Write(Normals[i]);

                writer.WriteUint32Offset(80);
                for (int i = 0; i < Colors.Length; i++)
                {
                    writer.Write((byte)(Colors[i].X * 255));
                    writer.Write((byte)(Colors[i].Y * 255));
                    writer.Write((byte)(Colors[i].Z * 255));
                    writer.Write((byte)(Colors[i].W * 255));
                }

                writer.WriteUint32Offset(84);
                for (int i = 0; i < TexCoords.Length; i++)
                    writer.Write(TexCoords[i]);

                writer.WriteUint32Offset(48);
                for (int i = 0; i < Nodes.Length; i++)
                    writer.WriteStruct(Nodes[i]);

                writer.WriteUint32Offset(56);
                for (int i = 0; i < FileHeader.JointCount; i++)
                {
                    var mat = Matrix4Table[i];
                    writer.Write(mat.Row0);
                    writer.Write(mat.Row1);
                    writer.Write(mat.Row2);
                }

                writer.WriteUint32Offset(60);
                for (int i = 0; i < Weights.Length; i++) {
                    for (int j = 0; j < Weights[i].Weights.Count; j++) {
                        writer.Write(Weights[i].Weights[j]);
                    }
                }

                writer.WriteUint32Offset(64);
                for (int i = 0; i < Weights.Length; i++) {
                    for (int j = 0; j < Weights[i].Weights.Count; j++) {
                        writer.Write((ushort)Weights[i].JointIndices[j]);
                    }
                }

                writer.WriteUint32Offset(68);
                for (int i = 0; i < Weights.Length; i++) {
                    writer.Write((byte)Weights[i].Weights.Count);
                }
            }
        }

        public class Weight
        {
            public List<int> JointIndices = new List<int>();
            public List<float> Weights = new List<float>();
        }

        private Mesh ParsePackets(FileReader reader, DrawElement element)
        {
            Mesh mesh = new Mesh();
            mesh.DrawElement = element;
            mesh.Shape = Shapes[element.ShapeIndex];
            mesh.Packets = ParsePackets(mesh, 0, reader, element).ToArray();
           //mesh.Packets = ParsePackets(mesh, (ShapePackets.Length / 2), reader, element).ToArray();
            return mesh;
        }

        private List<ShapePacket> ParsePackets(Mesh mesh, int startLODIndex, FileReader reader, DrawElement element)
        {
            List<ShapePacket> drawpackets = new List<ShapePacket>();

            ushort[] matrixIndices = new ushort[10];
            int startIndex = startLODIndex + mesh.Shape.PacketBeginIndex;
            for (int i = startIndex; i < startIndex + mesh.Shape.PacketCount; i++)
            {
                var packet = ShapePackets[i];
                drawpackets.Add(packet);

                bool isLOD = startLODIndex > 0;

                for (int m = 0; m < packet.MatrixIndicesCount; m++)
                {
                    if (packet.MatrixIndices[m] == ushort.MaxValue) continue;
                    matrixIndices[m] = packet.MatrixIndices[m];
                }

                reader.SeekBegin(packet.DataOffset);
                while (reader.BaseStream.Position < packet.DataOffset + packet.DataSize)
                {
                    byte opcode = reader.ReadByte();
                    if (opcode == 0)
                        continue;

                    ShapePacket.DrawList drawPacket = new ShapePacket.DrawList();
                    drawPacket.OpCode = opcode;
                    packet.DrawLists.Add(drawPacket);

                    ushort numVertices = reader.ReadUInt16();
                    drawPacket.Vertices = new List<ShapePacket.VertexGroup>();

                    for (int v = 0; v < numVertices; v++)
                    {
                        var drawList = new ShapePacket.VertexGroup();
                        if (!isLOD)
                        {
                            drawList.MatrixIndex = reader.ReadSByte();
                            if (drawList.MatrixIndex != -1)
                                drawList.MatrixDataIndex = matrixIndices[(drawList.MatrixIndex / 3)];

                            drawList.Tex0MatrixIndex = reader.ReadSByte();
                            drawList.Tex1MatrixIndex = reader.ReadSByte();
                            drawList.PositionIndex = reader.ReadInt16();

                            if (FileHeader.NormalCount > 0)
                                drawList.NormalIndex = reader.ReadInt16();
                            if (mesh.Shape.NormalFlags > 1) //NBT
                            {
                                drawList.TangentIndex = reader.ReadInt16();
                                drawList.BinormalIndex = reader.ReadInt16();
                            }

                            if (FileHeader.ColorCount > 0)
                                drawList.ColorIndex = reader.ReadInt16();
                            if (FileHeader.TexcoordCount > 0)
                                drawList.TexCoordIndex = reader.ReadInt16();
                        }
                        else
                        {
                            drawList.MatrixIndex = reader.ReadSByte();
                             if (drawList.MatrixIndex != -1)
                                drawList.MatrixDataIndex = matrixIndices[(drawList.MatrixIndex / 3)];

                            drawList.PositionIndex = reader.ReadInt16();
                            if (FileHeader.NormalCount > 0)
                                drawList.NormalIndex = reader.ReadByte();
                        }

                        drawPacket.Vertices.Add(drawList);
                    }
                }
            }
            return drawpackets;
        }

        private T[] ReadList<T>(FileReader reader, uint offset, ushort count, bool offsetArray = false)
            where T : new()
        {
            T[] instance = new T[count];
            if (count == 0)
                return instance;

            reader.SeekBegin(offset);
            uint[] offsets = new uint[count];
            if (offsetArray)
                offsets = reader.ReadUInt32s(count);

            for (int i = 0; i < count; i++) {
                if (offsetArray) reader.SeekBegin(offsets[i]);

                instance[i] = new T();
                if (typeof(ISection).IsAssignableFrom(typeof(T)))
                    ((ISection)instance[i]).Read(reader);
                else
                    instance[i] = reader.ReadStruct<T>();
            }

            return instance;
        }
    }
}
