using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.Pikmin1
{
    public class ARC : IFileFormat, IArchiveFile
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "ARC" };
        public string[] Extension { get; set; } = new string[] { "*.arc" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;


        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".arc" && File.Exists(fileInfo.FilePath.Replace(".arc", ".dir"));
        }

        public IEnumerable<ArchiveFileInfo> Files => Header.Files;
        public void ClearFiles() { Header.Files.Clear(); }

        public ARC_Parser Header;

        public void Load(Stream stream) {
            var dir = new DIR(File.OpenRead(FileInfo.FilePath.Replace(".arc", ".dir")));
            Header = new ARC_Parser(stream, dir);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Add(new ARC_Parser.FileEntry()
            {
                FileName = archiveFileInfo.FileName,
                FileData = archiveFileInfo.FileData,
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Remove((ARC_Parser.FileEntry)archiveFileInfo);
            return true;
        }

        public class ArcFile : ArchiveFileInfo
        {

        }
    }
}
