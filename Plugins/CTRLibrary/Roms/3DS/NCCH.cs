using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Toolbox;
using Toolbox.Core;
using Toolbox.Core.IO;
using CTR.NCCH;

namespace FirstPlugin
{
    public class NCCH : IArchiveFile, IFileFormat, IDisposable
    {
        private System.IO.Stream _stream = null;

        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "Nintendo Content Container" };
        public string[] Extension { get; set; } = new string[] { "*.cxi" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "NCCH", 0x100);
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

        private Header header;
        private RomFS RomFS;
        const int MediaUnitSize = 0x200;

        public void Load(System.IO.Stream stream)
        {
            _stream = stream;
            FileInfo.KeepOpen = true;
            using (var reader = new FileReader(stream, true))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                header = reader.ReadStruct<Header>();

                Console.WriteLine("RomfsOffset " + header.RomfsOffset);
                Console.WriteLine("RomfsSize " + header.RomfsSize);

                if (header.RomfsOffset != 0 && header.RomfsSize != 0)
                {
                    uint offset = header.RomfsOffset * MediaUnitSize;
                    uint size = header.RomfsSize * MediaUnitSize;

                    RomFS = new RomFS();
                    RomFS.FileInfo = new File_Info();
                    RomFS.FileInfo.ParentArchive = this;
                    RomFS.Load(new SubStream(stream, offset, size));
                    files.AddRange(RomFS.files);
                }
            }
        }

        public void Dispose()
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
}
