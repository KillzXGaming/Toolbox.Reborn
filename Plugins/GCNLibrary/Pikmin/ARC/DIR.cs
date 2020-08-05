using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1
{
    public class DIR
    {
        public Directory[] Directories { get; set; }

        public DIR(Stream stream) {
            Read(new FileReader(stream));
        }

        private void Read(FileReader reader) {
            reader.SetByteOrder(true);
            uint fileSize = reader.ReadUInt32();
            uint numDirectories = reader.ReadUInt32();
            Directories = new Directory[numDirectories];
            for (int i = 0; i < numDirectories; i++) {
                Directories[i] = new Directory();
                Directories[i].FileOffset = reader.ReadUInt32();
                Directories[i].FileSize = reader.ReadUInt32();
                uint stringLength = reader.ReadUInt32();
                Directories[i].DirectoryPath = reader.ReadString((int)stringLength, true);
            }
        }

        public class Directory
        {
            public uint FileOffset { get; set; }
            public uint FileSize { get; set; }
            public string DirectoryPath { get; set; }
        }
    }
}
