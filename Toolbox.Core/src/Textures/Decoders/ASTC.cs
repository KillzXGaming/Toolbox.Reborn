using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.TextureDecoding;
using Ryujinx.Graphics.Gal.Texture;

namespace Toolbox.Core.Imaging
{
    public class ASTCDecode : ITextureDecoder
    {
        public bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            output = null;
            if (format.ToString().StartsWith("ASTC"))
            {
                var x = (int)TextureFormatHelper.GetBlockWidth(format);
                var y = (int)TextureFormatHelper.GetBlockHeight(format);
                var z = (int)TextureFormatHelper.GetBlockDepth(format);
                output = ASTCDecoder.DecodeToRGBA8888(input, x, y, z, width, height, 1);
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
