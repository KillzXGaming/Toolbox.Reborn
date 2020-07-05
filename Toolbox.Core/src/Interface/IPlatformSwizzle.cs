using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.Imaging
{
    public interface IPlatformSwizzle
    {
        TexFormat OutputFormat { get; set; }

        byte[] DecodeImage(STGenericTexture texture, byte[] data, uint width, uint height, int array, int mip);
        byte[] EncodeImage(STGenericTexture texture, byte[] data, uint width, uint height, int array, int mip);
    }
}
