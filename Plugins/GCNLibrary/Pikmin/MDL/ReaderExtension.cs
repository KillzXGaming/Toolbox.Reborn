using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1.Model
{
    public static class ReaderExtension
    {
        public static void AlignPadding(this FileReader reader, int offset) {
            reader.Seek((~(offset - 1) & (reader.Position + offset - 1)) - reader.Position);
        }

        public static T[] ParsePrimitive<T>(this FileReader reader)
        {
            int count = reader.ReadInt32();

            AlignPadding(reader, 0x20);
            T[] values = new T[count];
            for (int i = 0; i < count; i++)
            {
                object value = null;
                if (typeof(T) == typeof(uint)) value = reader.ReadUInt32();
                else if (typeof(T) == typeof(int)) value = reader.ReadInt32();
                else if (typeof(T) == typeof(short)) value = reader.ReadInt16();
                else if (typeof(T) == typeof(ushort)) value = reader.ReadUInt16();
                else if (typeof(T) == typeof(float)) value = reader.ReadSingle();
                else if (typeof(T) == typeof(bool)) value = reader.ReadBoolean();
                else if (typeof(T) == typeof(sbyte)) value = reader.ReadSByte();
                else if (typeof(T) == typeof(byte)) value = reader.ReadByte();
                else if (typeof(T) == typeof(string)) {
                    uint size = reader.ReadUInt32();
                    value = reader.ReadString((int)size, true);
                }
                else
                    throw new Exception("Unsupported primitive type! " + typeof(T));

                values[i] = (T)value;
            }

            AlignPadding(reader, 0x20);
            return values;
        }

        public static T[] ParseArray<T>(this FileReader reader)
               where T : IModChunk, new()
        {
            int count = reader.ReadInt32();

            AlignPadding(reader, 0x20);
            T[] values = new T[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = new T();
                values[i].Read(reader);
            }

            AlignPadding(reader, 0x20);
            return values;
        }

        public static T[] ParseStructs<T>(this FileReader reader)
        {
            int count = reader.ReadInt32();

            AlignPadding(reader, 0x20);
            T[] values = reader.ReadMultipleStructs<T>(count).ToArray();
            AlignPadding(reader, 0x20);
            return values;
        }
    }
}
