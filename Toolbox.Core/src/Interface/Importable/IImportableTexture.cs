using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IImportableTexture
    {
        bool IdentifyImport(string extension);
        STGenericTexture Import(string filePath);
    }
}
