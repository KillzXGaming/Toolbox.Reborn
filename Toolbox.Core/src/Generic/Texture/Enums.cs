using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core
{
    public enum STCompressionMode
    {
        Slow,
        Normal,
        Fast
    }

    public enum STRotateFlipType
    {
        Rotate90,
        Rotate180,
        Rotate270,
        FlipX,
        FlipY,
    }

    public enum STChannelType
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Alpha = 3,
        One = 4,
        Zero = 5,
    }

    public enum STSurfaceType
    {
        Texture1D,
        Texture2D,
        Texture3D,
        TextureCube,
        Texture1D_Array,
        Texture2D_Array,
        Texture2D_Mulitsample,
        Texture2D_Multisample_Array,
        TextureCube_Array,
    }
}
