using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;

namespace CTRLibrary
{
    public class DARC_Parser
    {
        public List<FileEntry> Files = new List<FileEntry>();

        public DARC_Parser(Stream stream)
        {

        }

        public void Save(Stream stream)
        {

        }

        public class FileEntry : ArchiveFileInfo
        {

        }
    }
}
