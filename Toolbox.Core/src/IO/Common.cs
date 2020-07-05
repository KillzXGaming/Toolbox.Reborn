﻿using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Toolbox.Core.IO
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic
    {
        int value;
        public static implicit operator string(Magic magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
        public static implicit operator Magic(string s) => new Magic { value = BitConverter.ToInt32(Encoding.ASCII.GetBytes(s), 0) };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic8
    {
        long value;
        public static implicit operator string(Magic8 magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
        public static implicit operator Magic8(string s) => new Magic8 { value = BitConverter.ToInt64(Encoding.ASCII.GetBytes(s), 0) };
    }
}