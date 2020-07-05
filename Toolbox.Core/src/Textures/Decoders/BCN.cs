using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.TextureDecoding;

namespace Toolbox.Core.Imaging
{
    public class BCN : ITextureDecoder
    {
        public bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
           output = null;
            switch (format)
            {
                case TexFormat.BC1_UNORM:
                    output = DXT.DecompressBC1(input, width, height, false);
                    break;
                case TexFormat.BC1_SRGB:
                    output = DXT.DecompressBC1(input, width, height, true);
                    break;
                case TexFormat.BC2_UNORM:
                    output = DXT.DecompressBC2(input, width, height, false);
                    break;
                case TexFormat.BC2_SRGB:
                    output = DXT.DecompressBC2(input, width, height, true);
                    break;
                case TexFormat.BC3_UNORM:
                    output = DXT.DecompressBC3(input, width, height, false);
                    break;
                case TexFormat.BC3_SRGB:
                    output = DXT.DecompressBC3(input, width, height, true);
                    break;
                case TexFormat.BC4_UNORM:
                    output = DXT.DecompressBC4(input, width, height, false);
                    break;
                case TexFormat.BC4_SNORM:
                    output = DXT.DecompressBC4(input, width, height, true);
                    break;
                case TexFormat.BC5_UNORM:
                    output = DXT.DecompressBC5(input, width, height, false);
                    break;
                case TexFormat.BC5_SNORM:
                    output = DXT.DecompressBC5(input, width, height, true);
                    break;
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
