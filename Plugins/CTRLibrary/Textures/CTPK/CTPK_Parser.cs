using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;
using System.Runtime.InteropServices;
using System.IO;

namespace CTRLibrary
{
    public class CTPK_Parser
    {
        public const string MAGIC = "CTPK";

        public ushort Version { get; set; }

        public List<TextureEntry> Textures { get; set; }

        public List<ConversionInfo> ConversionInfos { get; set; }

        public CTPK_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        void Read(FileReader reader)
        {
            Textures = new List<TextureEntry>();
            ConversionInfos = new List<ConversionInfo>();

            reader.ReadSignature(4, MAGIC);
            Version = reader.ReadUInt16();
            ushort numTextures = reader.ReadUInt16();
            uint textureDataOffset = reader.ReadUInt32();
            uint hashArrayOffset = reader.ReadUInt32();
            uint conversionInfoOffset = reader.ReadUInt32();

            reader.Position = 0x20;
            for (int i = 0; i < numTextures; i++) {
                TextureEntry entry = new TextureEntry();
                entry.Read(reader);
                Textures.Add(entry);
            }

            reader.SeekBegin(conversionInfoOffset);
            for (int i = 0; i < numTextures; i++)
                ConversionInfos.Add(reader.ReadStruct<ConversionInfo>());

            for (int i = 0; i < numTextures; i++)
            {
                reader.SeekBegin(textureDataOffset + Textures[i].DataOffset);
                Textures[i].ImageData = reader.ReadBytes((int)Textures[i].ImageSize);
            }
        }

        void Write(FileWriter writer) {
            writer.Write(MAGIC);
        }


        public class TextureEntry
        {
            public CTR_3DS.PICASurfaceFormat PicaFormat { get; set; }

            public string Name { get; set; }
            public uint ImageSize { get; set; }
            public uint TextureFormat { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public byte MipCount { get; set; }
            public byte Type { get; set; }
            public ushort FaceCount { get; set; }
            public uint UnixTimeStamp { get; set; }

            internal uint DataOffset { get; set; }
            internal uint BitmapSizeOffset { get; set; }

            public byte[] ImageData { get; set; }

            public void Read(FileReader reader)
            {
                Name = reader.LoadString(false, typeof(uint));
                ImageSize = reader.ReadUInt32();
                DataOffset = reader.ReadUInt32();
                TextureFormat = reader.ReadUInt32();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                MipCount = reader.ReadByte();
                Type = reader.ReadByte();
                FaceCount = reader.ReadUInt16();
                BitmapSizeOffset = reader.ReadUInt32();
                UnixTimeStamp = reader.ReadUInt32();

                PicaFormat = (CTR_3DS.PICASurfaceFormat)TextureFormat;
            }

            public void Write(FileWriter writer)
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ConversionInfo
        {
            public byte TextureFormat { get; set; }
            public byte Unknown { get; set; }
            public bool Compressed { get; set; }
            public byte Etc1Quality { get; set; }
        }
    }
}
