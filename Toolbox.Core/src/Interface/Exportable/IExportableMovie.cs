using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IExportableMovie
    {
        void Export(IModelFormat fileFormat, string filePath);
    }
}
