using System;
using System.Collections.Generic;
using System.Text;


namespace Toolbox.Core
{
    /// <summary>
    /// A container to store textures.
    /// </summary>
    public interface ITextureContainer
    {
        IEnumerable<STGenericTexture> TextureList { get; }
        bool DisplayIcons { get; }
    }
}
