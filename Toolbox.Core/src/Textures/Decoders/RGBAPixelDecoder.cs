using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core.Imaging
{
    public class RGBAPixelDecoder : ITextureDecoder
    {
        /// Convert a 4-bit color component to 8 bit
        private static byte Convert1To8(byte value) {
            return (byte)(value * 255);
        }

        /// Convert a 4-bit color component to 8 bit
        private static byte Convert4To8(byte value) {
            return (byte)((value << 4) | value);
        }

        /// Convert a 5-bit color component to 8 bit
        private static byte Convert5To8(byte value) {
            return (byte)((value << 3) | value >> 2);
        }

        /// Convert a 6-bit color component to 8 bit
        private static byte Convert6To8(byte value) {
            return (byte)((value << 2) | value >> 4);
        }

        /// Convert a 6-bit color component to 8 bit
        private static byte Convert7To8(byte value) {
            return (byte)(value << 7);
        }

        private bool IsSupported(TexFormat format)
        {
            switch (format)
            {
                case TexFormat.L8:
                case TexFormat.LA4:
                case TexFormat.RGB8_UNORM:
                case TexFormat.RGB8_SRGB:
                case TexFormat.RGBA8_UNORM:
                case TexFormat.RGBA8_SRGB:
                case TexFormat.BGRA8_UNORM:
                case TexFormat.BGRA8_SRGB:
                case TexFormat.BGR565_UNORM:
                case TexFormat.RGB565_UNORM:
                case TexFormat.RGB5_UNORM:
                case TexFormat.RGBA4_UNORM:
                case TexFormat.BGRA4_UNORM:
                case TexFormat.RGB5A1_UNORM:
                case TexFormat.GRGB8_UNORM:
                case TexFormat.RG8_UNORM:
                case TexFormat.RG4_UNORM:
                case TexFormat.R8_UNORM:
                    return true;
            }
            return false;
        }

        private static byte[] GetComponentsFromPixel(TexFormat format, int pixel, byte[] comp)
        {
            switch (format)
            {
                case TexFormat.L8:
                    comp[0] = (byte)(pixel & 0xFF);
                    break;
                case TexFormat.LA8:
                    comp[0] = (byte)(pixel & 0xFF);
                    comp[1] = (byte)((pixel & 0xFF00) >> 8);
                    break;
                case TexFormat.LA4:
                    comp[0] = (byte)((pixel & 0xF) * 17);
                    comp[1] = (byte)(((pixel & 0xF0) >> 4) * 17);
                    break;
                case TexFormat.BGR565_UNORM:
                    comp[2] = Convert5To8((byte)((pixel >> 11) & 0x1F));
                    comp[1] = Convert6To8((byte)((pixel >> 5) & 0x3F));
                    comp[0] = Convert5To8((byte)(pixel & 0x1F));
                    comp[3] = 255;
                    break;
                case TexFormat.RGB565_UNORM:
                    comp[0] = Convert5To8((byte)((pixel >> 11) & 0x1F));
                    comp[1] = Convert6To8((byte)((pixel >> 5) & 0x3F));
                    comp[2] = Convert5To8((byte)(pixel & 0x1F));
                    comp[3] = 255;
                    break;
                case TexFormat.RGB5_UNORM:
                    {
                        int R = ((pixel >> 0) & 0x1f) << 3;
                        int G = ((pixel >> 5) & 0x1f) << 3;
                        int B = ((pixel >> 10) & 0x1f) << 3;

                        comp[0] = (byte)(R | (R >> 5));
                        comp[1] = (byte)(G | (G >> 5));
                        comp[2] = (byte)(B | (B >> 5));
                    }
                    break;
                case TexFormat.RGBA4_UNORM:
                    {
                        comp[2] = Convert4To8((byte)(pixel & 0xF));
                        comp[1] = Convert4To8((byte)((pixel >> 4) & 0xF));
                        comp[0] = Convert4To8((byte)((pixel >> 8) & 0xF));
                        comp[3] = Convert4To8((byte)((pixel >> 12) & 0xF));
                    }
                    break;
                case TexFormat.BGRA4_UNORM:
                    {
                        comp[3] = Convert4To8((byte)((pixel >> 12) & 0xF));
                        comp[1] = Convert4To8((byte)((pixel >> 8) & 0xF));
                        comp[0] = Convert4To8((byte)((pixel >> 4) & 0xF));
                        comp[2] = Convert4To8((byte)(pixel & 0xF));
                    }
                    break;
                case TexFormat.RG4_UNORM:
                    {
                        float R = ((pixel >> 0) & 0xF) / 15.0f;
                        float G = ((pixel >> 4) & 0xF) / 15.0f;
                        comp[0] = (byte)(R * 255);
                        comp[1] = (byte)(G * 255);
                    }
                    break;
                case TexFormat.RGB5A1_UNORM:
                    {
                        int R = ((pixel >> 0) & 0x1f) << 3;
                        int G = ((pixel >> 5) & 0x1f) << 3;
                        int B = ((pixel >> 10) & 0x1f) << 3;
                        int A = ((pixel & 0x8000) >> 15) * 0xFF;

                        comp[0] = (byte)(R | (R >> 5));
                        comp[1] = (byte)(G | (G >> 5));
                        comp[2] = (byte)(B | (B >> 5));
                        comp[3] = (byte)A;
                    }
                    break;
                case TexFormat.GRGB8_UNORM:
                    comp[0] = (byte)((pixel & 0xFF00) >> 8);
                    comp[1] = (byte)(pixel & 0xFF);
                    comp[2] = (byte)((pixel & 0xFF00) >> 8);
                    comp[3] = (byte)((pixel & 0xFF0000) >> 16);
                    break;
                case TexFormat.RG8_UNORM:
                    comp[0] = (byte)(pixel & 0xFF);
                    comp[1] = (byte)((pixel & 0xFF00) >> 8);
                    break;
                case TexFormat.R8_UNORM:
                    comp[0] = (byte)(pixel & 0xFF);
                    comp[1] = (byte)(pixel & 0xFF);
                    comp[2] = (byte)(pixel & 0xFF);
                    break;
                case TexFormat.RGB8_UNORM:
                case TexFormat.RGB8_SRGB:
                    comp[2] = (byte)(pixel & 0xFF);
                    comp[1] = (byte)((pixel & 0xFF00) >> 8);
                    comp[0] = (byte)((pixel & 0xFF0000) >> 16);
                    comp[3] = (byte)(0xFF);
                    break;
                case TexFormat.RGBA8_SINT:
                case TexFormat.RGBA8_UINT:
                case TexFormat.RGBA8_UNORM:
                case TexFormat.RGBA8_SRGB:
                    comp[0] = (byte)(pixel & 0xFF);
                    comp[1] = (byte)((pixel & 0xFF00) >> 8);
                    comp[2] = (byte)((pixel & 0xFF0000) >> 16);
                    comp[3] = (byte)((pixel & 0xFF000000) >> 24);
                    break;
            }

            if (format.ToString().StartsWith("BGR")) {
                return new byte[4] { comp[1],comp[0],comp[2],comp[3] };
            }

            return comp;
        }

        //Method from https://github.com/aboood40091/BNTX-Editor/blob/master/formConv.py
        public bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            if (!IsSupported(format)) {
                output = null;
                return false;
            }

            if (format == TexFormat.BGRA8_UNORM || format == TexFormat.BGRA8_SRGB) {
                output = ImageUtility.ConvertBgraToRgba(input);
                return true;
            }

            uint bpp = TextureFormatHelper.GetBytesPerPixel(format);
            int size = width * height * 4;

            bpp = (uint)(input.Length / (width * height));

           output = new byte[size];

            int inPos = 0;
            int outPos = 0;

            byte[] comp = new byte[] { 0, 0, 0, 0xFF, 0, 0xFF };
            byte[] compSel = new byte[4] { 0, 1, 2, 3 };

            if (format == TexFormat.LA8)
            {
                compSel = new byte[4] { 0, 0, 0, 1 };
                bpp = 2;
            }
            else if (format == TexFormat.L8)
                compSel = new byte[4] { 0, 0, 0, 5 };

            for (int Y = 0; Y < height; Y++)
            {
                for (int X = 0; X < width; X++)
                {
                    inPos = (Y * width + X) * (int)bpp;
                    outPos = (Y * width + X) * 4;

                    int pixel = 0;
                    for (int i = 0; i < bpp; i++)
                        pixel |= input[inPos + i] << (8 * i);

                    comp = GetComponentsFromPixel(format, pixel, comp);

                    output[outPos + 3] = comp[compSel[3]];
                    output[outPos + 2] = comp[compSel[2]];
                    output[outPos + 1] = comp[compSel[1]];
                    output[outPos + 0] = comp[compSel[0]];
                }
            }

            return output != null;
        }

        public bool Encode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            output = null;
            return false;
        }
    }
}