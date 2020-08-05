using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;
using System.IO;

namespace GCNLibrary
{
    public class GCDisk : IArchiveFile, IFileFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "Gamecube Rom" };
        public string[] Extension { get; set; } = new string[] { "*.iso" };
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public List<FileEntry> files = new List<FileEntry>();
        public IEnumerable<ArchiveFileInfo> Files => files;

        public void ClearFiles() { files.Clear(); }

        public bool Identify(File_Info fileInfo, Stream stream) {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(1033843650, 28);
            }
        }

        public DiskHeader Header;

        public void Load(System.IO.Stream stream)
        {
            CanSave = true;
            FileInfo.KeepOpen = true;

            using (var reader = new FileReader(stream, true))
            {
                Header = new DiskHeader();
                Header.Read(reader, this);
            }
        }

        public class DiskHeader
        {
            public char[] GameCode { get; set; }

            public byte DiskID { get; set; }
            public byte Version { get; set; }
            public bool AudioStreaming { get; set; }
            public byte StreamBufferSize { get; set; }

            public char[] GameName { get; set; }

            public FileSystemTable FileTable;

            public void Read(FileReader reader, GCDisk disk)
            {
                reader.SetByteOrder(true);

                GameCode = reader.ReadChars(6);
                DiskID = reader.ReadByte();
                Version = reader.ReadByte();
                AudioStreaming = reader.ReadBoolean();
                StreamBufferSize = reader.ReadByte();
                byte[] Padding = reader.ReadBytes(0x12);
                uint DvdMagic = reader.ReadUInt32();
                GameName = reader.ReadChars(0x3e0);
                uint DebugMonitorOffset = reader.ReadUInt32();
                uint DebugLoadAddress = reader.ReadUInt32();
                byte[] Padding2 = reader.ReadBytes(0x18);
                uint DolOffset = reader.ReadUInt32();
                uint FstOffset = reader.ReadUInt32();
                uint FstSize = reader.ReadUInt32();
                uint FstMaxSize = reader.ReadUInt32();
                uint userPos = reader.ReadUInt32();
                uint userLength = reader.ReadUInt32();
                uint unknown = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();

                reader.SeekBegin(FstOffset);
                FileTable = new FileSystemTable();
                FileTable.Read(reader, disk.files, disk.FilePath);
            }

            public void Write(FileWriter writer, GCDisk disk)
            {
                writer.Write(GameCode);
                writer.Write(DiskID);
                writer.Write(Version);
                writer.Write(AudioStreaming);
                writer.Write(StreamBufferSize);
                writer.Seek(0x12);
                writer.Write(1033843650);

                FileTable.Write(writer, disk.files);
            }
        }

        public class FileSystemTable
        {
            public void Read(FileReader reader, List<FileEntry> Files, string FileName)
            {
                long pos = reader.Position;

                FSTEntry root = new FSTEntry();
                root.NameOffset = reader.ReadUInt32();
                root.Offset = reader.ReadUInt32();
                root.Size = reader.ReadUInt32();

                List<FSTEntry> Entires = new List<FSTEntry>();

                uint stringTableOffset = (uint)pos;
                for (int i = 0; i < root.Size; i++)
                {
                    uint offset = (uint)(pos + (i * 0xC));
                    reader.BaseStream.Position = offset;

                    FSTEntry entry = new FSTEntry();
                    entry.NameOffset = reader.ReadUInt32();
                    entry.Offset = reader.ReadUInt32();
                    entry.Size = reader.ReadUInt32();
                    Entires.Add(entry);

                    stringTableOffset += 0x0C;
                }

                SetFileNames(reader, Entires, 1, Entires.Count, "", stringTableOffset);



                for (int i = 0; i < Entires.Count - 1; i++)
                {
                    if (!Entires[i].IsDirectory)
                    {
                        var fileEntry = new FileEntry(Entires[i]);
                        fileEntry.FileName = Entires[i].FullPath;
                        fileEntry.FileData = new SubStream(reader.BaseStream, Entires[i].Offset, Entires[i].Size);
                        Files.Add(fileEntry);
                    }
                }
            }

            //Based on https://github.com/lioncash/GameFormatReader/blob/master/GameFormatReader/GCWii/Discs/GC/FST.cs#L72
            private int SetFileNames(FileReader reader, List<FSTEntry> fileEntries, int firstIndex, int lastIndex, string directory, uint stringTableOffset)
            {
                int currentIndex = firstIndex;
                while (currentIndex < lastIndex)
                {
                    FSTEntry entry = fileEntries[currentIndex];
                    uint tableOffset = stringTableOffset + (entry.NameOffset & 0xFFFFFF);
                    reader.BaseStream.Position = tableOffset;

                    string filename = reader.ReadZeroTerminatedString();
                    entry.FullPath = directory + filename;

                    if (entry.IsDirectory)
                    {
                        entry.FullPath += "/";
                        currentIndex = SetFileNames(reader, fileEntries, currentIndex + 1, (int)entry.Size, entry.FullPath, stringTableOffset);
                    }
                    else
                    {
                        ++currentIndex;
                    }
                }

                return currentIndex;
            }

            public void Write(FileWriter writer, List<FileEntry> Files)
            {
                long pos = writer.Position;

                //reserve space
                for (int i = 0; i < Files.Count; i++)
                {
                    writer.Write(uint.MaxValue);
                    writer.Write(uint.MaxValue);
                    writer.Write(uint.MaxValue);
                }

                for (int i = 0; i < Files.Count; i++)
                {
                    writer.WriteUint32Offset(pos + (i * 12));
                    writer.Write(Files[i].FileName);
                }

                for (int i = 0; i < Files.Count; i++)
                {
                    writer.WriteUint32Offset(pos + (i * 12) + 4);

                    //More simple to get the size this way than getting file data over and over
                    //Also we don't need to store the bytes in memory
                    //    long _fileStart = writer.Position;
                    writer.Write(Files[i].AsBytes());
                    //   long _fileEnd = writer.Position;

                    /*   using (writer.TemporarySeek(pos + (i * 12) + 8, System.IO.SeekOrigin.Begin))
                       {
                           writer.Write((uint)(_fileEnd - _fileStart));
                       }*/
                }
            }
        }

        public void Save(System.IO.Stream stream)
        {
            Header.Write(new FileWriter(stream), this);
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
            FSTEntry FstEntry;

            public FileEntry(FSTEntry entry) {
                FstEntry = entry;
            }

            public override Stream DecompressData(Stream compressed)
            {
                if (compressed.Length < 8) return compressed;

                foreach (var compression in FileManager.GetCompressionFormats()) {
                    Console.WriteLine($"DECOMPRESS {compression}");
                    if (compression.Identify(compressed, FileName))
                        return compression.Decompress(compressed);
                }
                return base.DecompressData(compressed);
            }
        }

        public class FSTEntry
        {
            public bool IsDirectory
            {
                get { return (NameOffset & 0xFF000000) != 0; }
            }

            public uint NameOffset { get; set; }
            public uint Offset { get; set; }
            public uint Size { get; set; }

            public string FullPath { get; set; }

            public FSTEntry()
            {
            }
        }
    }
}
