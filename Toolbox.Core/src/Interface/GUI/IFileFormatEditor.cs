using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public interface IFileFormatEditor
    {
        bool SupportsMultipleFiles { get; }

        bool Identify(IFileFormat fileFormat);
        void LoadFileFormat(IFileFormat fileFormat);

        IFileFormat[] GetFileFormats();
    }
}
