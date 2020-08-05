using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary.Pikmin1
{
    public class ARC_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();
        public ARC_Parser(System.IO.Stream stream, DIR dir)
        {
            using (var reader = new FileReader(stream)) {
                reader.SetByteOrder(true);
        
                foreach (var file in dir.Directories)
                {
                    reader.SeekBegin(file.FileOffset);
                    var fileEnry = new FileEntry();
                    fileEnry.SetData(reader.ReadBytes((int)file.FileSize));
                    fileEnry.FileName = file.DirectoryPath;
                    Files.Add(fileEnry);
                }
            }
        }

        public void Save(System.IO.Stream stream)
        {
            using (var writer = new FileWriter(stream)) {

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
