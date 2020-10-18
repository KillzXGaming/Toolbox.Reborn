using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CTRLibrary
{
    public class GAR_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();

        public List<IFileGroup> FileGroups = new List<IFileGroup>();
        public List<IFileInfo> GarFileInfos = new List<IFileInfo>();

        public string Signature;
        public ushort FileGroupCount;
        public ushort FileCount;
        public uint FileGroupOffset;
        public uint FileInfoOffset;
        public uint DataOffset;
        public string Codename;

        public VersionMagic Version;

        public enum VersionMagic
        {
            ZAR1, //OOT3D
            GAR2, //MM3D
            GAR5, //LM3DS
        }

        public GAR_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                Signature = reader.ReadString(4, Encoding.ASCII);
                switch (Signature)
                {
                    case "ZAR\x01":
                        Version = VersionMagic.ZAR1;
                        break;
                    case "GAR\x02":
                        Version = VersionMagic.GAR2;
                        break;
                    case "GAR\x05":
                        Version = VersionMagic.GAR5;
                        break;
                }

                uint FileSize = reader.ReadUInt32();
                FileGroupCount = reader.ReadUInt16();
                FileCount = reader.ReadUInt16();
                FileGroupOffset = reader.ReadUInt32();
                FileInfoOffset = reader.ReadUInt32();
                DataOffset = reader.ReadUInt32();
                Codename = reader.ReadString(0x08);

                switch (Codename)
                {
                    case "queen\0\0\0":
                    case "jenkins\0":
                        ReadZeldaArchive(reader);
                        break;
                    case "agora\0\0\0":
                    case "SYSTEM\0\0":
                        ReadSystemGrezzoArchive(reader);
                        break;
                    default:
                        throw new Exception($"Unexpected codename! {Codename}");
                }
            }
        }

        private void ReadSystemGrezzoArchive(FileReader reader)
        {
            reader.SeekBegin(FileGroupOffset);
            for (int i = 0; i < FileGroupCount; i++)
            {
                FileGroups.Add(new SystemFileGroup(reader));
                reader.Align(0x20);
            }

            reader.BaseStream.Position = FileGroups.Min(x => ((SystemFileGroup)x).SubTableOffset);
            byte[] entries = reader.ReadBytes((int)FileInfoOffset - (int)reader.BaseStream.Position);

            reader.SeekBegin(FileInfoOffset);
            for (int i = 0; i < FileGroupCount; i++)
            {
                for (int f = 0; f < ((SystemFileGroup)FileGroups[i]).FileCount; f++)
                {
                    GarFileInfos.Add(new SystemGarFileInfo(reader));
                    ((SystemGarFileInfo)GarFileInfos.Last()).ext = ((SystemFileGroup)FileGroups[i]).Name;
                }
            }

            reader.SeekBegin(DataOffset);
            for (int i = 0; i < GarFileInfos.Count; i++)
            {
                var info = (SystemGarFileInfo)GarFileInfos[i];
                Files.Add(new FileEntry()
                {
                    OpenFileFormatOnLoad = info.ext == "csab",
                    FileName = $"{info.Name}.{info.ext}",
                    FileData = new SubStream(reader.BaseStream, info.FileOffset, info.FileSize)
                });
            }
        }

        private void ReadZeldaArchive(FileReader reader)
        {
            reader.SeekBegin(FileGroupOffset);
            for (int i = 0; i < FileGroupCount; i++)
                FileGroups.Add(new FileGroup(reader));

            for (int i = 0; i < FileGroupCount; i++)
                ((FileGroup)FileGroups[i]).Ids = reader.ReadInt32s((int)((FileGroup)FileGroups[i]).FileCount);


            reader.SeekBegin(FileInfoOffset);
            for (int i = 0; i < FileGroupCount; i++)
            {
                for (int f = 0; f < ((FileGroup)FileGroups[i]).FileCount; f++)
                {
                    if (Version == VersionMagic.ZAR1)
                        GarFileInfos.Add(new ZarFileInfo(reader));
                    else
                        GarFileInfos.Add(new GarFileInfo(reader));
                }
            }

            reader.SeekBegin(DataOffset);
            uint[] Offsets = reader.ReadUInt32s(FileCount);
            for (int i = 0; i < GarFileInfos.Count; i++)
            {
                if (GarFileInfos[i] is ZarFileInfo)
                {
                    Files.Add(new FileEntry()
                    {
                       // OpenFileFormatOnLoad = ((ZarFileInfo)GarFileInfos[i]).FileName.Contains("csab"),

                        FileName = ((ZarFileInfo)GarFileInfos[i]).FileName,
                        FileData = new SubStream(reader.BaseStream, Offsets[i], ((ZarFileInfo)GarFileInfos[i]).FileSize)
                    });
                }
                else
                {
                    Files.Add(new FileEntry()
                    {
                       // OpenFileFormatOnLoad = ((GarFileInfo)GarFileInfos[i]).FileName.Contains("csab"),

                        FileName = ((GarFileInfo)GarFileInfos[i]).FileName,
                        FileData = new SubStream(reader.BaseStream, Offsets[i], ((GarFileInfo)GarFileInfos[i]).FileSize)
                    });
                }
            }
        }

        public void Save(Stream stream)
        {

        }

        public class FileGroup : IFileGroup
        {
            public uint FileCount;
            public uint DataOffset;
            public uint InfoOffset;

            public int[] Ids;

            public FileGroup(FileReader reader)
            {
                FileCount = reader.ReadUInt32();
                DataOffset = reader.ReadUInt32();
                InfoOffset = reader.ReadUInt32();
                reader.ReadUInt32(); //padding
            }
        }

        public class SystemFileGroup : IFileGroup
        {
            public uint FileCount;
            public uint Unknown;
            public uint InfoOffset;
            public string Name;
            public uint SubTableOffset;

            public int[] Ids;

            public SystemFileGroup(FileReader reader)
            {
                FileCount = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();
                InfoOffset = reader.ReadUInt32();
                Name = reader.LoadString(false, typeof(uint));
                SubTableOffset = reader.ReadUInt32();
            }
        }

        public class ZarFileInfo : IFileInfo
        {
            public uint FileSize;
            public string FileName;

            public ZarFileInfo(FileReader reader)
            {
                FileSize = reader.ReadUInt32();
                FileName = reader.LoadString(false, typeof(uint));
            }
        }

        public class GarFileInfo : IFileInfo
        {
            public uint FileSize;
            public string FileName;
            public string Name;

            public GarFileInfo(FileReader reader)
            {
                FileSize = reader.ReadUInt32();
                Name = reader.LoadString(false, typeof(uint));
                FileName = reader.LoadString(false, typeof(uint));
            }
        }

        public class SystemGarFileInfo : IFileInfo
        {
            public uint FileSize;
            public uint FileOffset;
            public string FileName;
            public string Name;
            public string ext;

            public SystemGarFileInfo(FileReader reader)
            {
                FileSize = reader.ReadUInt32();
                FileOffset = reader.ReadUInt32();
                Name = reader.LoadString(false, typeof(uint));
                reader.ReadUInt32();//padding
            }
        }

        public interface IFileInfo
        {

        }

        public interface IFileGroup
        {

        }

        public class FileEntry : ArchiveFileInfo
        {
            public uint Unknown { get; set; }

            internal uint Size;
            internal uint Offset;
            internal uint NameOffset;

            public void Read(FileReader reader)
            {
                uint Unknown = reader.ReadUInt32();
                NameOffset = reader.ReadUInt32();
                Offset = reader.ReadUInt32();
                Size = reader.ReadUInt32();
            }
        }
    }
}
