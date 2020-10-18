using System;
using System.Collections.Generic;
using Toolbox.Core;
using Toolbox.Core.IO;
using System.IO;

namespace CTRLibrary.Grezzo
{
    public class CTXB : IFileFormat, ITextureContainer
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "CTXB" };
        public string[] Extension { get; set; } = new string[] { "*.ctxb" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "ctxb");
            }
        }

        public bool DisplayIcons => true;

        public IEnumerable<STGenericTexture> TextureList => Header.Textures;
        public CTXB_Parser Header;

        public void Load(Stream stream) {
            Header = new CTXB_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Write(new FileWriter(stream));
        }

        public class TextureWrapper : STGenericTexture
        {
            public CTXB_Parser.Texture TextureInfo;

            public byte[] ImageData
            {
                get { return TextureInfo.ImageData; }
                set { TextureInfo.ImageData = value; }
            }

            public TextureWrapper(CTXB_Parser.Texture textureInfo)
            {
                TextureInfo = textureInfo;
                Width = textureInfo.Width;
                Height = textureInfo.Height;
                MipCount = textureInfo.MaxLevel;
                Name = textureInfo.Name;
                Platform = new Toolbox.Core.Imaging.CTRSwizzle(textureInfo.PicaFormat);
            }

            public void CreateNew(string fileName)
            {
                TextureInfo = new CTXB_Parser.Texture();
            }

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
            {
                return ImageData;
            }

            public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
            {
                
            }
        }
    }
}
