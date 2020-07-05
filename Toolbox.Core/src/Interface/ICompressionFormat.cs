using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a compression format. 
    /// These can be attached to IFileFormats in FileInfo for saving back.
    /// </summary>
    public interface ICompressionFormat
    {
        string[] Description { get; }
        string[] Extension { get; }

        bool CanCompress { get; }

        bool Identify(Stream stream, string fileName);

        Stream Decompress(Stream stream);
        Stream Compress(Stream stream);
    }
}
