using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CTRLibrary
{
    public class WTA_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();

        public ushort Version { get; set; }
        public bool IsBigEndian { get; set; }

        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public WTA_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                reader.SetByteOrder(false);

                uint magic = reader.ReadUInt32();
                uint Version = reader.ReadUInt32();
                if (Version != 1)
                    reader.SetByteOrder(true);
                reader.ReadUInt32(); //0
                uint alignment = reader.ReadUInt32();
                reader.ReadUInt32(); //slightly larger than file size
                reader.ReadUInt32(); //0
                uint dataOffset = reader.ReadUInt32();
                uint stringTableOffset = reader.ReadUInt32();
                uint stringTableSize = reader.ReadUInt32();
                reader.ReadUInt32(); //64
                uint unkSectionCount = reader.ReadUInt32();
                uint FileCount = reader.ReadUInt32();
                reader.ReadUInt32(); //1
                reader.ReadUInt32(); //0
                reader.ReadUInt32(); //0
                reader.ReadUInt32(); //0

                //Skip an unknown section that is 32 bytes in size
                reader.Seek(unkSectionCount * 32);

                for (int i = 0; i < FileCount; i++)
                    Files.Add(new FileEntry(reader, dataOffset, stringTableOffset));

                //Now read data and align offsets
                reader.SeekBegin(dataOffset);
                for (int i = 0; i < FileCount; i++)
                {
                    Files[i].FileName = $"File {i}";
                    Files[i].DataOffset = reader.Position;
                    if (Files[i].CompressedSize != 0)
                        reader.Seek((int)Files[i].CompressedSize);
                    else
                        reader.Seek((int)Files[i].UncompressedSize);

                    //    Console.WriteLine($"{i} {files[i].DataOffset} {files[i].CompressedSize} {files[i].Alignment}");

                    if (Files[i].Alignment != 0)
                        reader.Align((int)Files[i].Alignment);
                }

                //Try to get file names from file formats inside
                //The string table for this file uses a bunch of ids and not very ideal for viewing
                for (int i = 0; i < FileCount; i++)
                {
                    if (Files[i].CompressedSize == 0)
                        continue;

                    reader.SeekBegin(Files[i].DataOffset);
                    var data = reader.ReadBytes((int)Files[i].CompressedSize);
                    if (Files[i].CompressedSize != 0 && Files[i].CompressedSize != Files[i].UncompressedSize && data[0] == 0x78 && data[1] == 0x5E)
                        data = STLibraryCompression.ZLIB.Decompress(data);

                    using (var dataReader = new FileReader(data))
                    {
                        if (dataReader.CheckSignature(4, "FRES") || dataReader.CheckSignature(4, "BNTX"))
                        {
                            dataReader.SetByteOrder(false);
                            dataReader.SeekBegin(16);
                            uint fileNameOffset = dataReader.ReadUInt32();
                            dataReader.SeekBegin(fileNameOffset);
                            Files[i].FileName = dataReader.ReadZeroTerminatedString();
                        }
                    }
                }
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.SetByteOrder(IsBigEndian);
                writer.WriteSignature("WTA ");
            }
        }

        public class FileEntry : ArchiveFileInfo
        {
            public uint Alignment;
            public uint CompressedSize;
            public uint UncompressedSize;

            public long DataOffset;

            public override Stream FileData
            {
                get { return DecompressData(); }
            }

            private Stream DecompressData()
            {
                using (var reader = new FileReader(baseStream, true))
                {
                    reader.SeekBegin(DataOffset);
                    var data = reader.ReadBytes((int)CompressedSize);
                    if (CompressedSize != UncompressedSize && data[0] == 0x78 && data[1] == 0x5E)
                        data = STLibraryCompression.ZLIB.Decompress(data);

                    return new MemoryStream(data);
                }
            }

            private Stream baseStream;

            public FileEntry() { }

            public FileEntry(FileReader reader, uint dataOffset, uint stringTableOffset)
            {
                long pos = reader.Position;

                baseStream = reader.BaseStream;
                uint nameLength = reader.ReadUInt32();
                uint hash = reader.ReadUInt32();
                Alignment = reader.ReadUInt32();
                UncompressedSize = reader.ReadUInt32();
                CompressedSize = reader.ReadUInt32();
                uint nameOffset = reader.ReadUInt32();
                uint unk = reader.ReadUInt32();
                uint unk2 = reader.ReadUInt32();

                // FileName = reader.ReadString(0x20, true);
                // reader.ReadUInt32();

                long endpos = reader.Position;
                reader.Seek(endpos, System.IO.SeekOrigin.Begin);
            }
        }
    }
}
