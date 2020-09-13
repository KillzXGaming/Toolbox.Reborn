using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.LM.BIN
{
    public class Texture
    {
        public Texture() { }

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public Decode_Gamecube.TextureFormats Format { get; set; }
        public byte Unknown { get; set; }

        public byte[] ImageData { get; set; }

        public Texture(FileReader reader, BIN_Parser header)
        {
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Format = (Decode_Gamecube.TextureFormats)reader.ReadByte();
            Unknown = reader.ReadByte();
            reader.ReadUInt16(); //padding
            uint imageOffset = reader.ReadUInt32();

            using (reader.TemporarySeek(header.TextureOffset + imageOffset, System.IO.SeekOrigin.Begin)) {
                var size = Decode_Gamecube.GetDataSize(Format, Width, Height);
                ImageData = reader.ReadBytes(size);
            }
        }

        public void Write(FileWriter writer, BIN_Parser header)
        {
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((byte)Format);
            writer.Write(Unknown);
            writer.Write((ushort)0);
            writer.Write(uint.MaxValue);
        }
    }
}
