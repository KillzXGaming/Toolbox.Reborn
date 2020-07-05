using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IExportableTexture
    {
        bool IdentifyExport(string extension);
        void Export(STGenericTexture texture, TextureExportSettings settings, string filePath);
    }
}
