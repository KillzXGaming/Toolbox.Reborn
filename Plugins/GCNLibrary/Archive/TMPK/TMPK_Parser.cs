using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace GCNLibrary
{
    public class TMPK_Parser
    {
        public static readonly uint DefaultAlignment = 4;

        public List<FileEntry> Files = new List<FileEntry>();
        public uint Alignment;

        public TMPK_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                reader.SetByteOrder(true);

                reader.ReadSignature(4, "TMPK");
                uint FileCount = reader.ReadUInt32();
                Alignment = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();
                for (int i = 0; i < FileCount; i++)
                {
                    var info = new FileEntry(reader);
                    Files.Add(info);
                }
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.SetByteOrder(true);
                writer.WriteSignature("TMPK");
                writer.Write(Files.Count);
                writer.Write(Alignment);
                writer.Write(0);
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].SaveFileFormat();

                    Files[i]._posHeader = writer.Position;
                    writer.Write(uint.MaxValue);
                    writer.Write(uint.MaxValue);
                    writer.Write((uint)Files[i].FileData.Length); //Padding
                    writer.Write(0); //Padding
                }
                for (int i = 0; i < Files.Count; i++)
                {
                    writer.WriteUint32Offset(Files[i]._posHeader);
                    writer.Write(Files[i].FileName, Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);
                }
                for (int i = 0; i < Files.Count; i++)
                {
                    SetAlignment(writer, Files[i].FileName);
                    writer.WriteUint32Offset(Files[i]._posHeader + 4);

                    writer.Write(Files[i].AsBytes());
                }
            }
        }

        private void SetAlignment(FileWriter writer, string FileName)
        {
            if (FileName.EndsWith(".gmx"))
                writer.Align(0x40);
            else if (FileName.EndsWith(".gtx"))
                writer.Align(0x2000);
            else
                writer.Write(DefaultAlignment);
        }

        public class FileEntry : ArchiveFileInfo
        {
            internal long _posHeader;

            public FileEntry() { }

            public FileEntry(FileReader reader)
            {
                long pos = reader.Position;

                uint NameOffset = reader.ReadUInt32();
                uint FileOffset = reader.ReadUInt32();
                uint FileSize = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();

                reader.Seek(NameOffset, System.IO.SeekOrigin.Begin);
                FileName = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated);

                reader.Seek(FileOffset, System.IO.SeekOrigin.Begin);
                SetData(reader.ReadBytes((int)FileSize));

                reader.Seek(pos + 16, System.IO.SeekOrigin.Begin);
            }
        }
    }
}
