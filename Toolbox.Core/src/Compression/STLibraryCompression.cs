using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Toolbox.Core.IO
{
    /// <summary>
    /// Helper class to easily get supported compression formats.
    /// </summary>
    public class STLibraryCompression
    {
        public class ZLIB
        {
            public static byte[] Decompress(byte[] b, bool hasMagic = true)
            {
                using (var br = new FileReader(new MemoryStream(b), true))
                {
                    if (br.ReadString(4) == "ZCMP")
                    {
                        return DecompressZCMP(b);
                    }
                    else
                    {
                        var ms = new System.IO.MemoryStream();
                        if (hasMagic)
                        {
                            br.Position = 2;
                            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 6)), CompressionMode.Decompress))
                                ds.CopyTo(ms);
                        }
                        else
                        {
                            using (var ds = new DeflateStream(new MemoryStream(b), CompressionMode.Decompress))
                                ds.CopyTo(ms);
                        }

                        return ms.ToArray();
                    }
                }
            }

            public static Byte[] DecompressZCMP(byte[] b)
            {
                using (var br = new FileReader(new MemoryStream(b), true))
                {
                    var ms = new System.IO.MemoryStream();
                    br.BaseStream.Position = 130;
                    using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 80)), CompressionMode.Decompress))
                        ds.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            public static byte[] Compress(byte[] b, uint Position = 0)
            {
                var output = new MemoryStream();
                output.Write(new byte[] { 0x78, 0xDA }, 0, 2);

                using (var zipStream = new DeflateStream(output, CompressionMode.Compress, true))
                    zipStream.Write(b, 0, b.Length);

                //Add this as it weirdly prevents the data getting corrupted
                //From https://github.com/IcySon55/Kuriimu/blob/f670c2719affc1eaef8b4c40e40985881247acc7/src/Kontract/Compression/ZLib.cs
                var adler = b.Aggregate(Tuple.Create(1, 0), (x, n) => Tuple.Create((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
                output.Write(new[] { (byte)(adler.Item2 >> 8), (byte)adler.Item2, (byte)(adler.Item1 >> 8), (byte)adler.Item1 }, 0, 4);
                return output.ToArray();
            }

            public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
            {
                byte[] buffer = new byte[2000];
                int len;
                while ((len = input.Read(buffer, 0, 2000)) > 0)
                {
                    output.Write(buffer, 0, len);
                }
                output.Flush();
            }
        }


        public class ZLIB_GZ
        {
            public static bool IsCompressed(Stream stream)
            {
                if (stream.Length < 32) return false;

                using (var reader = new FileReader(stream, true))
                {
                    reader.Position = 0;
                    ushort check = reader.ReadUInt16();
                    reader.ReadUInt16();
                    if (check != 0)
                        reader.SetByteOrder(true);
                    else
                        reader.SetByteOrder(false);

                    uint chunkCount = reader.ReadUInt32();
                    uint decompressedSize = reader.ReadUInt32();
                    if (reader.BaseStream.Length > 8 + (chunkCount * 4) + 128)
                    {
                        uint[] chunkSizes = reader.ReadUInt32s((int)chunkCount);
                        reader.Align(128);

                        //Now search for zlibbed chunks
                        uint size = reader.ReadUInt32();
                        ushort magic = reader.ReadUInt16();

                        reader.Position = 0;
                        if (magic == 0x78da || magic == 0xda78)
                            return true;
                        else
                            return false;
                    }

                    reader.Position = 0;
                }

                return false;
            }

            public static Stream Decompress(Stream stream)
            {
                using (var reader = new FileReader(stream, true))
                {
                    ushort check = reader.ReadUInt16();
                    reader.ReadUInt16();
                    if (check != 0)
                        reader.SetByteOrder(true);
                    else
                        reader.SetByteOrder(false);

                    try
                    {
                        uint chunkCount = reader.ReadUInt32();
                        uint decompressedSize = reader.ReadUInt32();
                        uint[] chunkSizes = reader.ReadUInt32s((int)chunkCount); //Not very sure about this

                        reader.Align(128);

                        List<byte[]> DecompressedChunks = new List<byte[]>();

                        Console.WriteLine($"pos {reader.Position}");

                        //Now search for zlibbed chunks
                        while (!reader.EndOfStream)
                        {
                            uint size = reader.ReadUInt32();

                            long pos = reader.Position;
                            ushort magic = reader.ReadUInt16();

                            ///Check zlib magic
                            if (magic == 0x78da || magic == 0xda78)
                            {
                                var data = STLibraryCompression.ZLIB.Decompress(reader.getSection((uint)pos, size));
                                DecompressedChunks.Add(data);

                                reader.SeekBegin(pos + size); //Seek the compressed size and align it to goto the next chunk
                                reader.Align(128);
                            }
                            else //If the magic check fails, seek back 2. This shouldn't happen, but just incase
                                reader.Seek(-2);
                        }

                        //Return the decompressed stream with all chunks combined
                        return new MemoryStream(ByteUtils.CombineArray(DecompressedChunks.ToArray()));
                    }
                    catch
                    {

                    }
                }

                return null;
            }

            public static Stream Compress(Stream stream, bool isBigEndian = true)
            {
                uint decompSize = (uint)stream.Length;
                uint[] section_sizes;
                uint sectionCount = 0;

                var mem = new MemoryStream();
                using (var reader = new FileReader(stream, true))
                using (var writer = new FileWriter(mem, true))
                {
                    writer.SetByteOrder(isBigEndian);

                    if (!(decompSize % 0x10000 != 0))
                        sectionCount = decompSize / 0x10000;
                    else
                        sectionCount = (decompSize / 0x10000) + 1;

                    writer.Write(0x10000);
                    writer.Write(sectionCount);
                    writer.Write(decompSize);
                    writer.Write(new uint[sectionCount]);
                    writer.Align(128);

                    reader.SeekBegin(0);
                    section_sizes = new uint[sectionCount];
                    for (int i = 0; i < sectionCount; i++)
                    {
                        byte[] chunk = STLibraryCompression.ZLIB.Compress(reader.ReadBytes(0x10000));

                        section_sizes[i] = (uint)chunk.Length;

                        writer.Write(chunk.Length);
                        writer.Write(chunk);
                        writer.Align(128);
                    }

                    writer.SeekBegin(12);
                    for (int i = 0; i < sectionCount; i++)
                        writer.Write(section_sizes[i] + 4);
                }
                return mem;
            }
        }
    }
}
