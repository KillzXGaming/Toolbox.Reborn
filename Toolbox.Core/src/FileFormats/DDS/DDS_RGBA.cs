using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class DDS_RGBA
    {
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

        public static TexFormat GetUncompressedType(DDS dds, byte[] Components, bool IsRGB, bool HasAlpha, bool HasLuminance, DDS.DDSPFHeader header)
        {
            uint bpp = header.RgbBitCount;
            uint RedMask = header.RBitMask;
            uint GreenMask = header.GBitMask;
            uint BlueMask = header.BBitMask;
            uint AlphaMask = HasAlpha ? header.ABitMask : 0;

            if (HasLuminance)
            {
                throw new Exception("Luminance not supported!");
            }
            else if (IsRGB)
            {
                if (bpp == 16)
                {
                    if (RedMask == A1R5G5B5_MASKS[0] && GreenMask == A1R5G5B5_MASKS[1] && BlueMask == A1R5G5B5_MASKS[2] && AlphaMask == A1R5G5B5_MASKS[3])
                    {
                        return TexFormat.BGR5A1_UNORM;
                    }
                    else if (RedMask == X1R5G5B5_MASKS[0] && GreenMask == X1R5G5B5_MASKS[1] && BlueMask == X1R5G5B5_MASKS[2] && AlphaMask == X1R5G5B5_MASKS[3])
                    {
                        return TexFormat.RGB565_UNORM;
                    }
                    else if (RedMask == A4R4G4B4_MASKS[0] && GreenMask == A4R4G4B4_MASKS[1] && BlueMask == A4R4G4B4_MASKS[2] && AlphaMask == A4R4G4B4_MASKS[3])
                    {
                        return TexFormat.RGBA4_UNORM;
                    }
                    else if (RedMask == X4R4G4B4_MASKS[0] && GreenMask == X4R4G4B4_MASKS[1] && BlueMask == X4R4G4B4_MASKS[2] && AlphaMask == X4R4G4B4_MASKS[3])
                    {
                        return TexFormat.RGBA4_UNORM;
                    }
                    else if (RedMask == R5G6B5_MASKS[0] && GreenMask == R5G6B5_MASKS[1] && BlueMask == R5G6B5_MASKS[2] && AlphaMask == R5G6B5_MASKS[3])
                    {
                        return TexFormat.RGB565_UNORM;
                    }
                    else
                    {
                        throw new Exception("Unsupported 16 bit image!");
                    }
                }
                else if (bpp == 24)
                {
                    if (RedMask == R8G8B8_MASKS[0] && GreenMask == R8G8B8_MASKS[1] && BlueMask == R8G8B8_MASKS[2] && AlphaMask == R8G8B8_MASKS[3])
                    {
                      //  dds.bdata = ConvertToRgba(this, "RGB8", 3, new byte[4] { 2, 1, 0, 3 });
                        return TexFormat.RGB8_UNORM;
                    }
                    else
                    {
                        throw new Exception("Unsupported 24 bit image!");
                    }
                }
                else if (bpp == 32)
                {
                    if (RedMask == A8B8G8R8_MASKS[0] && GreenMask == A8B8G8R8_MASKS[1] && BlueMask == A8B8G8R8_MASKS[2] && AlphaMask == A8B8G8R8_MASKS[3])
                    {
                        return TexFormat.RGBA8_UNORM;
                    }
                    else if (RedMask == X8B8G8R8_MASKS[0] && GreenMask == X8B8G8R8_MASKS[1] && BlueMask == X8B8G8R8_MASKS[2] && AlphaMask == X8B8G8R8_MASKS[3])
                    {
                        //dds.bdata = ConvertToRgba(this, "RGB8X", 4, new byte[4] { 2, 1, 0, 3 });
                        return TexFormat.BGRX8_UNORM;
                    }
                    else if (RedMask == A8R8G8B8_MASKS[0] && GreenMask == A8R8G8B8_MASKS[1] && BlueMask == A8R8G8B8_MASKS[2] && AlphaMask == A8R8G8B8_MASKS[3])
                    {
                        return TexFormat.BGRA8_UNORM;
                    }
                    else if (RedMask == X8R8G8B8_MASKS[0] && GreenMask == X8R8G8B8_MASKS[1] && BlueMask == X8R8G8B8_MASKS[2] && AlphaMask == X8R8G8B8_MASKS[3])
                    {
                     //   dds.bdata = ConvertToRgba(this, "RGB8X", 4, new byte[4] { 0, 1, 2, 3 });
                        return TexFormat.BGRX8_UNORM;
                    }
                    else
                    {
                        throw new Exception("Unsupported 32 bit image!");
                    }
                }
            }
            else
            {
                throw new Exception("Unknown type!");
            }
            return TexFormat.UNKNOWN;
        }
    }
}
