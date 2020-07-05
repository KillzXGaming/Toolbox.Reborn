using System;
using System.Collections.Generic;
using System.IO;

namespace Toolbox.Core
{
    public interface IFileIconLoader
    {
        string Identify(string name, Stream stream);
        Dictionary<string, byte[]> ImageList { get; }
    }
}
