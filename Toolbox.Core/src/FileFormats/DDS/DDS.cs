using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;
using Toolbox.Core.Imaging;
using Toolbox.Core.DXGI;

namespace Toolbox.Core
{
    public class DDS : STGenericTexture, IFileFormat, IExportableTexture
    {
        public bool IdentifyExport(string ext) { return ext == ".dds" || ext == ".dds2"; }

        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "DDS" };
        public string[] Extension { get; set; } = new string[] { "*.dds" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "DDS ");
            }
        }

        #region Constants

        public const uint FOURCC_DXT1 = 0x31545844;
        public const uint FOURCC_DXT2 = 0x32545844;
        public const uint FOURCC_DXT3 = 0x33545844;
        public const uint FOURCC_DXT4 = 0x34545844;
        public const uint FOURCC_DXT5 = 0x35545844;
        public const uint FOURCC_ATI1 = 0x31495441;
        public const uint FOURCC_BC4U = 0x55344342;
        public const uint FOURCC_BC4S = 0x53344342;
        public const uint FOURCC_BC5U = 0x55354342;
        public const uint FOURCC_BC5S = 0x53354342;
        public const uint FOURCC_DX10 = 0x30315844;

        public const uint FOURCC_ATI2 = 0x32495441;
        public const uint FOURCC_RXGB = 0x42475852;

        // RGBA Masks
        private static int[] A1R5G5B5_MASKS = { 0x7C00, 0x03E0, 0x001F, 0x8000 };
        private static int[] X1R5G5B5_MASKS = { 0x7C00, 0x03E0, 0x001F, 0x0000 };
        private static int[] A4R4G4B4_MASKS = { 0x0F00, 0x00F0, 0x000F, 0xF000 };
        private static int[] X4R4G4B4_MASKS = { 0x0F00, 0x00F0, 0x000F, 0x0000 };
        private static int[] R5G6B5_MASKS = { 0xF800, 0x07E0, 0x001F, 0x0000 };
        private static int[] R8G8B8_MASKS = { 0xFF0000, 0x00FF00, 0x0000FF, 0x000000 };
        private static uint[] A8B8G8R8_MASKS = { 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000 };
        private static int[] X8B8G8R8_MASKS = { 0x000000FF, 0x0000FF00, 0x00FF0000, 0x00000000 };
        private static uint[] A8R8G8B8_MASKS = { 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000 };
        private static int[] X8R8G8B8_MASKS = { 0x00FF0000, 0x0000FF00, 0x000000FF, 0x00000000 };

        private static int[] L8_MASKS = { 0x000000FF, 0x0000, };
        private static int[] A8L8_MASKS = { 0x000000FF, 0x0F00, };

        #endregion

        #region enums

        public enum CubemapFace
        {
            PosX,
            NegX,
            PosY,
            NegY,
            PosZ,
            NegZ
        }

        [Flags]
        public enum DDSD : uint
        {
            CAPS = 0x00000001,
            HEIGHT = 0x00000002,
            WIDTH = 0x00000004,
            PITCH = 0x00000008,
            PIXELFORMAT = 0x00001000,
            MIPMAPCOUNT = 0x00020000,
            LINEARSIZE = 0x00080000,
            DEPTH = 0x00800000
        }
        [Flags]
        public enum DDPF : uint
        {
            ALPHAPIXELS = 0x00000001,
            ALPHA = 0x00000002,
            FOURCC = 0x00000004,
            RGB = 0x00000040,
            YUV = 0x00000200,
            LUMINANCE = 0x00020000,
        }
        [Flags]
        public enum DDSCAPS : uint
        {
            COMPLEX = 0x00000008,
            TEXTURE = 0x00001000,
            MIPMAP = 0x00400000,
        }
        [Flags]
        public enum DDSCAPS2 : uint
        {
            CUBEMAP = 0x00000200,
            CUBEMAP_POSITIVEX = 0x00000400 | CUBEMAP,
            CUBEMAP_NEGATIVEX = 0x00000800 | CUBEMAP,
            CUBEMAP_POSITIVEY = 0x00001000 | CUBEMAP,
            CUBEMAP_NEGATIVEY = 0x00002000 | CUBEMAP,
            CUBEMAP_POSITIVEZ = 0x00004000 | CUBEMAP,
            CUBEMAP_NEGATIVEZ = 0x00008000 | CUBEMAP,
            CUBEMAP_ALLFACES = (CUBEMAP_POSITIVEX | CUBEMAP_NEGATIVEX |
                                  CUBEMAP_POSITIVEY | CUBEMAP_NEGATIVEY |
                                  CUBEMAP_POSITIVEZ | CUBEMAP_NEGATIVEZ),
            VOLUME = 0x00200000
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic Magic = "DDS ";
            public uint Size = 0x7C;
            public uint Flags;
            public uint Height;
            public uint Width;
            public uint PitchOrLinearSize;
            public uint Depth;
            public uint MipCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public uint[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DDSPFHeader
        {
            public uint Size = 0x20;
            public uint Flags;
            public uint FourCC;
            public uint RgbBitCount;
            public uint RBitMask;
            public uint GBitMask;
            public uint BBitMask;
            public uint ABitMask;
            public uint Caps1;
            public uint Caps2;
            public uint Caps3;
            public uint Caps4;
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DX10Header
        {
            public uint DxgiFormat;
            public uint ResourceDim;
            public uint MiscFlags1;
            public uint ArrayCount;
            public uint MiscFlags2;
        }

        public string FileName { get; set; }

        public Header MainHeader;
        public DDSPFHeader PfHeader;
        public DX10Header Dx10Header;

        public byte[] ImageData;

        public bool IsDX10 => PfHeader.FourCC == FOURCC_DX10;

        public bool IsCubeMap
        {
            get { return PfHeader.Caps2 == (uint)DDSCAPS2.CUBEMAP_ALLFACES; }
            set
            {
                if (value)
                    PfHeader.Caps2 = (uint)DDSCAPS2.CUBEMAP_ALLFACES;
                else
                    PfHeader.Caps2 = 0;
            }
        }

        public DDS()
        {
            MainHeader = new Header();
            PfHeader = new DDSPFHeader();
            Dx10Header = new DX10Header();
        }

        public DDS(uint width, uint height, uint depth, TexFormat format, List<byte[]> imageData) : base()
        {
            MainHeader.Width = width;
            MainHeader.Height = height;
            MainHeader.Depth = depth;
        }

        public void Export(STGenericTexture texture, TextureExportSettings settings, string filePath)
        {
            List<Surface> surfaces = texture.GetSurfaces(settings.ArrayLevel, settings.ExportArrays);

            DDS dds = new DDS();
            dds.MainHeader = new DDS.Header();
            dds.MainHeader.Width = texture.Width;
            dds.MainHeader.Height = texture.Height;
            dds.MainHeader.Depth = texture.Depth;
            dds.MainHeader.MipCount = (uint)texture.MipCount;
            dds.MainHeader.PitchOrLinearSize = (uint)surfaces[0].mipmaps[0].Length;

            if (surfaces.Count > 1) //Use DX10 format for array surfaces as it can do custom amounts
                dds.SetFlags(texture.Platform.OutputFormat, true, texture.IsCubemap);
            else
                dds.SetFlags(texture.Platform.OutputFormat, false, texture.IsCubemap);

            if (dds.IsDX10)
            {
                if (dds.Dx10Header == null)
                    dds.Dx10Header = new DDS.DX10Header();
                dds.Dx10Header.ResourceDim = 3;
                if (texture.IsCubemap)
                    dds.Dx10Header.ArrayCount = (uint)(texture.ArrayCount / 6);
                else
                    dds.Dx10Header.ArrayCount = (uint)texture.ArrayCount;
            }
            dds.Save(filePath, surfaces);
        }

        public DDS(string fileName) { Load(fileName); }

        public void ToGenericFormat()
        {
            TexFormat format = TexFormat.RGBA8_UNORM;

            if (IsDX10)
                format = (TexFormat)Dx10Header.DxgiFormat;
            else
            {
                switch (PfHeader.FourCC)
                {
                    case FOURCC_DXT1:
                        format = TexFormat.BC1_UNORM;
                        break;
                    case FOURCC_DXT2:
                    case FOURCC_DXT3:
                        format = TexFormat.BC2_UNORM;
                        break;
                    case FOURCC_DXT4:
                    case FOURCC_DXT5:
                        format = TexFormat.BC3_UNORM;
                        break;
                    case FOURCC_ATI1:
                    case FOURCC_BC4U:
                        format = TexFormat.BC4_UNORM;
                        break;
                    case FOURCC_ATI2:
                    case FOURCC_BC5U:
                        format = TexFormat.BC5_UNORM;
                        break;
                    case FOURCC_BC5S:
                        format = TexFormat.BC5_SNORM;
                        break;
                    case FOURCC_RXGB:
                        format = TexFormat.RGBA8_UNORM;
                        break;
                    default:
                        format = TexFormat.RGBA8_UNORM;
                        break;
                }
            }
            Platform.OutputFormat = format;
        }

        public void SetFlags(TexFormat format, bool isDX10, bool isCubemap)
        {
            var dxgiFormat = (DXGI_FORMAT)format;

            MainHeader.Flags = (uint)(DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT | DDSD.LINEARSIZE);
            PfHeader.Caps1 = (uint)DDSCAPS.TEXTURE;
            if (MainHeader.MipCount > 1)
                PfHeader.Caps1 |= (uint)(DDSCAPS.COMPLEX | DDSCAPS.MIPMAP);

            if (isCubemap)
            {
                PfHeader.Caps2 |= (uint)(DDSCAPS2.CUBEMAP | DDSCAPS2.CUBEMAP_POSITIVEX | DDSCAPS2.CUBEMAP_NEGATIVEX |
                                      DDSCAPS2.CUBEMAP_POSITIVEY | DDSCAPS2.CUBEMAP_NEGATIVEY |
                                      DDSCAPS2.CUBEMAP_POSITIVEZ | DDSCAPS2.CUBEMAP_NEGATIVEZ);
            }
            if (isDX10)
            {
                PfHeader.Flags = (uint)DDPF.FOURCC;
                PfHeader.FourCC = FOURCC_DX10;

                Dx10Header.DxgiFormat = (uint)dxgiFormat;
                if (isCubemap)
                {
                    Dx10Header.ArrayCount = (ArrayCount / 6);
                    Dx10Header.MiscFlags1 = 0x4;
                }
                return;
            }

            var masks = DDS_RGBA.GetMasks(format);
            PfHeader.RBitMask = masks[0];
            PfHeader.GBitMask = masks[1];
            PfHeader.BBitMask = masks[2];
            PfHeader.ABitMask = masks[3];

            PfHeader.RgbBitCount = 0x8 * TextureFormatHelper.GetBytesPerPixel(format);
            PfHeader.Flags = (uint)(DDPF.RGB | DDPF.ALPHAPIXELS);

            switch (format)
            {
                case TexFormat.RGBA8_UNORM:
                case TexFormat.RGB8_SRGB:
                    break;
                case TexFormat.RGB565_UNORM:
                    PfHeader.Flags = (uint)(DDPF.RGB | DDPF.ALPHAPIXELS);
                    break;
                case TexFormat.BC1_SRGB:
                case TexFormat.BC1_UNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_DXT1;
                    break;
                case TexFormat.BC2_SRGB:
                case TexFormat.BC2_UNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_DXT3;
                    break;
                case TexFormat.BC3_SRGB:
                case TexFormat.BC3_UNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_DXT5;
                    break;
                case TexFormat.BC4_UNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_BC4U;
                    break;
                case TexFormat.BC4_SNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_BC4S;
                    break;
                case TexFormat.BC5_UNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_BC5U;
                    break;
                case TexFormat.BC5_SNORM:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_BC5S;
                    break;
                case TexFormat.BC6H_SF16:
                case TexFormat.BC6H_UF16:
                case TexFormat.BC7_UNORM:
                case TexFormat.BC7_SRGB:
                    PfHeader.Flags = (uint)DDPF.FOURCC;
                    PfHeader.FourCC = FOURCC_DX10;
                    if (Dx10Header == null)
                        Dx10Header = new DX10Header();

                    Dx10Header.DxgiFormat = (uint)dxgiFormat;
                    break;
            }
        }

        public void Load(string fileName)
        {
            FileName = fileName;
            FileInfo = new File_Info() { FilePath = fileName };
            Load(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public void Save(string fileName, List<Surface> surfaces)
        {
            List<byte[]> dataList = new List<byte[]>();
            foreach (var surface in surfaces)
                dataList.Add(ByteUtils.CombineArray(surface.mipmaps.ToArray()));
            ImageData = ByteUtils.CombineArray(dataList.ToArray());
            dataList.Clear();

            Save(fileName);
        }

        public void Save(string fileName)
        {
            Save(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
        }

        public void Load(Stream stream)
        {
            Name = FileInfo.FileName;
            using (var reader = new FileReader(stream))
            {
                MainHeader = reader.ReadStruct<Header>();
                PfHeader = reader.ReadStruct<DDSPFHeader>();
                if (IsDX10)
                    Dx10Header = reader.ReadStruct<DX10Header>();

                byte[] Components = new byte[4] { 0, 1, 2, 3 };

                bool Compressed = false;
                bool HasLuminance = false;
                bool HasAlpha = false;
                bool IsRGB = false;

                switch (PfHeader.Flags)
                {
                    case 4:
                        Compressed = true;
                        break;
                    case 2:
                    case (uint)DDPF.LUMINANCE:
                        HasLuminance = true;
                        break;
                    case (uint)DDPF.RGB:
                        IsRGB = true;
                        break;
                    case 0x41:
                        IsRGB = true;
                        HasAlpha = true;
                        break;
                }

                ToGenericFormat();

                if (!IsDX10 && !this.IsBCNCompressed()) {
                    Platform.OutputFormat = DDS_RGBA.GetUncompressedType(this, Components, IsRGB, HasAlpha, HasLuminance, PfHeader);
                }

                Depth = 1;
                Width = MainHeader.Width;
                Height = MainHeader.Height;
                MipCount = MainHeader.MipCount == 0 ? 1 : MainHeader.MipCount;
                ArrayCount = IsCubeMap ? (uint)6 : 1;
            }
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {

            int Dx10Size = IsDX10 ? 20 : 0;
            using (FileReader reader = new FileReader(FileInfo.FilePath)) {
                reader.TemporarySeek((int)(MainHeader.Size + Dx10Size + 4), SeekOrigin.Begin);
                return reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }

                var surfaces = DDSHelper.GetArrayFaces(this, ArrayCount, DepthLevel);
            return surfaces[ArrayLevel].mipmaps[MipLevel];
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            Width = width;
            Height = height;
            ImageData = ByteUtils.CombineArray(imageData.ToArray());
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.WriteStruct(MainHeader);
                writer.WriteStruct(PfHeader);
                if (IsDX10)
                    writer.WriteStruct(Dx10Header);

                writer.Write(ImageData);
            }
        }

        public List<byte[]> GetImageData() {
            return new List<byte[]>() { ImageData };
        }
    }
}