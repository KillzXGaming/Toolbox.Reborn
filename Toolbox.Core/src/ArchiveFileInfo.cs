using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Toolbox.Core.IO;

namespace Toolbox.Core
{
    public class ArchiveFileInfo
    {
        public IArchiveFile ParentArchiveFile { get; set; }

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Toggles visibily of the entry in GUI.
        /// </summary>
        public bool Visibile { get; set; } = true;

        /// <summary>
        /// Determines wether to open the file format automatically
        /// </summary>
        public bool OpenFileFormatOnLoad { get; set; } = false;

        protected Stream _stream;

        private byte[] dataBytes;
        public virtual Stream FileData
        {
            get
            {
                if (dataBytes != null)
                    return new MemoryStream(dataBytes);

                if (_stream != null)
                    _stream.Position = 0;
                return _stream;
            }
            set {
                if (dataBytes != null)
                    dataBytes = value.ToArray();
                _stream = value; }
        }

        public void SetData(Stream data)
        {
            _stream = data;
        }

        public void SetData(byte[] data)
        {
            dataBytes = data;
        }

        public byte[] AsBytes() {
            if (dataBytes != null)
                return dataBytes;

            return FileData.ToArray();
        }

        //The attached file format instance when the file is opened.
        public IFileFormat FileFormat = null;

        public virtual IFileFormat OpenFile()
        {
            Console.WriteLine($"FileName {FileName} OpenFile() {FileData.Length}");

            var file = STFileLoader.OpenFileFormat(DecompressData(FileData), FileName,
                new STFileLoader.Settings()
            {
                ParentArchive = ParentArchiveFile,
            });
            return file;
        }

        public virtual uint GetFileSize() { return 0; return (uint)FileData.Length; }

        public void FileWrite(string filePath) {
           DecompressData(FileData).SaveToFile(filePath);
        }

        public Task FileWriteAsync(string filePath) {
           return Task.Run(() => DecompressData(FileData).SaveToFile(filePath));
        }

        public void SaveFileFormat()
        {
            if (FileFormat != null && FileFormat.CanSave) {
                var mem = new System.IO.MemoryStream();
                FileFormat.Save(mem);
                FileData = CompressData(new MemoryStream(mem.ToArray()));
                //FileFormat.Load(FileData);
            }
        }

        public virtual Stream DecompressData(Stream compressed) {
            return compressed;
        }

        public virtual Stream CompressData(Stream decompressed) {
            return decompressed;
        }
    }
}
