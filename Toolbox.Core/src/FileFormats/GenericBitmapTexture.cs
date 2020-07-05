using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Toolbox.Core.Imaging;

namespace Toolbox.Core
{
    public class GenericBitmapTexture : STGenericTexture, IFileFormat, IImportableTexture, IExportableTexture
    {
        public bool IdentifyImport(string ext)
        {
            return
                ext == ".png" ||
                ext == "jpg" ||
                ext == ".gif" ||
                ext == ".tiff";
        }

        public bool IdentifyExport(string ext) {
            return
                ext == ".png" ||
                ext == "jpg" ||
                ext == ".gif" ||
                ext == ".tiff"; 
        }

        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "PNG" };
        public string[] Extension { get; set; } = new string[] { "*.png", "*.jpg", "*.tiff", "*.bmp", "*.gif" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, System.IO.Stream stream) {
            return fileInfo.Extension == ".png";
        }

        private byte[] ImageData;

        public GenericBitmapTexture(byte[] FileData, int width, int height)
        {
            Name = $"bitmap_{width}x{height}";
            Platform.OutputFormat = TexFormat.RGBA8_UNORM;
            Width = (uint)width;
            Height = (uint)height;

            ImageData = FileData;
        }

        public GenericBitmapTexture() { }

        public GenericBitmapTexture(Bitmap Image) {
            LoadBitmap(Image);
        }

        public GenericBitmapTexture(string filePath) {
            LoadBitmap(new Bitmap(filePath), true);
        }

        public GenericBitmapTexture(System.IO.Stream stream) {
            LoadBitmap(new Bitmap(stream), false);
        }

        public GenericBitmapTexture(byte[] imageData)
        {
            Bitmap bmp;
            using (var ms = new System.IO.MemoryStream(imageData))
            {
                bmp = new Bitmap(ms);
                LoadBitmap(bmp);
            }
        }


        public void Load(System.IO.Stream stream) {
            LoadBitmap(new Bitmap(stream));
        }

        private void LoadBitmap(Bitmap bitmap, bool swapBlueRed = true)
        {
            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                bitmap = BitmapExtension.ToArgb32(bitmap);

            Name = FileInfo != null ? FileInfo.FileName : "bitmap";
            ImageData = BitmapExtension.ImageToByte(bitmap);
            if (swapBlueRed)
                ImageData = ImageUtility.ConvertBgraToRgba(ImageData);
            Width = (uint)bitmap.Width;
            Height = (uint)bitmap.Height;
            Platform.OutputFormat = TexFormat.RGBA8_UNORM;
            MipCount = 1;
            CanEdit = true;
        }

        public void Unload()
        {

        }
            
        public void Save(System.IO.Stream stream)
        {
            ImageFormat format = ImageFormat.Png;
            if (FileInfo.Extension == ".jpg")
                format = ImageFormat.Jpeg;
            if (FileInfo.Extension == ".gif")
                format = ImageFormat.Gif;
            if (FileInfo.Extension == ".bmp")
                format = ImageFormat.Bmp;
            if (FileInfo.Extension == ".tiff")
                format = ImageFormat.Tiff;

            var bitmap = BitmapExtension.CreateBitmap(ImageData, (int)Width, (int)Height);
            bitmap.Save(stream, format);
        }

        public STGenericTexture Import(string fileName) {
            return new GenericBitmapTexture(System.IO.File.OpenRead(fileName));
        }

        public void Export(STGenericTexture texture, TextureExportSettings settings, string filePath)
        {
            int numSurfaces = settings.ExportArrays ? (int)texture.ArrayCount : 1;
            if (numSurfaces == 0) numSurfaces = 1;
            for (int i = 0; i < numSurfaces; i++)
            {
                var image = texture.GetBitmap(i);
                image.Save(numSurfaces == 1 ? filePath : $"{filePath}_{i}");
            }
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
