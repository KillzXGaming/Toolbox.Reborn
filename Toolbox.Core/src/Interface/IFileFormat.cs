using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Toolbox.Core
{
    /// <summary>
    /// A file format interface used to load, edit and save file formats.
    /// </summary>
    public interface IFileFormat
    {
        string[] Description { get; }
        string[] Extension { get; }

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        bool CanSave { get; set; }

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        File_Info FileInfo { get; set; }

        bool Identify(File_Info fileInfo, Stream stream);

        void Load(Stream stream);
        void Save(Stream stream);
    }
}   
