using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.WiiU;
using System.Linq;

namespace Toolbox.Core.Imaging
{
    public class WiiUSwizzle : IPlatformSwizzle
    {
        public TexFormat OutputFormat { get; set; } = TexFormat.RGBA8_UNORM;

        public GX2.GX2AAMode AAMode { get; set; }
        public GX2.GX2TileMode TileMode { get; set; }
        public GX2.GX2RResourceFlags ResourceFlags { get; set; }
        public GX2.GX2SurfaceDimension SurfaceDimension { get; set; }
        public GX2.GX2SurfaceUse SurfaceUse { get; set; }
        public GX2.GX2SurfaceFormat Format { get; set; }

        public uint Pitch { get; set; }
        public uint Alignment { get; set; }
        public uint Swizzle { get; set; }

        public uint[] MipOffsets { get; set; }

        public byte[] MipData { get; set; }

        public override string ToString() {
            return OutputFormat.ToString();
        }

        public WiiUSwizzle(GX2.GX2SurfaceFormat format)
        {
            Format = format;
            OutputFormat = FormatList[format];

            AAMode = GX2.GX2AAMode.GX2_AA_MODE_1X;
            TileMode = GX2.GX2TileMode.MODE_2D_TILED_THIN1;
            ResourceFlags = GX2.GX2RResourceFlags.GX2R_BIND_TEXTURE;
            SurfaceDimension = GX2.GX2SurfaceDimension.DIM_2D;
            SurfaceUse = GX2.GX2SurfaceUse.USE_COLOR_BUFFER;
            Alignment = 0;
            Pitch = 0;
        }

        public WiiUSwizzle(TexFormat format)
        {
            Format = FormatList.FirstOrDefault(x => x.Value == format).Key;
            OutputFormat = format;

            AAMode = GX2.GX2AAMode.GX2_AA_MODE_1X;
            TileMode = GX2.GX2TileMode.MODE_2D_TILED_THIN1;
            ResourceFlags = GX2.GX2RResourceFlags.GX2R_BIND_TEXTURE;
            SurfaceDimension = GX2.GX2SurfaceDimension.DIM_2D;
            SurfaceUse = GX2.GX2SurfaceUse.USE_COLOR_BUFFER;
            Alignment = 0;
            Pitch = 0;
        }

        public byte[] DecodeImage(STGenericTexture texture, byte[] data, uint width, uint height, int array, int mip) {

            uint bpp = TextureFormatHelper.GetBytesPerPixel(OutputFormat);

            GX2.GX2Surface surf = new GX2.GX2Surface();
            surf.bpp = bpp;
            surf.height = texture.Height;
            surf.width = texture.Width;
            surf.depth = texture.ArrayCount;
            surf.alignment = Alignment;
            surf.aa = (uint)AAMode;
            surf.dim = (uint)SurfaceDimension;
            surf.format = (uint)Format;
            surf.use = (uint)SurfaceUse;
            surf.pitch = Pitch;
            surf.data = data;
            surf.mipData = MipData != null ? MipData : data;
            surf.mipOffset = MipOffsets != null ? MipOffsets : new uint[0];
            surf.numMips = texture.MipCount;
            surf.numArray = texture.ArrayCount;
            surf.tileMode = (uint)TileMode;
            surf.swizzle = Swizzle;

            Console.WriteLine("WII U DECODE");

            return GX2.Decode(surf, array, mip);
        }

        public byte[] EncodeImage(STGenericTexture texture, byte[] data, uint width, uint height, int array, int mip) {
            //Swizzle and create surface
            var NewSurface = GX2.CreateGx2Texture(data, "",
                (uint)TileMode,
                (uint)AAMode,
                (uint)width,
                (uint)height,
                (uint)1,
                (uint)Format,
                (uint)0,
                (uint)SurfaceDimension,
                (uint)texture.MipCount
                );

            MipData = NewSurface.mipData;
            return NewSurface.data;
        }

        static Dictionary<GX2.GX2SurfaceFormat, TexFormat> FormatList = new Dictionary<GX2.GX2SurfaceFormat, TexFormat>()
        {
            { GX2.GX2SurfaceFormat.TC_R8_UNORM, TexFormat.R8_UNORM },
            { GX2.GX2SurfaceFormat.TC_R8_G8_UNORM, TexFormat.RG8_UNORM },
            { GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNORM, TexFormat.RGBA8_UNORM  },
            { GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB, TexFormat.RGBA8_SRGB  },

            { GX2.GX2SurfaceFormat.TC_R4_G4_B4_A4_UNORM, TexFormat.RGBA4_UNORM },
            { GX2.GX2SurfaceFormat.T_R4_G4_UNORM, TexFormat.RG4_UNORM},
            { GX2.GX2SurfaceFormat.TCS_R5_G6_B5_UNORM,TexFormat.RGB565_UNORM },
            { GX2.GX2SurfaceFormat.TC_R5_G5_B5_A1_UNORM, TexFormat.RGB5A1_UNORM },
            { GX2.GX2SurfaceFormat.TC_A1_B5_G5_R5_UNORM,  TexFormat.BGR5A1_UNORM  },

            { GX2.GX2SurfaceFormat.TCD_R16_UNORM,  TexFormat.R16_UNORM },
            { GX2.GX2SurfaceFormat.TC_R16_G16_UNORM, TexFormat.RG16_UNORM  },
           // { GX2.GX2SurfaceFormat.TC_R16_G16_B16_A16_UNORM, new RGBA(16, 16, 16, 16) },

            { GX2.GX2SurfaceFormat.T_BC1_UNORM,  TexFormat.BC1_UNORM},
            { GX2.GX2SurfaceFormat.T_BC1_SRGB, TexFormat.BC1_SRGB },
            { GX2.GX2SurfaceFormat.T_BC2_UNORM, TexFormat.BC2_UNORM },
            { GX2.GX2SurfaceFormat.T_BC2_SRGB, TexFormat.BC2_SRGB },
            { GX2.GX2SurfaceFormat.T_BC3_UNORM, TexFormat.BC3_UNORM },
            { GX2.GX2SurfaceFormat.T_BC3_SRGB,  TexFormat.BC3_SRGB },
            { GX2.GX2SurfaceFormat.T_BC4_UNORM, TexFormat.BC4_UNORM },
            { GX2.GX2SurfaceFormat.T_BC4_SNORM,TexFormat.BC4_SNORM },
            { GX2.GX2SurfaceFormat.T_BC5_UNORM, TexFormat.BC5_UNORM },
            { GX2.GX2SurfaceFormat.T_BC5_SNORM, TexFormat.BC5_SNORM },
        };
    }
}
