using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.LM.BIN
{
    public class Sampler
    {
        public short TextureIndex { get; set; }
        public short PaletteIndex { get; set; }
        public byte WrapS { get; set; }
        public byte WrapT { get; set; }
        public ushort Unknown { get; set; }

        public Texture Texture { get; set; }

        public Sampler() { }

        public Sampler(FileReader reader, BIN_Parser header)
        {
            TextureIndex = reader.ReadInt16();
            PaletteIndex = reader.ReadInt16();
            WrapS = reader.ReadByte();
            WrapT = reader.ReadByte();
            Unknown = reader.ReadUInt16();
            Texture = header.ReadSection<Texture>(reader, TextureIndex);
        }

        public void Write(FileWriter writer, BIN_Parser header)
        {
            writer.Write(TextureIndex);
            writer.Write(PaletteIndex);
            writer.Write(WrapS);
            writer.Write(WrapT);
            writer.Write(Unknown);
            writer.Write(new byte[12]); //padding
        }
    }
}
