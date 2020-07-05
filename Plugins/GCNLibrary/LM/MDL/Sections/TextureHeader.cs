using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace GCNLibrary.LM.MDL
{
    public class TextureHeader : ISection
    {
        public Decode_Gamecube.TextureFormats Format;
        public byte Padding;
        public ushort Width;
        public ushort Height;
        public byte[] ImageData;

        public void Read(FileReader reader)
        {
            byte format = reader.ReadByte();
            Padding = reader.ReadByte();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            reader.ReadBytes(26);

            if (!FormatList.ContainsKey(format))
                throw new Exception($"Unknown textue format! {format}");

            Format = FormatList[format];
            var size = Decode_Gamecube.GetDataSize(Format, Width, Height);
            ImageData = reader.ReadBytes(size);
        }

        public void Write(FileWriter writer)
        {
            byte format = FormatList.FirstOrDefault(x => x.Value == Format).Key;
            writer.Write(format);
            writer.Write(Padding);
            writer.Write(Width);
            writer.Write(Height);
            writer.Align(32);
            writer.Write(ImageData);
        }

        Dictionary<byte, Decode_Gamecube.TextureFormats> FormatList = new Dictionary<byte, Decode_Gamecube.TextureFormats>()
        {
            { 0x03, Decode_Gamecube.TextureFormats.I4 },
            { 0x04, Decode_Gamecube.TextureFormats.I8 },
            { 0x05, Decode_Gamecube.TextureFormats.IA4 },
            { 0x06, Decode_Gamecube.TextureFormats.IA8 },
            { 0x07, Decode_Gamecube.TextureFormats.RGB565 },
            { 0x08, Decode_Gamecube.TextureFormats.RGB5A3 },
            { 0x09, Decode_Gamecube.TextureFormats.RGBA32 },
            { 0x0A, Decode_Gamecube.TextureFormats.CMPR },
        };
    }
}
