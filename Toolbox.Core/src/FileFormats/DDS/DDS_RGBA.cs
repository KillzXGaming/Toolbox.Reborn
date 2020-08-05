using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class DDS_RGBA
    {
        // RGBA Masks
        private static uint[] A1R5G5B5_MASKS = { 0x7C00, 0x03E0, 0x001F, 0x8000 };
        private static uint[] X1R5G5B5_MASKS = { 0x7C00, 0x03E0, 0x001F, 0x0000 };
        private static uint[] A4R4G4B4_MASKS = { 0x0F00, 0x00F0, 0x000F, 0xF000 };
        private static uint[] X4R4G4B4_MASKS = { 0x0F00, 0x00F0, 0x000F, 0x0000 };
        private static uint[] R5G6B5_MASKS = { 0xF800, 0x07E0, 0x001F, 0x0000 };
        private static uint[] R8G8B8_MASKS = { 0xFF0000, 0x00FF00, 0x0000FF, 0x000000 };
        private static uint[] A8B8G8R8_MASKS = { 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000 };
        private static uint[] X8B8G8R8_MASKS = { 0x000000FF, 0x0000FF00, 0x00FF0000, 0x00000000 };
        private static uint[] A8R8G8B8_MASKS = { 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000 };
        private static uint[] X8R8G8B8_MASKS = { 0x00FF0000, 0x0000FF00, 0x000000FF, 0x00000000 };

        private static uint[] L8_MASKS = { 0x000000FF, 0x0000, };
        private static uint[] A8L8_MASKS = { 0x000000FF, 0x0F00, };

        public static uint[] GetMasks(TexFormat format)
        {
            switch (format)
            {
                case TexFormat.RGB5A1_UNORM: return A1R5G5B5_MASKS;
                case TexFormat.RGB5_UNORM: return X1R5G5B5_MASKS;
                case TexFormat.RGBA4_UNORM: return A4R4G4B4_MASKS;
                case TexFormat.RGB565_UNORM: return R5G6B5_MASKS;
                case TexFormat.RGB8_UNORM: return R8G8B8_MASKS;
                case TexFormat.RGB8_SRGB: return R8G8B8_MASKS;
                case TexFormat.BGRA8_UNORM: return A8B8G8R8_MASKS;
                case TexFormat.BGRA8_SRGB: return A8B8G8R8_MASKS;
                case TexFormat.BGRX8_UNORM: return X8B8G8R8_MASKS;
                case TexFormat.BGRX8_UNORM_SRGB:return X8B8G8R8_MASKS;
                //  case TexFormat.RGB4_UNORM:
                //    return new int[4] { X4R4G4B4_MASKS[1], X4R4G4B4_MASKS[2], X4R4G4B4_MASKS[3], X4R4G4B4_MASKS[4] };
                default:
                    return A8R8G8B8_MASKS;
            }
        }

        public static TexFormat GetUncompressedType(DDS dds, byte[] Components, bool IsRGB, bool HasAlpha, bool HasLuminance, DDS.DDSPFHeader header)
        {
            uint bpp = header.RgbBitCount;
            uint RedMask = header.RBitMask;
            uint GreenMask = header.GBitMask;
            uint BlueMask = header.BBitMask;
            uint AlphaMask = HasAlpha ? header.ABitMask : 0;

            uint[] maskRGBA = new uint[4] { RedMask, GreenMask, BlueMask, AlphaMask };

            if (HasLuminance)
            {
                throw new Exception("Luminance not supported!");
            }
            else if (IsRGB)
            {
                if (bpp == 16)
                {
                    if (IsMatch(maskRGBA, A1R5G5B5_MASKS)) return TexFormat.BGR5A1_UNORM;
                    else if (IsMatch(maskRGBA, X1R5G5B5_MASKS)) return TexFormat.RGB565_UNORM;
                    else if (IsMatch(maskRGBA, A4R4G4B4_MASKS)) return TexFormat.RGBA4_UNORM;
                    else if (IsMatch(maskRGBA, X4R4G4B4_MASKS)) return TexFormat.RGBA4_UNORM;
                    else if (IsMatch(maskRGBA, R5G6B5_MASKS)) return TexFormat.RGB565_UNORM;
                    else
                    {
                        throw new Exception("Unsupported 16 bit image!");
                    }
                }
                else if (bpp == 24)
                {
                    if (IsMatch(maskRGBA, R8G8B8_MASKS))
                    {
                        return TexFormat.RGB8_UNORM;
                    }
                    else
                    {
                        throw new Exception("Unsupported 24 bit image!");
                    }
                }
                else if (bpp == 32)
                {
                    if (IsMatch(maskRGBA, A8B8G8R8_MASKS)) return TexFormat.RGBA8_UNORM;
                    else if(IsMatch(maskRGBA, X8B8G8R8_MASKS)) return TexFormat.BGRX8_UNORM;
                    else if (IsMatch(maskRGBA, A8R8G8B8_MASKS)) return TexFormat.BGRA8_UNORM;
                    else if (IsMatch(maskRGBA, X8R8G8B8_MASKS)) return TexFormat.BGRX8_UNORM;
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

        static bool IsMatch(uint[] rgba, uint[] masks)
        {
            return (rgba[0] == masks[0] &&
                    rgba[1] == masks[1] &&
                    rgba[2] == masks[2] &&
                    rgba[3] == masks[3]);
        }
    }
}
