using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace Toolbox.Core.GX
{
    public class DisplayListHelper
    {
        public class Config
        {
            public int OpCodeShift = 0;
        }   

        public class VertexBuffer
        {
                
        }

        public class Packet
        {

        }

        public class MeshSettings
        {
            public STGenericMesh Mesh { get; set; }
            public GXVertexLayout[] Layouts { get; set; }
        }

        public static VertexBuffer[] CreatePackets(MeshSettings[] meshSettings)
        {
            VertexBuffer[] vertexBuffers = new VertexBuffer[meshSettings.Length];
            for (int i = 0; i < meshSettings.Length; i++)
            {
                 for (int l = 0; l < meshSettings[i].Layouts.Length; l++)
                {
                    var layout = meshSettings[i].Layouts[l];
                    switch (layout.Attribute)
                    {

                    }
                }
            }
            return vertexBuffers;
        }

        public static List<STVertex> ReadDisplayLists(Stream stream, Stream dataStream, 
            GXVertexLayout[] layouts, Config config = null, ushort[] matrixIndices = null)
        {
            if (config == null) config = new Config();

            int numColors = GetColorCount(layouts);
            int numTexCoords = GetTexCoordsCount(layouts);

            bool hasVertexColors = layouts.Any(x => x.Attribute == GXAttributes.Color0);

            List<GXOpCodes> operations = new List<GXOpCodes>();

            List<STVertex> vertices = new List<STVertex>();
            using (var dataReader = new FileReader(dataStream, true))
            using (var reader = new FileReader(stream, true)) {
                dataReader.SetByteOrder(true);
                reader.SetByteOrder(true);

                while (!reader.EndOfStream)
                {
                    GXOpCodes opCode = (GXOpCodes)(reader.ReadByte());
                    if (opCode == GXOpCodes.NOOP) continue;

                    opCode = (GXOpCodes)((byte)opCode + config.OpCodeShift);

                    if (!operations.Contains(opCode))
                        operations.Add(opCode);

                    List<STVertex> verts = new List<STVertex>();

                    ushort numVertices = reader.ReadUInt16();
                    for (int v = 0; v < numVertices; v++) {
                        STVertex vertex = new STVertex();
                        vertex.TexCoords = new OpenTK.Vector2[numTexCoords];
                        if (hasVertexColors)
                            vertex.Colors = new OpenTK.Vector4[numColors];

                        for (int l = 0; l < layouts.Length; l++) {
                            var index = ReadLayout(reader, layouts[l]);
                            ParseData(vertex, dataReader, layouts[l], index, matrixIndices);
                        }
                        verts.Add(vertex);
                    }

                    if (opCode == GXOpCodes.DRAW_TRIANGLES)
                    {
                        vertices.AddRange(verts);
                    }
                    else if (opCode == GXOpCodes.DRAW_TRIANGLE_FAN)
                    {
                        verts = ConvertTriFans(verts);
                        vertices.AddRange(verts);
                    }
                    else if (opCode == GXOpCodes.DRAW_TRIANGLE_STRIP)
                    {
                        verts = ConvertTriStrips(verts);
                        vertices.AddRange(verts);
                    }
                    else if (opCode == GXOpCodes.DRAW_QUADS || opCode == GXOpCodes.DRAW_QUADS_2)
                    {
                        verts = ConvertQuads(verts);
                        vertices.AddRange(verts);
                    }
                    else 
                    {
                        Console.WriteLine($"Unknown opCode {opCode}");
                        vertices.AddRange(verts);
                    }
                }
            }
            operations.Clear();
            return vertices;
        }

        private static int GetTexCoordsCount(GXVertexLayout[] layouts)
        {
            int numTexCoords = 0;
            for (int i = 0; i < layouts.Length; i++)
            {
                if (layouts[i].Attribute >= GXAttributes.TexCoord0 &&
                    layouts[i].Attribute <= GXAttributes.TexCoord7)
                    numTexCoords++;
            }
            return numTexCoords;
        }

        private static int GetColorCount(GXVertexLayout[] layouts)
        {
            int numColors = 0;
            for (int i = 0; i < layouts.Length; i++)
            {
                if (layouts[i].Attribute >= GXAttributes.Color0 || 
                    layouts[i].Attribute <= GXAttributes.Color1)
                    numColors++;
            }
            return numColors;
        }

        private static List<STVertex> ConvertQuads(List<STVertex> vertices)
        {
            int indexCount = ((vertices.Count * 6) / 4) * 3;

            List<STVertex> outVertices = new List<STVertex>();
            for (int index = 0; index < vertices.Count / 4; index += 4)
            {
                var vert1 = vertices[index + 0];
                var vert2 = vertices[index + 1];
                var vert3 = vertices[index + 2];

                var vert4 = vertices[index + 0];
                var vert5 = vertices[index + 2];
                var vert6 = vertices[index + 3];

                outVertices.Add(vert1);
                outVertices.Add(vert2);
                outVertices.Add(vert3);
                outVertices.Add(vert4);
                outVertices.Add(vert5);
                outVertices.Add(vert6);
            }
            return outVertices;
        }

        private static List<STVertex> ConvertTriFans(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            int vertexId = 0;
            int firstVertex = vertexId;

            for (int index = 0; index < 3; index++)
                outVertices.Add(vertices[index]);

            for (int index = 2; index < vertices.Count; index++)
            {
                var vert1 = vertices[firstVertex];
                var vert2 = vertices[index - 1];
                var vert3 = vertices[index];

                if (!vert1.Position.Equals(vert2.Position) &&
                    !vert2.Position.Equals(vert3.Position) &&
                    !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private static List<STVertex> ConvertTriStrips(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            for (int index = 2; index < vertices.Count; index++)
            {
                bool isEven = (index % 2 != 1);

                var vert1 = vertices[index - 2];
                var vert2 = isEven ? vertices[index] : vertices[index - 1];
                var vert3 = isEven ? vertices[index - 1] : vertices[index];

                if (!vert1.Position.Equals(vert2.Position) &&
                    !vert2.Position.Equals(vert3.Position) &&
                    !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private static void ParseData(STVertex vertex, FileReader dataReader, GXVertexLayout layout, int index, ushort[] matrixIndices)
        {
            int numElements = GetElementCount(layout.Attribute);
            int stride = GetDataStride(layout.CompType) * numElements;
            if (IsColor(layout))
                stride = GetColorDataStride(layout.CompType);

            using (dataReader.TemporarySeek(layout.DataOffset + (index * stride), SeekOrigin.Begin))
            {
               // Console.WriteLine($"attribute {layout.Attribute} stride {stride} index {index}");
                switch (layout.Attribute)
                {
                    case GXAttributes.Position:
                        {
                            float X = ReadDataLayout(dataReader, layout);
                            float Y = ReadDataLayout(dataReader, layout);
                            float Z = ReadDataLayout(dataReader, layout);
                            vertex.Position = new OpenTK.Vector3(X, Y, Z);
                        }
                        break;
                    case GXAttributes.Normal:
                        {
                            float X = ReadDataLayout(dataReader, layout);
                            float Y = ReadDataLayout(dataReader, layout);
                            float Z = ReadDataLayout(dataReader, layout);
                            vertex.Normal = new OpenTK.Vector3(X, Y, Z).Normalized();
                        }
                        break;
                    case GXAttributes.TexCoord0:
                    case GXAttributes.TexCoord1:
                    case GXAttributes.TexCoord2:
                    case GXAttributes.TexCoord3:
                    case GXAttributes.TexCoord4:
                    case GXAttributes.TexCoord5:
                    case GXAttributes.TexCoord6:
                    case GXAttributes.TexCoord7:
                        {
                            int channel = (int)(layout.Attribute - GXAttributes.TexCoord0);
                            float X = ReadDataLayout(dataReader, layout);
                            float Y = ReadDataLayout(dataReader, layout);
                            vertex.TexCoords[channel] = new OpenTK.Vector2(X, Y);
                        }
                        break;
                    case GXAttributes.Color0:
                    case GXAttributes.Color1:
                        {
                            int channel = (int)(layout.Attribute - GXAttributes.Color0);
                            var color = ReadDataColorLayout(dataReader, layout);
                            vertex.Colors[0] = new OpenTK.Vector4(color.X, color.Y, color.Z, color.W);
                        }
                        break;
                    case GXAttributes.PosNormMatrix:
                        {
                            if (index != -1) {
                                int boneID = index / 3;
                                if (matrixIndices != null)
                                    boneID = matrixIndices[index / 3];
                                Console.WriteLine($"BONEID {index} real {boneID}");

                                vertex.BoneIndices.Add(boneID);
                                vertex.BoneWeights.Add(1.0f);
                            }
                        }
                        break;
                }
            }
        }

        private static ushort ReadLayout(FileReader reader, GXVertexLayout layout)
        {
            switch (layout.AttType)
            {
                case GXAttributeType.INDEX16: return reader.ReadUInt16();
                case GXAttributeType.INDEX8: return reader.ReadByte();
            }
            return 0;
        }

        private static int GetElementCount(GXAttributes attribute)
        {
            switch (attribute)
            {
                case GXAttributes.Position:
                case GXAttributes.Normal:
                    return 3;
                case GXAttributes.TexCoord0:
                case GXAttributes.TexCoord1:
                case GXAttributes.TexCoord2:
                case GXAttributes.TexCoord3:
                case GXAttributes.TexCoord4:
                case GXAttributes.TexCoord5:
                case GXAttributes.TexCoord6:
                case GXAttributes.TexCoord7:
                    return 2;
                case GXAttributes.Color0:
                case GXAttributes.Color1:
                    return 4;
            }
            return 0;
        }

        private static int GetDataStride(GXComponentType compType)
        {
            switch (compType)
            {
                case GXComponentType.S8:
                case GXComponentType.U8:
                    return 1;
                case GXComponentType.U16: 
                case GXComponentType.S16:
                    return 2;
                case GXComponentType.F32:
                    return 4;
            }
            return 0;
        }

        private static int GetColorDataStride(GXComponentType compType)
        {
            switch (compType)
            {
                case GXComponentType.RGBA4:
                case GXComponentType.RGB565:
                    return 2;
                case GXComponentType.RGBA6:
                    return 6;
                case GXComponentType.RGB8:
                    return 3;
                case GXComponentType.RGBX8:
                case GXComponentType.RGBA8:
                    return 4;
            }
            return 0;
        }

        static bool IsColor(GXVertexLayout layout)
        {
            switch (layout.CompType)
            {
                case GXComponentType.RGBA4:
                case GXComponentType.RGBA8:
                case GXComponentType.RGBX8:
                case GXComponentType.RGB565:
                case GXComponentType.RGB8:
                    return layout.Attribute == GXAttributes.Color0 ||
                           layout.Attribute == GXAttributes.Color1;
                default:
                    return false;
            }
        }

        private static float ReadDataLayout(FileReader reader, GXVertexLayout layout)
        {
            if (layout.Divisor != 0)
            {
                switch (layout.CompType)
                {
                    case GXComponentType.S8: return reader.ReadSByte() / layout.Divisor;
                    case GXComponentType.U8: return reader.ReadByte() / layout.Divisor;
                    case GXComponentType.U16: return reader.ReadUInt16() / layout.Divisor;
                    case GXComponentType.S16: return reader.ReadInt16() / layout.Divisor;
                }
            }

            switch (layout.CompType)
            {
                case GXComponentType.S8: return reader.ReadSByte();
                case GXComponentType.U8: return reader.ReadByte();
                case GXComponentType.U16: return reader.ReadUInt16();
                case GXComponentType.S16: return reader.ReadInt16();
                case GXComponentType.F32: return reader.ReadSingle();
            }
            return 0;
        }

        private static int GetComponentShift(GXAttributes attribute, GXComponentType type, int compShift)
        {
            if (attribute == GXAttributes.Normal || attribute == GXAttributes.NormalBinormalTangent)
            {
                if (type == GXComponentType.U8 || type == GXComponentType.S8)
                    return 6;
                else if (type == GXComponentType.U16 || type == GXComponentType.S16)
                    return 14;
            }
            return GetComponentShiftRaw(type, compShift);
        }

        private static int GetComponentShiftRaw(GXComponentType type, int compShift)
        {
            switch (type)
            {
                case GXComponentType.F32:
                    return 0;
                case GXComponentType.U8:
                case GXComponentType.U16:
                case GXComponentType.S8:
                case GXComponentType.S16:
                    return compShift;
                default:
                    return 0;
            }
        }

        private static Vector4 ReadDataColorLayout(FileReader reader, GXVertexLayout layout)
        {
            switch (layout.CompType)
            {
                case GXComponentType.RGBA8:
                    return new Vector4(
                        reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f,
                        reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f);
                case GXComponentType.RGB8:
                    return new Vector4(
                        reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f,
                        reader.ReadByte() / 255.0f, 1.0f);
                case GXComponentType.RGBX8:
                    return new Vector4(
                        reader.ReadByte() / 255.0f, reader.ReadByte() / 255.0f,
                        reader.ReadByte() / 255.0f, 1.0f);
                case GXComponentType.RGB565:
                    {
                        short value = reader.ReadInt16();
                        int R = (value & 0xF800) >> 11;
                        int G = (value & 0x07E0) >> 5;
                        int B = (value & 0x001F);
                        return new Vector4(R / 255f, G / 255f, B / 255f, 1.0f);
                    }
                case GXComponentType.RGBA4:
                    {
                        ushort value = reader.ReadUInt16();
                        float R = (float)((value >> 12) & 0x0F) / 0x0F;
                        float G = (float)((value >> 8) & 0x0F) / 0x0F;
                        float B = (float)((value >> 4) & 0x0F) / 0x0F;
                        float A = (float)((value >> 0) & 0x0F) / 0x0F;

                        return new Vector4(R, G, B, A);
                    }
                case GXComponentType.RGBA6:
                    {
                        int value = reader.ReadInt32();
                        int R = (value & 0xFC0000) >> 18;
                        int G = (value & 0x03F000) >> 12;
                        int B = (value & 0x000FC0) >> 6;
                        int A = (value & 0x00003F);
                        return new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
                    }
                default:
                    return new Vector4(
                        reader.ReadByte() / 255f, reader.ReadByte() / 255f,
                        reader.ReadByte() / 255f, reader.ReadByte() / 255f);
            }
            return new Vector4(255,255,255,255);
        }

        public class DisplayList
        {
            public GXVertexLayout[] Layouts { get; set; }
            public GXOpCodes OpCode { get; set; }
            public uint VertexCount { get; set; }
        }

        public class Envelope
        {
            public List<float> Weights = new List<float>();
            public List<short> Joints = new List<short>();
        }

        public static DisplayList[] CreateDisplayList(
            GXVertexLayout[] layouts, List<STVertex> vertices)
        {
            List<DisplayList> displayLists = new List<DisplayList>();

            return displayLists.ToArray();
        }
    }
}
