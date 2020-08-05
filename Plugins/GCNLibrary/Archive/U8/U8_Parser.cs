using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary
{
    public class U8_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();

        private readonly uint BEMagic = 0x55AA382D;
        private readonly uint LEMagic = 0x2D38AA55;

        private bool IsBigEndian = false;

        public U8_Parser(System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream)) {
                reader.SetByteOrder(true);
                uint Signature = reader.ReadUInt32();
                IsBigEndian = Signature == BEMagic;
                reader.SetByteOrder(IsBigEndian);

                uint FirstNodeOffset = reader.ReadUInt32();
                uint NodeSectionSize = reader.ReadUInt32();
                uint FileDataOffset = reader.ReadUInt32();
                byte[] Reserved = new byte[4];

                reader.SeekBegin(FirstNodeOffset);
                var RootNode = new NodeEntry();
                RootNode.Read(reader);

                //Root has total number of nodes 
                uint TotalNodeCount = RootNode.Setting2;

                //Read all our entries
                List<NodeEntry> entries = new List<NodeEntry>();
                entries.Add(RootNode);
                for (int i = 0; i < TotalNodeCount - 1; i++)
                {
                    var node = new NodeEntry();
                    node.Read(reader);
                    entries.Add(node);
                }

                //Read string pool
                uint stringPoolPos = 0;
                Dictionary<uint, string> StringTable = new Dictionary<uint, string>();
                for (int i = 0; i < TotalNodeCount; i++)
                {
                    string str = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated, Encoding.ASCII);
                    StringTable.Add(stringPoolPos, str);
                    stringPoolPos += (uint)str.Length + 1;
                }

                //Set the strings
                for (int i = 0; i < TotalNodeCount; i++)
                {
                    entries[i].Name = StringTable[entries[i].StringPoolOffset];
                }

                entries[0].Name = "Root";

                SetFileNames(entries, 1, entries.Count, "");

                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].nodeType != NodeEntry.NodeType.Directory)
                    {
                        FileEntry entry = new FileEntry();
                        reader.SeekBegin(entries[i].Setting1);
                        entry.SetData(reader.ReadBytes((int)entries[i].Setting2));
                        entry.FileName = entries[i].FullPath;
                        Files.Add(entry);
                    }
                }
            }
        }

        public void Save(System.IO.Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                writer.SetByteOrder(IsBigEndian);
                writer.Write(BEMagic);
            }
        }

        private int SetFileNames(List<NodeEntry> fileEntries, int firstIndex, int lastIndex, string directory)
        {
            int currentIndex = firstIndex;
            while (currentIndex < lastIndex)
            {
                NodeEntry entry = fileEntries[currentIndex];
                string filename = entry.Name;
                entry.FullPath = directory + filename;

                if (entry.nodeType == NodeEntry.NodeType.Directory)
                {
                    entry.FullPath += "/";
                    currentIndex = SetFileNames(fileEntries, currentIndex + 1, (int)entry.Setting2, entry.FullPath);
                }
                else
                {
                    ++currentIndex;
                }
            }

            return currentIndex;
        }

        public class NodeEntry
        {
            public string FullPath { get; set; }

            public NodeType nodeType
            {
                get { return (NodeType)(flags >> 24); }
            }

            public enum NodeType
            {
                File,
                Directory,
            }

            public uint StringPoolOffset
            {
                get { return flags & 0x00ffffff; }
            }

            private uint flags;

            public uint Setting1; //Offset (file) or parent index (directory)
            public uint Setting2; //Size (file) or node count (directory)

            public string Name { get; set; }

            public void Read(FileReader reader)
            {
                flags = reader.ReadUInt32();
                Setting1 = reader.ReadUInt32();
                Setting2 = reader.ReadUInt32();
            }
        }

        public class FileEntry : ArchiveFileInfo
        {
            public NodeEntry nodeEntry;
        }
    }
}
