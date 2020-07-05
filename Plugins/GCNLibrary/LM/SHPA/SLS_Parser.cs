using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;
using OpenTK;

namespace GCNLibrary.LM
{
    public class SLS_Parser
    {
        public List<Shape> Shapes = new List<Shape>();
        public List<MorphGroup> MorphGroups = new List<MorphGroup>();
        public List<Vector3> Positions = new List<Vector3>();

        public SLS_Parser() { }

        public SLS_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint padding = reader.ReadUInt32();
            uint padding2 = reader.ReadUInt32();
            ushort shapeCount = reader.ReadUInt16();
            ushort groupCount = reader.ReadUInt16();
            uint hash = reader.ReadUInt32();
            uint shapeInfoOffset = reader.ReadUInt32();
            uint groupInfoOffset = reader.ReadUInt32();
            uint positionsOffset = reader.ReadUInt32();
            uint normalsOffset = reader.ReadUInt32();
            uint indicesOffset = reader.ReadUInt32();
            uint unkOffset = reader.ReadUInt32();
            uint groupIndicesOffset = reader.ReadUInt32();
            uint unk2Offset = reader.ReadUInt32();

            uint numPositions = (normalsOffset - positionsOffset) / 12;
            reader.SeekBegin(positionsOffset);
            Console.WriteLine($"positionsOffset {positionsOffset}");

            for (int i = 0; i < numPositions; i++)
            {
                Positions.Add(reader.ReadVec3());
                Console.WriteLine($"Position {i} {Positions[i]}");
            }

            reader.SeekBegin(shapeInfoOffset);
            for (int i = 0; i < shapeCount; i++)
                Shapes.Add(new Shape(reader));

            reader.SeekBegin(groupInfoOffset);
            for (int i = 0; i < groupCount; i++)
                MorphGroups.Add(new MorphGroup(reader));

            for (int i = 0; i < groupCount; i++)
            {
                reader.SeekBegin(indicesOffset + (MorphGroups[i].PositionStartIndex * 2));
                ushort[] indices = reader.ReadUInt16s(MorphGroups[i].PositionCount);
                for (int j = 0; j < indices.Length; j++)
                {
                    int index = (indices[j] & 0xff);
                    Console.WriteLine($"index {index}");
                    reader.SeekBegin(positionsOffset + (index * 12));
                    MorphGroups[i].Positions.Add(reader.ReadVec3());
                }
            }
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);
        }

        public class Shape
        {
            public List<MorphGroup> Groups = new List<MorphGroup>();

            public Shape(FileReader reader)
            {
                ushort index = reader.ReadUInt16();
                uint morphGroupIndex = reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                uint elementCount = reader.ReadUInt32();
                uint elemntSize = reader.ReadUInt32();
            }
        }

        public class MorphGroup
        {
            public List<Vector3> Positions = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();

            public ushort PositionStartIndex { get; set; }
            public ushort PositionCount { get; set; }
            public ushort NormalStartIndex { get; set; }
            public ushort NormalCount { get; set; }

            public MorphGroup(FileReader reader)
            {
                PositionStartIndex = reader.ReadUInt16();
                PositionCount = reader.ReadUInt16();
                NormalStartIndex = reader.ReadUInt16();
                NormalCount = reader.ReadUInt16();
            }
        }
    }
}
