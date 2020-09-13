using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using System.Runtime.InteropServices;

namespace GCNLibrary
{
    public class BTI : STGenericTexture, IFileFormat
    {
        public bool CanSave { get; set; } = true;
        public string[] Description { get; set; } = new string[] { "BTI" };
        public string[] Extension { get; set; } = new string[] { "*.bti" };
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".bti";
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public byte ImageFormat;
            public byte EnableAlpha;
            public ushort Width;
            public ushort Height;
            public byte WrapS;
            public byte WrapT;
            public ushort PaletteFormat;
            public ushort PaletteCount;
            public uint PaletteOffset;
            public uint Unknown;
            public byte MagFilter;
            public byte MinFilter;
            public ushort Unknown2;
            public byte MipCount;
            public byte Unknown3;
            public ushort Unknown4;
            public uint ImageOffset;
        }

        public Header ImageHeader;
        public byte[] ImageData;

        public void Load(Stream stream)
        {
            Name = FileInfo.FileName;
            CanEdit = true;

            using (var reader = new FileReader(stream))
            {
                reader.SetByteOrder(true);
                ImageHeader = reader.ReadStruct<Header>();
                Width = ImageHeader.Width;
                Height = ImageHeader.Height;
                MipCount = ImageHeader.MipCount;
                var format = (Decode_Gamecube.TextureFormats)ImageHeader.ImageFormat;
                var paletteformat = (Decode_Gamecube.PaletteFormats)ImageHeader.PaletteFormat;
                int imageSize = Decode_Gamecube.GetDataSizeWithMips(format, Width, Height, MipCount);

                reader.SeekBegin(ImageHeader.ImageOffset);
                ImageData = reader.ReadBytes(imageSize);
                Platform = new GamecubeSwizzle(format, paletteformat);

                if (ImageHeader.PaletteCount > 0)
                {
                    reader.SeekBegin(ImageHeader.PaletteOffset);
                    ((GamecubeSwizzle)Platform).PaletteData = reader.ReadUInt16s(ImageHeader.PaletteCount);
                }
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                writer.SetByteOrder(true);
                writer.WriteStruct(ImageHeader);
                writer.Write(ImageData);
                if (((GamecubeSwizzle)Platform).PaletteData.Length > 0) {
                    writer.Write(((GamecubeSwizzle)Platform).PaletteData);
                }
            }
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
        {
            return ImageData;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            throw new NotImplementedException();
        }
    }
}
