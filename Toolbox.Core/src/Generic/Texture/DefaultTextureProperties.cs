using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class DefaultTextureProperties
    {
        private STGenericTexture texture;

        public uint Width => texture.Width;
        public uint Height => texture.Height;
        public uint Depth => texture.Depth;

        public string Format => $"{texture.Platform.ToString()}";
        public uint MipCount => texture.MipCount;
        public uint ArrayCount => texture.ArrayCount;
        public uint ImageSize => (uint)texture.DataSizeInBytes;

        public STSurfaceType SurfaceType => texture.SurfaceType;

        public DefaultTextureProperties(STGenericTexture tex) {
            texture = tex; 
        }
    }
}
