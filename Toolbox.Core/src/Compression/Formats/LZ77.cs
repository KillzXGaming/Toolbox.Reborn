using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Toolbox.Core.IO;

namespace Toolbox.Core
{
    public class LZ77 : ICompressionFormat
    {
        public string[] Description { get; set; } = new string[] { "LZ77 Compressed" };
        public string[] Extension { get; set; } = new string[] { "*.lz", };

        public bool Identify(Stream stream, string fileName)
        {
            if (stream.Length < 16)
                return false;

            using (var reader = new FileReader(stream, true))
            {
                if (Utils.GetExtension(fileName) == ".carc")
                {
                    return reader.ReadByte() == 0x10;
                }
                if (Utils.GetExtension(fileName) == ".lz")
                {
                    reader.SeekBegin(12);
                    return reader.ReadByte() == 0x11;
                }
            }
            return false;
        }

        public bool CanCompress { get; } = true;

        public Stream Decompress(Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                byte type = reader.ReadByte();
                if (type == 0x10)
                {
                    uint decomp_size = reader.ReadUInt32();

                    var sub = new SubStream(stream, 0);
                    return new MemoryStream(LZ77_WII.Decompress10(sub.ToArray(), (int)decomp_size));
                }
                else if (type == 0x11)
                {
                    uint decomp_size = reader.ReadUInt32();

                    var sub = new SubStream(stream, 16);
                    return new MemoryStream(LZ77_WII.Decompress11(sub.ToArray(), (int)decomp_size));
                }
                else
                {
                    return new MemoryStream();
                }
            }
        }

        public Stream Compress(Stream stream)
        {
            MemoryStream mem = new MemoryStream();

            return mem;
        }
    }
}
