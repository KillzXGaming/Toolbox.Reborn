using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CTRLibrary
{
    public class DARC : IFileFormat, IArchiveFile
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "DARC" };
        public string[] Extension { get; set; } = new string[] { "*.darc"};

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "darc");
            }
        }

        public DARC_Parser Header;

        public IEnumerable<ArchiveFileInfo> Files => Header.Files;
        public void ClearFiles() { Header.Files.Clear(); }

        public void Load(Stream stream) {
            Header = new DARC_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }

        public class FileEntry : ArchiveFileInfo
        {
        }
    }
}
