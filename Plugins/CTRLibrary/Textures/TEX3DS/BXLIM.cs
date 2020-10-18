using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;

namespace PluginSample
{
    public class TEX : STGenericTexture, IFileFormat
    {
        public bool CanSave { get; set; } = false;
        public string[] Description { get; set; } = new string[] { "3DS Texture" };
        public string[] Extension { get; set; } = new string[] { "*.tex" };
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return (fileInfo.Extension == ".tex");
        }

        public class TEXHeader
        {
            public uint Width;
            public uint Height;
            public byte Format;
            public byte MipCount;
            public ushort Padding;
        }

        public byte[] ImageData;

        TEXHeader Header;

        public void Load(Stream stream)
        {

        }

        public void Save(Stream stream)
        {
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {
            return ImageData;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            throw new NotImplementedException();
        }
    }
}
