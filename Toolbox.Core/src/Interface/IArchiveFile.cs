using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a file that archives multiple files.
    /// </summary>
    public interface IArchiveFile
    {
        /// <summary>
        /// Determines if files can be added.
        /// </summary>
        bool CanAddFiles { get; }

        /// <summary>
        /// Determines if files can be rename.
        /// </summary>
        bool CanRenameFiles { get; }

        /// <summary>
        /// Determines if files can be replaced.
        /// </summary>
        bool CanReplaceFiles { get; }

        /// <summary>
        /// Determines if files can be deleted.
        /// </summary>
        bool CanDeleteFiles { get; }

        IEnumerable<ArchiveFileInfo> Files { get; }

        void ClearFiles();
        bool AddFile(ArchiveFileInfo archiveFileInfo);
        bool DeleteFile(ArchiveFileInfo archiveFileInfo);
    }
}
