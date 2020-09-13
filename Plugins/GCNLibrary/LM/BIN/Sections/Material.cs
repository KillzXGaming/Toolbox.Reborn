using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM.BIN
{
    public class Material
    {
        // 1 == Dynamic
        // 0 == Basic with only materials from bin.
        public byte DynamicLighting { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public STColor8 AmbientColor { get; set; }

        public short[] SamplerIndices { get; set; }

        public short Unknown4 { get; set; }
        public short Unknown5 { get; set; }

        public byte[] Padding { get; set; }

        public List<Sampler> Samplers = new List<Sampler>();

        public Material() { }

        public Material(FileReader reader, BIN_Parser header)
        {
            DynamicLighting = reader.ReadByte();
            Unknown2 = reader.ReadByte();
            Unknown3 = reader.ReadByte();
            AmbientColor = reader.ReadColor8RGBA();
            reader.ReadByte(); //padding
            SamplerIndices = reader.ReadInt16s(8);
            Unknown4 = reader.ReadInt16();
            Unknown5 = reader.ReadInt16();
            Padding = reader.ReadBytes(12);

            for (int i = 0; i < 8; i++)
            {
                if (SamplerIndices[i] != -1)
                    Samplers.Add(header.ReadSection<Sampler>(reader, SamplerIndices[i]));
            }
         }

        public void Write(FileWriter writer, BIN_Parser header)
        {
            writer.Write(DynamicLighting);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(AmbientColor);
            writer.Write((byte)0);
            writer.Write(SamplerIndices);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Padding);
        }
    }
}
