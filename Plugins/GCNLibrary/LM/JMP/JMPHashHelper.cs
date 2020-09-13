using System;
using System.Collections.Generic;
using System.Text;

namespace GCNLibrary.LM
{
    public class JMPHashHelper
    {
        static Dictionary<uint, string> _hashList = new Dictionary<uint, string>();
        static Dictionary<uint, string> HashList
        {
            get
            {
                if (_hashList.Count == 0)
                    _hashList = CalculateHashes();
                return _hashList;
            }
        }

        static string Hashes => Properties.Resources.Hashes;

        public static string GetHashName(uint hash)
        {
            if (HashList.ContainsKey(hash)) return HashList[hash];

            return hash.ToString();
        }

        private static Dictionary<uint, string> CalculateHashes()
        {
            Dictionary<uint, string> hashes = new Dictionary<uint, string>();

            string[] lines = Hashes.Split(
                        new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.None
                    );

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].StartsWith("#")) {
                    continue;
                }

                uint hash = Calculate(lines[i]);
                if (!hashes.ContainsKey(hash))
                    hashes.Add(hash, lines[i]);

                hash = CalculateV2(lines[i]);
                if (!hashes.ContainsKey(hash))
                    hashes.Add(hash, lines[i]);
            }

            return hashes;
        }

        private static uint Calculate(string str) {
            return Calculate(Encoding.ASCII.GetBytes(str));
        }

        private static uint CalculateV2(string str) {
            return CalculateV2(Encoding.ASCII.GetBytes(str));
        }

        private static uint Calculate(byte[] data)
        {
            uint hash = 0;
            for (var i = 0; i < data.Length; ++i)
            {
                hash <<= 8;
                hash += data[i];
                var r6 = unchecked((uint)((4993ul * hash) >> 32));
                var r0 = unchecked((byte)((((hash - r6) / 2) + r6) >> 24));
                hash -= r0 * 33554393u;
            }
            return hash;
        }

        private static uint CalculateV2(byte[] data)
        {
            uint hash = 0;
            for (var i = 0; i < data.Length; ++i) {
                hash = (hash * 31 + data[i]) & 0xFFFFFFFF;
            }
            return hash;
        }
    }
}
