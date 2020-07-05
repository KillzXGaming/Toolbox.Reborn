using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM
{
    public class SLK_Parser
    {
        public List<Shape> Shapes = new List<Shape>();

        public SLK_Parser() { }

        public SLK_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint padding = reader.ReadUInt32();
            ushort frameCount = reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();
            ushort shapeCount = reader.ReadUInt16();
            reader.ReadBytes(6); //padding
            uint blendDataOffset = reader.ReadUInt32();
            uint blendIndexListOffset = reader.ReadUInt32();
            uint blendCountListOffset = reader.ReadUInt32();
            uint ShapeIndexListOffset = reader.ReadUInt32();

            //To get the data, find shape indices first
            //Use those indices to fill a blend list
            reader.SeekBegin(ShapeIndexListOffset);
            ushort[] shapeIndices = reader.ReadUInt16s(shapeCount);

        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);
        }

        public class Shape
        {
            public List<BlendList> BlendLists = new List<BlendList>();
        }

        public class BlendList
        {
            public KeyFrame[] KeyFrames { get; set; }
        }

        public class KeyFrame
        {
            public float Frame { get; set; }
            public float Value { get; set; }
            public float Slope { get; set; }
        }
    }
}
