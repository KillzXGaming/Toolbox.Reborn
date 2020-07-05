using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IExportableModel
    {
        bool IdentfiyExport(string extension);
        void Export(STGenericModel model, string filePath);
    }
}
