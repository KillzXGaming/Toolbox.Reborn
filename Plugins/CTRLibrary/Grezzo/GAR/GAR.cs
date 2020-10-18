using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CTRLibrary
{
    public class GAR : IFileFormat, IArchiveFile, IDisposable
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "Grezzo Archive", "Zelda Archive" };
        public string[] Extension { get; set; } = new string[] { "*.gar", "*.zar" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(3, "ZAR") ||
                       reader.CheckSignature(3, "GAR");
            }
        }

        public GAR_Parser Header;

        public IEnumerable<ArchiveFileInfo> Files => Header.Files;
        public void ClearFiles() { Header.Files.Clear(); }

        private Stream _stream;

        public void Load(Stream stream) {
            _stream = stream;
            FileInfo.KeepOpen = true;
            Header = new GAR_Parser(stream);
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

        public void Dispose()
        {
            _stream?.Dispose();
            foreach (var file in Files)
                file.FileData?.Dispose();
            _stream = null;
        }
    }
}
