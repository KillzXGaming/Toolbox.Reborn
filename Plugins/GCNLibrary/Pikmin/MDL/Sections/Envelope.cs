using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1.Model
{
    public class Envelope : IModChunk
    {
        public float[] Weights;
        public ushort[] Indices;

        public void Read(FileReader reader)
        {
            ushort count = reader.ReadUInt16();

            Weights = new float[count];
            Indices = new ushort[count];
            for (int i = 0; i < count; i++) {
                Indices[i] = reader.ReadUInt16();
                Weights[i] = reader.ReadSingle();
            }
        }

        public void Write(FileWriter writer)
        {
            writer.Write((ushort)Indices.Length);
            for (int i = 0; i < Indices.Length; i++) {
                writer.Write(Indices[i]);
                writer.Write(Weights[i]);
            }
        }
    }
}
