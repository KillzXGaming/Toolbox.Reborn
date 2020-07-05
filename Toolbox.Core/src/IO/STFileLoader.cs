using Syroot.BinaryData;
using System.IO;
using System;
using System.IO.Compression;
using OpenTK;

namespace Toolbox.Core.IO
{
    public class STFileLoader
    {
        public class Settings
        {
            public Stream Stream = null;

            /// <summary>
            /// Keeps the file stream open.
            /// </summary>
            public bool KeepOpen = false;

            /// <summary>
            /// The parent archive format set.
            /// </summary>
            public IArchiveFile ParentArchive = null;

            /// <summary>
            /// The compression format set.
            /// </summary>
            public ICompressionFormat CompressionFormat = null;

            /// <summary>
            /// Filters file formats to only load the specified types.
            /// </summary>
            public Type[] FileFilter = null;

            //Keep these sizes stored for useful file information
            internal uint DecompressedSize = 0;
            internal uint CompressedSize = 0;
        }

        public static Settings TryDecompressFile(Stream stream, string fileName)
        {
            long streamStartPos = stream.Position;

            Settings settings = new Settings();
            settings.DecompressedSize = (uint)stream.Length;

            try
            {
                foreach (ICompressionFormat compressionFormat in FileManager.GetCompressionFormats())
                {
                    stream.Position = streamStartPos;
                    if (compressionFormat.Identify(stream, fileName))
                    {
                        stream.Position = streamStartPos;

                        settings.CompressedSize = (uint)stream.Length;
                        settings.Stream = compressionFormat.Decompress(stream);
                        settings.DecompressedSize = (uint)stream.Length;
                        settings.CompressionFormat = compressionFormat;
                        return settings;
                    }
                }
            } //It's possible some types fail to compress if identify was incorrect so we should skip any errors
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return settings;
        }

        /// <summary>
        /// Gets the <see cref="IFileFormat"/> from a file or byte array. 
        /// </summary>
        /// <param name="FileName">The name of the file</param>
        /// <param name="data">The byte array of the data</param>
        /// <param name="InArchive">If the file is in an archive so it can be saved back</param>
        /// <param name="archiveNode">The node being replaced from an archive</param>
        /// <param name="Compressed">If the file is being compressed or not</param>
        /// <param name="CompType">The type of <see cref="CompressionType"/> being used</param>
        /// <returns></returns>
        public static IFileFormat OpenFileFormat(string FileName, Settings settings = null)
        {
            return OpenFileFormat(File.OpenRead(FileName), FileName, settings);
        }

        /// <summary>
        /// Gets the <see cref="IFileFormat"/> from a file or byte array. 
        /// </summary>
        /// <param name="FileName">The name of the file</param>
        /// <param name="data">The byte array of the data</param>
        /// <param name="InArchive">If the file is in an archive so it can be saved back</param>
        /// <param name="Compressed">If the file is being compressed or not</param>
        /// <param name="CompressionFormat">The type of <see cref="ICompressionFormat"/> being used</param>
        /// <returns></returns>
        public static IFileFormat OpenFileFormat(Stream stream, string FileName, Settings settings = null)
        {
            //File is empty so return
            if (stream == null || stream.Length < 8) return null;

            //Create settings if none set
            if (settings == null) settings = new Settings();

            long streamStartPos = stream.Position;

            //Check if the current setting has a compression format set or not
            if (settings.CompressionFormat == null)
            {
                var decompressedSettings = TryDecompressFile(stream, FileName);
                if (decompressedSettings.CompressionFormat != null) {
                    stream.Dispose();
                    stream.Close();
                    return OpenFileFormat(decompressedSettings.Stream, FileName, decompressedSettings);
                }
            }

            var info = new File_Info();
            info.FileName = Path.GetFileName(FileName);
            info.FilePath = FileName;
            info.ParentArchive = settings.ParentArchive;

            stream.Position = streamStartPos;
            foreach (IFileFormat fileFormat in FileManager.GetFileFormats())
            {
                //Set the file name so we can check it's extension in the identifier. 
                //Most is by magic but some can be extension or name.

                if (fileFormat.Identify(info, stream) && !IsFileFiltered(fileFormat, settings))
                {
                    fileFormat.FileInfo = info;
                    fileFormat.FileInfo.Stream = stream;
                    return SetFileFormat(fileFormat, FileName, stream, settings);
                }
            }

            //Dispose stream if file format nor compression formats can load
            if (settings.CompressionFormat == null)
                stream.Close();

            return null;
        }

        private static bool IsFileFiltered(IFileFormat fileFormat, Settings settings)
        {
            if (settings.FileFilter == null || settings.FileFilter.Length == 0)
                return false;

            foreach (var type in settings.FileFilter)
            {
                if (type == fileFormat.GetType())
                    return false;

                foreach (var inter in type.GetInterfaces())
                {
                    if (inter.IsGenericType && inter.GetGenericTypeDefinition() == fileFormat.GetType())
                        return false;
                }
            }
            return true;
        }

        private static IFileFormat SetFileFormat(IFileFormat fileFormat, string FileName, Stream stream, Settings settings = null)
        {
            fileFormat.FileInfo.DecompressedSize = (uint)settings.DecompressedSize;
            fileFormat.FileInfo.CompressedSize = (uint)settings.CompressedSize;
            fileFormat.FileInfo.Compression = settings.CompressionFormat;
            fileFormat.Load(stream);

            //Apply necessary file info for archive files
            if (fileFormat is IArchiveFile) {
                foreach (var file in ((IArchiveFile)fileFormat).Files)
                    file.ParentArchiveFile = (IArchiveFile)fileFormat;
            }

            //After file has been loaded and read, we'll dispose unless left ope
            if (!settings.KeepOpen && !fileFormat.FileInfo.KeepOpen)
            {
                stream.Dispose();
                stream.Close();
                settings.Stream?.Dispose();
                settings.Stream?.Close();
                GC.SuppressFinalize(stream);
            }

            return fileFormat;
        }
    }
}
