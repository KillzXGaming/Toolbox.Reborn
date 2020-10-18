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
    //Documentation from https://www.3dbrew.org/wiki/NCSD
    public class NCSD : IArchiveFile, IFileFormat
    {
        private System.IO.Stream _stream = null;

        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "CTR Cart Image" };
        public string[] Extension { get; set; } = new string[] { "*.cci", "*.3ds" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = false;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "NCSD", 0x100);
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

        public NCCH ncch;
        private CTR.NCSD.Header header;

        const int MediaUnitSize = 0x200;

        public void Load(System.IO.Stream stream)
        {
            _stream = stream;
            FileInfo.KeepOpen = true;
            using (var reader = new FileReader(stream, true))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                header = reader.ReadStruct<CTR.NCSD.Header>();

                for (int i = 0; i < header.Parts.Length; i++)
                {
                    if (header.Parts[i].Offset != 0)
                    {
                        string name;
                        if (PartNames.ContainsKey(i))
                            name = PartNames[i];
                        else
                            name = "Unknown.cfa";

                        //Load the cxi
                        if (i == 0)
                        {
                            ncch = new NCCH();
                            ncch.FileInfo = new File_Info();
                            ncch.FileInfo.ParentArchive = this;

                            Console.WriteLine("Length NCCH " + reader.BaseStream.Length);

                            ncch.Load(new SubStream(stream,
                                header.Parts[i].Offset * MediaUnitSize, 
                                header.Parts[i].Size * MediaUnitSize));

                            files.AddRange(ncch.files);
                        }
                    }
                }
            }
        }

        public Dictionary<int, string> PartNames = new Dictionary<int, string>()
        {
            {0, "GameData.cxi" },
            {1, "EManual.cfa" },
            {2, "DLP.cfa" },
            {6, "FirmwareUpdate.cfa" },
            {7, "UpdateData.cfa" },
        };

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
}
