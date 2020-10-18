using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.IO;

namespace CTRLibrary.Grezzo
{
    public class CTXB_Parser
    {
        public List<Chunk> Chunks = new List<Chunk>();

        public List<CTXB.TextureWrapper> Textures = new List<CTXB.TextureWrapper>();

        public CTXB_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                string Magic = reader.ReadSignature(4, "ctxb");
                uint FileSize = reader.ReadUInt32();
                uint ChunkCount = reader.ReadUInt32();
                reader.ReadUInt32(); //padding
                uint ChunkOffset = reader.ReadUInt32();
                uint TextureDataOffset = reader.ReadUInt32();
                for (int i = 0; i < ChunkCount; i++)
                    Chunks.Add(new Chunk(reader));

                for (int i = 0; i < ChunkCount; i++)
                {
                    for (int t = 0; t < Chunks[i].Textures.Count; t++)
                    {
                        var texWrapper = new CTXB.TextureWrapper(Chunks[i].Textures[t]);
                        texWrapper.Name = $"Texture_{t}";
                        if (Chunks[i].Textures[t].Name != string.Empty)
                            texWrapper.Name = Chunks[i].Textures[t].Name;

                        texWrapper.Width = Chunks[i].Textures[t].Width;
                        texWrapper.Height = Chunks[i].Textures[t].Height;
                        texWrapper.Platform = new CTRSwizzle(Chunks[i].Textures[t].PicaFormat);

                        reader.SeekBegin(TextureDataOffset + Chunks[i].Textures[t].DataOffset);
                        Chunks[i].Textures[t].ImageData = reader.ReadBytes((int)Chunks[i].Textures[t].ImageSize);

                        Textures.Add(texWrapper);
                    }
                }
            }
        }

        public void Write(FileWriter writer)
        {
            writer.WriteSignature("ctxb");
            writer.Write(uint.MaxValue);
            writer.Write(Chunks.Count);
            writer.Write(0);
            writer.Write(24);
            writer.Write(24 + Chunks.Count * 8 + (Chunks.Sum(x => x.Textures.Count) * 40));

            foreach (var chunk in Chunks)
            {
                long pos = writer.Position;
                writer.WriteSignature(chunk.Magic);
                writer.Write(uint.MaxValue);
                writer.Write(chunk.Textures.Count);
                foreach (var tex in chunk.Textures)
                    tex.Write(writer);

                writer.WriteSectionSizeU32(pos + 4, pos, writer.Position);
            }

            uint dataOffset = 0;
            foreach (var chunk in Chunks)
            {
                foreach (var tex in chunk.Textures)
                {
                    tex.DataOffset = dataOffset;
                    writer.Write(tex.ImageData);

                    dataOffset += (uint)tex.ImageData.Length;
                }
            }

            using (writer.TemporarySeek(4, System.IO.SeekOrigin.Begin))
            {
                writer.Write((uint)writer.BaseStream.Length);
            }
        }

        public class Chunk
        {
            public readonly string Magic = "tex ";

            public List<Texture> Textures = new List<Texture>();

            public Chunk(FileReader reader)
            {
                reader.ReadSignature(4, Magic);
                uint SectionSize = reader.ReadUInt32();
                uint TextureCount = reader.ReadUInt32();
                for (int i = 0; i < TextureCount; i++)
                    Textures.Add(new Texture(reader));
            }
        }

        public class Texture
        {
            public ushort MaxLevel { get; set; }
            public ushort Unknown { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public string Name { get; set; }

            public uint ImageSize { get; set; }
            public uint DataOffset { get; set; }

            public CTR_3DS.PICASurfaceFormat PicaFormat;

            public byte[] ImageData;

            public enum TextureFormat : uint
            {
                ETC1 = 0x0000675A,
                ETC1A4 = 0x0000675B,
                RGBA8 = 0x14016752,
                RGBA4444 = 0x80336752,
                RGBA5551 = 0x80346752,
                RGB565 = 0x83636754,
                RGB8 = 0x14016754,
                A8 = 0x14016756,
                L8 = 0x14016757,
                L4 = 0x67616757,
                LA8 = 0x14016758,
            }

            public Texture() { }

            public Texture(FileReader reader)
            {
                ImageSize = reader.ReadUInt32();
                MaxLevel = reader.ReadUInt16();
                Unknown = reader.ReadUInt16();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                TextureFormat Format = reader.ReadEnum<TextureFormat>(true);
                DataOffset = reader.ReadUInt32();
                Name = reader.ReadString(16).TrimEnd('\0');

                PicaFormat = FormatList[Format];
            }

            public void Write(FileWriter writer)
            {
                TextureFormat format = FormatList.FirstOrDefault(x => x.Value == PicaFormat).Key;

                writer.Write(ImageData.Length);
                writer.Write(MaxLevel);
                writer.Write(Unknown);
                writer.Write((ushort)Width);
                writer.Write((ushort)Height);
                writer.Write(format, true);
                writer.Write(DataOffset);
                writer.WriteString(Name, 16);
            }

            public static Dictionary<TextureFormat, CTR_3DS.PICASurfaceFormat> FormatList =
                new Dictionary<TextureFormat, CTR_3DS.PICASurfaceFormat>()
                {
                    { TextureFormat.A8, CTR_3DS.PICASurfaceFormat.A8 },
                    { TextureFormat.ETC1, CTR_3DS.PICASurfaceFormat.ETC1 },
                    { TextureFormat.ETC1A4, CTR_3DS.PICASurfaceFormat.ETC1A4 },
                    { TextureFormat.L4, CTR_3DS.PICASurfaceFormat.L4 },
                    { TextureFormat.L8, CTR_3DS.PICASurfaceFormat.L8 },
                    { TextureFormat.LA8, CTR_3DS.PICASurfaceFormat.LA8 },
                    { TextureFormat.RGB565, CTR_3DS.PICASurfaceFormat.RGB565 },
                    { TextureFormat.RGBA4444, CTR_3DS.PICASurfaceFormat.RGBA4 },
                    { TextureFormat.RGBA5551, CTR_3DS.PICASurfaceFormat.RGBA5551 },
                    { TextureFormat.RGBA8, CTR_3DS.PICASurfaceFormat.RGBA8 },
                    { TextureFormat.RGB8, CTR_3DS.PICASurfaceFormat.RGB8 },
                };
        }
    }
}
