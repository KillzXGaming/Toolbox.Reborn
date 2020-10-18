using System;
using System.Collections.Generic;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using System.IO;

namespace CTRLibrary
{
    public class CTPK : IFileFormat, ITextureContainer, IPropertyDisplay
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "CTPK" };
        public string[] Extension { get; set; } = new string[] { "*.ctpk" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "CTPK");
            }
        }

        public bool DisplayIcons => true;

        public object PropertyDisplay => Header;

        public List<STGenericTexture> Textures = new List<STGenericTexture>();
        public IEnumerable<STGenericTexture> TextureList => Textures;

        private CTPK_Parser Header { get; set; }

        public void Load(Stream stream)
        {
            Header = new CTPK_Parser(stream);
            foreach (var tex in Header.Textures)
                Textures.Add(new Texture(tex) { Name = $"Textures{Textures.Count}" });
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public class Texture : STGenericTexture
        {
            private CTPK_Parser.TextureEntry TextureHeader;

            public Texture(CTPK_Parser.TextureEntry texture)
            {
                TextureHeader = texture;

                Width = texture.Width;
                Height = texture.Height;
                MipCount = texture.MipCount;
                ArrayCount = 1;
                this.DisplayProperties = texture;
                Platform = new CTRSwizzle(texture.PicaFormat);
            }

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {
                return TextureHeader.ImageData;
            }

            public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
