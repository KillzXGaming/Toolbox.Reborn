﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Toolbox.Core.IO;

namespace Toolbox.Core
{
    public class LZ77_WII
    {
        private readonly static int N = 4096;
        private readonly static ushort[] textBuffer = new ushort[N + 17];
        private readonly static int F = 18;
        private static readonly int threshold = 2;

        public static byte[] Decompress10(byte[] input, int decomp_size)
        {
            UInt32 leng = (uint)(input[1] | (input[2] << 8) | (input[3] << 16));
            byte[] Result = new byte[leng];
            int Offs = 4;
            int dstoffs = 0;
            while (true)
            {
                byte header = input[Offs++];
                for (int i = 0; i < 8; i++)
                {
                    if ((header & 0x80) == 0) Result[dstoffs++] = input[Offs++];
                    else
                    {
                        byte a = input[Offs++];
                        byte b = input[Offs++];
                        int offs = (((a & 0xF) << 8) | b) + 1;
                        int length = (a >> 4) + 3;
                        for (int j = 0; j < length; j++)
                        {
                            Result[dstoffs] = Result[dstoffs - offs];
                            dstoffs++;
                        }
                    }
                    if (dstoffs >= leng) return Result;
                    header <<= 1;
                }
            }
            return Result;
        }

        //Ported from 
        //https://github.com/mistydemeo/quickbms/blob/5752a6a2a38e16211952553fcffa59570855ac42/included/nintendo.c#L58
        // various code from DSDecmp: http://code.google.com/p/dsdecmp/
        // original code of unlz77wii_raw10 from "Hector Martin <marcan@marcansoft.com>" http://wiibrew.org/wiki/Wii.py
        // ported to C by Luigi Auriemma
        public static byte[] Decompress11(byte[] input, int decomp_size)
        {
            int i, j, disp = 0, len = 0, cdest;
            byte b1, bt, b2, b3, flags;
            int threshold = 1;
            bool flag = false;

            int inputOffset = 0;
            int curr_size = 0;

            byte[] outdata = new byte[decomp_size];

            while (curr_size < decomp_size)
            {
                if (inputOffset >= input.Length) break;

                flags = input[inputOffset++];
                for (i = 0; i < 8 && curr_size < decomp_size; i++)
                {
                    flag = (flags & (0x80 >> i)) > 0;
                    if (flag)
                    {
                        if (inputOffset > input.Length) break;
                        b1 = input[inputOffset++];

                        switch ((int)(b1 >> 4))
                        {
                            //#region case 0
                            case 0:
                                {
                                    // ab cd ef
                                    // =>
                                    // len = abc + 0x11 = bc + 0x11
                                    // disp = def

                                    len = b1 << 4;
                                    if (inputOffset >= input.Length) break;
                                    bt = input[inputOffset++];
                                    len |= bt >> 4;
                                    len += 0x11;

                                    disp = (bt & 0x0F) << 8;
                                    if (inputOffset > input.Length) break;
                                    b2 = input[inputOffset++];
                                    disp |= b2;
                                    break;
                                }
                            //#endregion

                            //#region case 1
                            case 1:
                                {
                                    // ab cd ef gh
                                    // => 
                                    // len = bcde + 0x111
                                    // disp = fgh
                                    // 10 04 92 3F => disp = 0x23F, len = 0x149 + 0x11 = 0x15A

                                    if (inputOffset + 3 > input.Length) break;
                                    bt = input[inputOffset++];
                                    b2 = input[inputOffset++];
                                    b3 = input[inputOffset++];

                                    len = (b1 & 0xF) << 12; // len = b000
                                    len |= bt << 4; // len = bcd0
                                    len |= (b2 >> 4); // len = bcde
                                    len += 0x111; // len = bcde + 0x111
                                    disp = (b2 & 0x0F) << 8; // disp = f
                                    disp |= b3; // disp = fgh
                                    break;
                                }
                            //#endregion

                            //#region other
                            default:
                                {
                                    // ab cd
                                    // =>
                                    // len = a + threshold = a + 1
                                    // disp = bcd

                                    len = (b1 >> 4) + threshold;

                                    disp = (b1 & 0x0F) << 8;
                                    if (inputOffset >= input.Length) break;

                                    b2 = input[inputOffset++];
                                    disp |= b2;
                                    break;
                                }
                                //#endregion
                        }

                        if (disp > curr_size)
                            return null;

                        cdest = curr_size;

                        for (j = 0; j < len && curr_size < decomp_size; j++)
                            outdata[curr_size++] = outdata[cdest - disp - 1 + j];

                        if (curr_size > decomp_size)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (inputOffset >= input.Length) break;
                        outdata[curr_size++] = input[inputOffset++];

                        if (curr_size > decomp_size)
                        {
                            break;
                        }
                    }
                }
            }
            return outdata;
        }


        public static byte[] Decompress(byte[] input, bool useMagic = true)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            using (var reader = new FileReader(new MemoryStream(input), true))
            {
                reader.SetByteOrder(true);
                reader.ReadSignature(4, "LZ77");

                reader.Position = 0;

                int i, j, k, r, c, z;
                uint flags, decompressedSize, currentSize = 0;

                byte[] temp = new byte[8];
                reader.Read(temp, 0, 8);

                if (temp[4] != 0x10)
                { reader.Dispose(); throw new Exception("Unsupported Compression Type!"); }

                decompressedSize = (BitConverter.ToUInt32(temp, 4)) >> 8;

                for (i = 0; i < N - F; i++) textBuffer[i] = 0xdf;
                r = N - F; flags = 7; z = 7;

                MemoryStream outFile = new MemoryStream();
                while (outFile.Length < decompressedSize)
                {
                    flags <<= 1;
                    z++;

                    if (z == 8)
                    {
                        if ((c = reader.ReadByte()) == -1) break;

                        flags = (uint)c;
                        z = 0;
                    }

                    if ((flags & 0x80) == 0)
                    {
                        if ((c = reader.ReadByte()) == reader.Length - 1) break;
                        if (currentSize < decompressedSize) outFile.WriteByte((byte)c);

                        textBuffer[r++] = (byte)c;
                        r &= (N - 1);
                        currentSize++;
                    }
                    else
                    {
                        if ((i = reader.ReadByte()) == -1) break;
                        if ((j = reader.ReadByte()) == -1) break;

                        j = j | ((i << 8) & 0xf00);
                        i = ((i >> 4) & 0x0f) + threshold;
                        for (k = 0; k <= i; k++)
                        {
                            c = textBuffer[(r - j - 1) & (N - 1)];
                            if (currentSize < decompressedSize) outFile.WriteByte((byte)c); textBuffer[r++] = (byte)c; r &= (N - 1); currentSize++;
                        }
                    }
                }
                return outFile.ToArray();
            }
            return input;
        }
    }
}
