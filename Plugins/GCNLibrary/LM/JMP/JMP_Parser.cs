using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.LM
{
    public class JMP_Parser
    {
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
            reader.SetByteOrder(true);
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
            writer.SetByteOrder(true);
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
