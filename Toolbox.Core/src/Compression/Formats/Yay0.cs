using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;
using Syroot.BinaryData;

namespace Toolbox.Core
{
    public class Yay0 : ICompressionFormat
    {
        public int Alignment = 0;

        public string[] Description { get; set; } = new string[] { "Yay0" };
        public string[] Extension { get; set; } = new string[] { "*.Yay0", "*.szp", };

        public override string ToString() { return "Yay0"; }

        public bool Identify(Stream stream, string fileName)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "Yay0");
            }
        }

        public bool CanCompress { get; } = true;

        public Stream Decompress(Stream stream) {
            return new MemoryStream(DecompressData(stream));
        }

        //Based on https://github.com/Daniel-McCarthy/Mr-Peeps-Compressor
        //License in Licenses/MIT.txt
        //Slightly adjusted to be faster
        public static byte[] DecompressData(Stream stream)
        {
            List<byte> output = new List<byte>();
            uint decompressedLength;
            uint compressedOffset;
            uint uncompressedOffset;
            using (var reader = new FileReader(stream, true))
            {
                reader.SetByteOrder(true);
                reader.ReadUInt32(); //Magic
                decompressedLength = reader.ReadUInt32();
                compressedOffset = reader.ReadUInt32();
                uncompressedOffset = reader.ReadUInt32();
            }

            int currentOffset;
            int readerPos = 16;

            byte[] data = stream.ReadAllBytes();
            while (output.Count < decompressedLength)
            {
                byte bits = data[readerPos++];
                BitArray arrayOfBits = new BitArray(new byte[1] { bits });

                for (int i = 7; i > -1 && (output.Count < decompressedLength); i--) //iterate through layout bits
                {
                    currentOffset = readerPos;
                    if (arrayOfBits[i] == true)
                    {
                        //non-compressed
                        //add one byte from uncompressedOffset to newFile
                        output.Add(data[uncompressedOffset++]);
                    }
                    else
                    {
                        //compressed
                        //read 2 bytes
                        //4 bits = length
                        //12 bits = offset

                        byte byte1 = data[compressedOffset++];
                        byte byte2 = data[compressedOffset++];

                        byte byte1Upper = (byte)((byte1 & 0x0F));
                        byte byte1Lower = (byte)((byte1 & 0xF0) >> 4);

                        int finalOffset = ((byte1Upper << 8) | byte2) + 1;
                        int finalLength;

                        if (byte1Lower == 0)
                        {
                            finalLength = data[uncompressedOffset] + 0x12;
                            uncompressedOffset++;
                        }
                        else
                        {
                            finalLength = byte1Lower + 2;
                        }

                        for (int j = 0; j < finalLength; j++) //add data for finalLength iterations
                        {
                            output.Add(output[output.Count - finalOffset]); //add byte at offset (fileSize - finalOffset) to file
                        }
                    }
                    readerPos = (int)currentOffset;
                }
            }

            return output.ToArray();
        }

        public Stream Compress(Stream stream) {
            return new MemoryStream(Compress(stream, ByteOrder.BigEndian));
        }

        public static byte[] Compress(Stream input, ByteOrder byteOrder)
        {
            using (var br = new FileReader(input))
            {
                var cap = 0x111;
                var sz = input.Length;

                var cmds = new List<byte>();
                var ctrl = new List<byte>();
                var raws = new List<byte>();

                var cmdpos = 0;
                cmds.Add(0);

                var pos = 0;
                byte flag = 0x80;

                while (pos < sz)
                {
                    var searched = Search(input, pos, sz, cap);
                    var hitp = searched.Item1;
                    var hitl = searched.Item2;

                    if (hitl < 3)
                    {
                        raws.Add(ScanBytes(br, pos)[0]);
                        cmds[cmdpos] |= flag;
                        pos += 1;
                    }
                    else
                    {
                        var searched2 = Search(input, pos + 1, sz, cap);
                        var tstp = searched.Item1;
                        var tstl = searched.Item2;

                        if (hitl + 1 < tstl)
                        {
                            raws.Add(ScanBytes(br, pos)[0]);
                            cmds[cmdpos] |= flag;
                            pos += 1;
                            flag >>= 1;
                            if (flag == 0)
                            {
                                flag = 0x80;
                                cmdpos = cmds.Count;
                                cmds.Add(0);
                            }

                            hitl = tstl;
                            hitp = tstp;
                        }

                        var e = pos - hitp - 1;
                        pos += hitl;

                        if (hitl < 0x12)
                        {
                            hitl -= 2;
                            ctrl.AddRange(BitConverter.GetBytes((ushort)((hitl << 12) | e)).Reverse());
                        }
                        else
                        {
                            ctrl.AddRange(BitConverter.GetBytes((ushort)e).Reverse());
                            raws.Add((byte)(hitl - 0x12));
                        }
                    }

                    flag >>= 1;
                    if (flag == 0)
                    {
                        flag = 0x80;
                        cmdpos = cmds.Count;
                        cmds.Add(0);
                    }
                }

                if (flag == 0x80)
                    cmds.RemoveAt(cmdpos);

                var v = 4 - (cmds.Count & 3);
                cmds.AddRange(new byte[v & 3]);
                var l = cmds.Count + 16;
                var o = ctrl.Count + l;

                List<byte> header = new List<byte>();
                header.AddRange(Encoding.ASCII.GetBytes("Yay0"));
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes((int)sz) : BitConverter.GetBytes((int)sz).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(l) : BitConverter.GetBytes(l).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(o) : BitConverter.GetBytes(o).Reverse());
                header.AddRange(cmds);
                header.AddRange(ctrl);
                header.AddRange(raws);

                return header.ToArray();
            }
        }

        // Windowsize = 0x1000
        // Length = 0x111
        static Item Search(Stream data, int pos, long sz, int cap)
        {
            using (var br = new FileReader(data, true))
            {
                var ml = Math.Min(cap, sz - pos);
                if (ml < 3)
                    return new Item(0, 0);

                var mp = Math.Max(0, pos - 0x1000);
                var hitp = 0;
                var hitl = 3;

                if (mp < pos)
                {
                    var hl = (int)FirstIndexOfNeedleInHaystack(ScanBytes(br, mp, (pos + hitl) - mp), ScanBytes(br, pos, hitl));
                    while (hl < pos - mp)
                    {
                        while (hitl < ml && ScanBytes(br, pos + hitl)[0] == ScanBytes(br, mp + hl + hitl)[0])
                            hitl++;

                        mp += hl;
                        hitp = mp;
                        if (hitl == ml)
                            return new Item(hitp, hitl);

                        mp++;
                        hitl++;
                        if (mp >= pos)
                            break;

                        hl = (int)FirstIndexOfNeedleInHaystack(ScanBytes(br, mp, (pos + hitl) - mp), ScanBytes(br, pos, hitl));
                    }
                }

                if (hitl < 4)
                    hitl = 1;

                hitl--;

                return new Item(hitp, hitl);
            }
        }

        class Item
        {
            public int Item1;
            public int Item2;

            public Item(int item1, int item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }

        static byte[] ScanBytes(FileReader reader, int pos, int length = 1)
        {
            var startOffset = reader.BaseStream.Position;

            if (pos + length >= reader.BaseStream.Length) length = (int)reader.BaseStream.Length - pos;
            if (pos < 0 || pos >= reader.BaseStream.Length) pos = length = 0;

            reader.BaseStream.Position = pos;
            var result = reader.ReadBytes(length);
            reader.BaseStream.Position = startOffset;

            return result;
        }

        // max haystack size = windowsize + maxMatchLength-1
        // Returns position of needle in haystack, only if whole needle matches somewhere in haystack
        // Partial matches with length < needleLength are ignored
        public static long FirstIndexOfNeedleInHaystack(byte[] haystack, byte[] needle)
        {
            var m = needle.Length;
            var n = haystack.Length;

            var badChar = new int[256];
            BadCharHeuristic(needle, m, badChar);

            var s = 0;
            while (s <= n - m)
            {
                var j = m - 1;
                while (j >= 0 && needle[j] == haystack[s + j])
                    j--;

                if (j < 0)
                    return s;

                s += Math.Max(1, j - badChar[haystack[s + j]]);
            }

            return -1;
        }

        private static void BadCharHeuristic(byte[] input, int size, int[] badChar)
        {
            for (var i = 0; i < 256; i++)
                badChar[i] = -1;

            for (var i = 0; i < size; i++)
                badChar[input[i]] = i;
        }
    }
}