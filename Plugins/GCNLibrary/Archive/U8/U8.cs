using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary
{
    public class U8 : IFileFormat, IArchiveFile
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "U8" };
        public string[] Extension { get; set; } = new string[] { "*.u8" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        private readonly uint BEMagic = 0x55AA382D;
        private readonly uint LEMagic = 0x2D38AA55;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                reader.SetByteOrder(true);
                uint signature = reader.ReadUInt32();
                return signature == BEMagic || signature == LEMagic;
            }
        }

        public IEnumerable<ArchiveFileInfo> Files => Header.Files;
        public void ClearFiles() { Header.Files.Clear(); }

        public U8_Parser Header;

        public void Load(Stream stream) {
            Header = new U8_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Add(new U8_Parser.FileEntry()
            {
                FileName = archiveFileInfo.FileName,
                FileData = archiveFileInfo.FileData,
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Remove((U8_Parser.FileEntry)archiveFileInfo);
            return true;
        }

        public class ArcFile : ArchiveFileInfo
        {

        }
    }
}
