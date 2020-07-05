using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace GCNLibrary
{
    public class BNR_Parser
    {
        public List<U8_Parser.FileEntry> Files = new List<U8_Parser.FileEntry>();

        public BNR_Parser(System.IO.Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                reader.SeekBegin(64);
                reader.ReadSignature(4, "IMET");

                reader.SeekBegin(1536);
                U8 u8 = new U8();
                u8.Load(new SubStream(reader.BaseStream, reader.Position, reader.BaseStream.Length - 1536));
                foreach (U8_Parser.FileEntry file in u8.Files)
                {
                    file.SetData(ParseFileData(file.AsBytes()));
                    Files.Add(file);
                }
            }
        }

        public void Save(System.IO.Stream stream) {
        }

        private byte[] ParseFileData(byte[] data)
        {
            using (var reader = new FileReader(data))
            {
                reader.SetByteOrder(true);
                string magic = reader.ReadString(4, Encoding.ASCII);
                if (magic == "IMD5")
                {
                    uint fileSize = reader.ReadUInt32();
                    reader.Seek(8); //padding
                    byte[] md5Hash = reader.ReadBytes(16);
                    string compMagic = reader.ReadString(4, Encoding.ASCII);
                    reader.Position -= 4;

                    if (compMagic == "LZ77")
                        return LZ77_WII.Decompress(reader.ReadBytes((int)fileSize));
                    else
                        reader.ReadBytes((int)fileSize);
                }
                if (magic == "IMET")
                {

                }
                if (magic == "BNS")
                {

                }
            }
            return data;
        }
    }
}
