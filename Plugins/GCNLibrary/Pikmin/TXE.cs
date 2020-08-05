using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using System.Runtime.InteropServices;

namespace GCNLibrary
{
    public class TXE : STGenericTexture, IFileFormat
    {
        public bool CanSave { get; set; } = true;
        public string[] Description { get; set; } = new string[] { "TXE" };
        public string[] Extension { get; set; } = new string[] { "*.txe" };
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".txe";
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public ushort Width;
            public ushort Height;
            public ushort Unknown;
            public ushort ImageFormat;
            public uint ImageSize;
        }

        private Header ImageHeader;
        private byte[] ImageData;

        public TXE() { FileInfo = new File_Info(); }

        public void Load(Stream stream)
        {
            Name = FileInfo.FileName;
            CanEdit = true;

            using (var reader = new FileReader(stream)) {
                Read(reader);
            }
        }

        public void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            ImageHeader = reader.ReadStruct<Header>();
            reader.Align(32);

            Width = ImageHeader.Width;
            Height = ImageHeader.Height;
            MipCount = 1;

            Console.WriteLine($"Width {Width} Height {Height}");

            var format = FormatsTXE[ImageHeader.ImageFormat];
            ImageHeader.ImageSize = (uint)Decode_Gamecube.GetDataSize(format, (int)Width, (int)Height);

            ImageData = reader.ReadBytes((int)ImageHeader.ImageSize);
            Platform = new GamecubeSwizzle(format);
        }

        public static Dictionary<ushort, Decode_Gamecube.TextureFormats> FormatsTXE = new Dictionary<ushort, Decode_Gamecube.TextureFormats>()
        {
            [0] = Decode_Gamecube.TextureFormats.RGB565,
            [1] = Decode_Gamecube.TextureFormats.CMPR,
            [2] = Decode_Gamecube.TextureFormats.RGB5A3,
            [3] = Decode_Gamecube.TextureFormats.I4,
            [4] = Decode_Gamecube.TextureFormats.I8,
            [5] = Decode_Gamecube.TextureFormats.IA4,
            [6] = Decode_Gamecube.TextureFormats.IA8,
            [7] = Decode_Gamecube.TextureFormats.RGBA32,
        };

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                writer.SetByteOrder(true);
                writer.WriteStruct(ImageHeader);
                writer.Align(32);
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
