using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CTR.NCCH
{
    public class RomFS : IArchiveFile, IFileFormat
    {
        private System.IO.Stream _stream = null;

        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "NCCH RomFS" };
        public string[] Extension { get; set; } = new string[] { "*.bin" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                return reader.CheckSignature(4, "IVFC");
            }
        }

        public Type[] Types
        {
            get
            {
                List<Type> types = new List<Type>();
                return types.ToArray();
            }
        }

        public List<RomfsFileEntry> files = new List<RomfsFileEntry>();
        public IEnumerable<ArchiveFileInfo> Files => files;

        public void ClearFiles() { files.Clear(); }
        const int MediaUnitSize = 0x200;

        private RomfsHeader header;
        private Level3Header Level3Header;
        private long Level3Position;

        public void Load(System.IO.Stream stream)
        {
            _stream = stream;
            FileInfo.KeepOpen = true;
            using (var reader = new FileReader(stream, true))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                header = reader.ReadStruct<RomfsHeader>();
                reader.Align(0x10);
                byte[] masterHash = reader.ReadBytes((int)header.MasterHashSize);

                reader.Align(1 << (int)header.Level1BlockSize);
                Level3Position = reader.Position;
                Level3Header = reader.ReadStruct<Level3Header>();

                reader.SeekBegin(Level3Position + Level3Header.DirectoryMetaDataTableOffset);
                ReadDirectories(reader, reader.Position, Level3Position + Level3Header.FileMetaDataTableOffset);
            }
        }

        public byte[] GetFSData(long Offset, long Size)
        {
            using (var reader = new FileReader(_stream, true))
            {
                reader.SeekBegin(Level3Position + Level3Header.FileDataOffset + Offset);
                return reader.ReadBytes((int)Size);
            }
        }

        //Directory search based on https://github.com/IcySon55/Kuriimu/blob/master/src/archive/archive_nintendo/NcchSupport.cs#L324
        private void ReadDirectories(FileReader reader, long dirStartPosition, long fileStartPosition, string currentPath = "")
        {
            var currentDir = reader.ReadStruct<DirectoryMetaData>();
            string Name = reader.ReadString((int)currentDir.NameLength, Encoding.Unicode);

            if (currentDir.FirstChildOffset != uint.MaxValue)
            {
                reader.SeekBegin(dirStartPosition + currentDir.FirstChildOffset);
                ReadDirectories(reader, dirStartPosition, fileStartPosition, $"{currentPath}{Name}/");
            }

            if (currentDir.FirstFileOffset != uint.MaxValue)
            {
                reader.SeekBegin(fileStartPosition + currentDir.FirstFileOffset);
                var currentFileEntry = ReadFiles(reader, $"{currentPath}{Name}/");
                while (true)
                {
                    if (currentFileEntry.NextSiblingOffset != uint.MaxValue)
                    {
                        reader.SeekBegin(fileStartPosition + currentFileEntry.NextSiblingOffset);
                        currentFileEntry = ReadFiles(reader, $"{currentPath}{Name}/");
                    }
                    else
                        break;
                }
            }

            if (currentDir.NextSiblingOffset != uint.MaxValue)
            {
                reader.SeekBegin(dirStartPosition + currentDir.NextSiblingOffset);
                ReadDirectories(reader, dirStartPosition, fileStartPosition, currentPath);
            }
        }

        public FileMetaData ReadFiles(FileReader reader, string currentPath)
        {
            FileMetaData currentFileEntry = reader.ReadStruct<FileMetaData>();
            string Name = reader.ReadString((int)currentFileEntry.NameLength, Encoding.Unicode);

            files.Add(new RomfsFileEntry()
            {
                FileName = $"{currentPath}{Name}",
                RomfsParent = this,
                Offset = (long)currentFileEntry.FileDataOffset,
                Size = (long)currentFileEntry.FileDataSize,
            });

            return currentFileEntry;
        }

        public void Unload()
        {
            _stream?.Dispose();
            _stream?.Close();
            _stream = null;
        }

        public void Save(System.IO.Stream stream)
        {
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }
    }

    public class RomfsFileEntry : ArchiveFileInfo
    {
        public RomFS RomfsParent;

        public long Offset;
        public long Size;

        public override Stream FileData
        {
            get
            {
                return new MemoryStream(RomfsParent.GetFSData(Offset, Size));
            }
        }
    }
}
