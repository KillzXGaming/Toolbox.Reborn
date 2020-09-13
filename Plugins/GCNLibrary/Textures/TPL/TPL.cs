using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using System.Runtime.InteropServices;

namespace GCNLibrary
{
    public class TPL : IFileFormat, ITextureContainer
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "TPL" };
        public string[] Extension { get; set; } = new string[] { "*.tpl" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            using (var reader = new FileReader(stream, true)) {
                reader.SetByteOrder(true);
                return reader.ReadUInt32() == 0x0020AF30 || fileInfo.Extension == ".tpl";
            }
        }

        public bool DisplayIcons => true;

        public List<STGenericTexture> Textures = new List<STGenericTexture>();
        public IEnumerable<STGenericTexture> TextureList => Textures;

        public TPL_Parser Header;

        public void Load(Stream stream) {
            Header = new TPL_Parser(stream);
            foreach (var texture in Header.Images) {
                Textures.Add(new TPLTexture(texture) { Name = $"Texture{Textures.Count}" });
            }
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }
    }

    class TPLTexture : STGenericTexture
    {
        TPL_Parser.ImageEntry Image;

        public TPLTexture(TPL_Parser.ImageEntry imageEntry) {
            Image = imageEntry;
            Width = imageEntry.Header.Width;
            Height = imageEntry.Header.Height;
            Platform = new GamecubeSwizzle(imageEntry.Header.Format);
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {
            return Image.ImageData;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            throw new NotImplementedException();
        }
    }
}
