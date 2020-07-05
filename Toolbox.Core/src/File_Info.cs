using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Stores information used for a loaded file found in an <see cref="IFileFormat"/>.
    /// </summary>
    public class File_Info
    {
        /// <summary>
        /// The path the file is located at.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// The name of the file without the path.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Keeps the stream open.
        /// </summary>
        public bool KeepOpen = false;

        /// <summary>
        /// The archive being parented if the file is inside an archive.
        /// </summary>
        public IArchiveFile ParentArchive = null;

        /// <summary>
        /// The compression format if used for decompressing and compressing back.
        /// </summary>
        public ICompressionFormat Compression = null;

        /// <summary>
        /// The decompressed size of the format.
        /// </summary>
        public uint DecompressedSize;

        /// <summary>
        /// The compressed size of the format.
        /// </summary>
        public uint CompressedSize;

        /// <summary>
        /// The extension of the file.
        /// </summary>
        public string Extension => Utils.GetExtension(FileName);

        /// <summary>
        /// Gets the folder the file or archive file is located in.
        /// </summary>
        public string FolderPath {
            get { return System.IO.Path.GetDirectoryName(GetSourcePath(this)); }
        }

        public string SourcePath {
            get { return GetSourcePath(this); }
        }

        //The stream of the file. Used for disposing when the file is closed
        public Stream Stream { get; set; }

        static string GetSourcePath(File_Info fileInfo)
        {
            if (fileInfo.ParentArchive != null)
                return GetSourcePath(((IFileFormat)fileInfo.ParentArchive).FileInfo);

            return fileInfo.FilePath;
        }
    }
}
