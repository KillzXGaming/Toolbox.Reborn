using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.LM
{
    public class JMP_Parser
    {
        public bool IsBigEndian = false;

        public List<Field> Fields = new List<Field>();
        public List<Record> Records = new List<Record>();

        public JMP_Parser() { }

        public JMP_Parser(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            using (reader.TemporarySeek(8, SeekOrigin.Begin))
            {
                if (reader.ReadUInt32() != 76)
                    IsBigEndian = true;
            }

            reader.SetByteOrder(IsBigEndian);
            uint recordCount = reader.ReadUInt32();
            uint fieldCount = reader.ReadUInt32();
            uint recordOffset = reader.ReadUInt32();
            uint recordSize = reader.ReadUInt32();
            uint strTableOffset = recordOffset + (recordCount * recordSize);

            for (int i = 0; i < fieldCount; i++)
                Fields.Add(new Field(reader));

            for (int i = 0; i < recordCount; i++) {
                reader.SeekBegin(recordOffset + (i * recordSize));
                Records.Add(new Record(reader, Fields));
            }
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(IsBigEndian);
            writer.Write(Records.Count);
            writer.Write(Fields.Count);
            writer.Write(16 + (Fields.Count * 12));
            writer.Write(MaxFieldSize());

            for (int i = 0; i < Fields.Count; i++)
                Fields[i].Write(writer);
            for (int i = 0; i < Records.Count; i++)
                Records[i].Write(writer, Fields);
        }

        private uint MaxFieldSize()
        {
            uint size = uint.MinValue;
            for (int i = 0; i < Fields.Count; i++)
                size = Math.Min(size, Fields[i].GetDataSize());

            return AlignedSize(size, 4);
        }

        private uint AlignedSize(uint size, uint amount) {
            return ((size + amount - 1) % amount) *amount;
        }

        public class Field
        {
            /// <summary>
            /// Field Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Field Hash
            /// </summary>
            public uint Hash { get; set; }

            /// <summary>
            /// Field Bitmask
            /// </summary>
            public uint Bitmask { get; set; }

            /// <summary>
            /// Field Offset
            /// </summary>
            public ushort Offset { get; set; }

            /// <summary>
            /// Field Shift
            /// </summary>
            public sbyte Shift { get; set; }

            /// <summary>
            /// Field Type
            /// </summary>
            public FieldType Type { get; set; }

            public Field(FileReader reader)
            {
                Hash = reader.ReadUInt32();
                Bitmask = reader.ReadUInt32();
                Offset = reader.ReadUInt16();
                Shift = reader.ReadSByte();
                Type = (FieldType)reader.ReadByte();
                Name = JMPHashHelper.GetHashName(Hash);
            }

            public void Write(FileWriter writer)
            {
                writer.Write(Hash);
                writer.Write(Bitmask);
                writer.Write(Offset);
                writer.Write(Shift);
                writer.Write((byte)Type);
            }

            public uint GetDataSize()
            {
                switch (Type)
                {
                    case FieldType.Byte:   return 1;
                    case FieldType.Int16:  return 2;
                    case FieldType.Float:  return 4;
                    case FieldType.Int32:  return 4;
                    case FieldType.String: return 4;
                    default:
                      return 4;
                }
            }
        }

        public class Record
        {
            public object[] Values { get; set; }

            public Record(FileReader reader, List<Field> fields)
            {
                long pos = reader.Position;

                Values = new object[fields.Count];
                for (int i = 0; i < fields.Count; i++)
                {
                    reader.SeekBegin(pos + fields[i].Offset);
                    switch (fields[i].Type)
                    {
                        case FieldType.Int32:
                            Values[i] = (reader.ReadInt32() >> fields[i].Shift) & fields[i].Bitmask;
                            break;
                        case FieldType.Float:
                            Values[i] = reader.ReadSingle();
                            break;
                        case FieldType.String:
                            Values[i] = reader.ReadZeroTerminatedString();
                            break;
                        case FieldType.Int16:
                            Values[i] = (reader.ReadInt16() >> fields[i].Shift) & fields[i].Bitmask;
                            break;
                        case FieldType.Byte:
                            Values[i] = (reader.ReadByte() >> fields[i].Shift) & fields[i].Bitmask;
                            break;
                        case FieldType.StringJIS:
                            Values[i] = reader.ReadZeroTerminatedString(Encoding.GetEncoding("shift_jis"));
                            break;
                    }
                }
            }

           public void Write(FileWriter writer, List<Field> fields)
            {
                long pos = writer.Position;
                for (int i = 0; i < fields.Count; i++)
                {
                    writer.SeekBegin(pos + fields[i].Offset);
                    switch (fields[i].Type)
                    {
                        case FieldType.Int32:
                            writer.Write((uint)Values[i] << fields[i].Shift | fields[i].Bitmask);
                            break;
                        case FieldType.Float:
                            writer.Write((float)Values[i]);
                            break;
                        case FieldType.String:
                            writer.WriteString((string)Values[i]);
                            break;
                        case FieldType.Int16:
                            writer.Write((short)Values[i]);
                            break;
                        case FieldType.Byte:
                            writer.Write((byte)Values[i]);
                            break;
                        case FieldType.StringJIS:
                            writer.WriteString((string)Values[i], Encoding.GetEncoding("shift_jis"));
                            break;
                    }
                }
            }
        }

        public enum FieldType
        {
            Int32 = 0,
            String = 1,
            Float = 2,  
            Int16 = 4,
            Byte = 5,
            StringJIS = 6,
        }
    }
}
