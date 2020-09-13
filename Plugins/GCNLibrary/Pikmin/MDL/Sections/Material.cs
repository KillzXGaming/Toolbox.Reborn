using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1.Model
{
    public class MaterialList
    {
        public List<Material> Materials = new List<Material>();
        public List<MaterialLight> MaterialLights = new List<MaterialLight>();

        public uint Flags { get; set; }

        public MaterialList(FileReader reader)
        {
            uint count = reader.ReadUInt32();
            uint lightCount = reader.ReadUInt32();
            byte[] padding = reader.ReadBytes(0x10);

            for (int i = 0; i < lightCount; i++)
                MaterialLights.Add(new MaterialLight(reader));

            for (int i = 0; i < count; i++)
                Materials.Add(new Material(reader));
        }

        public void Write(FileWriter writer)
        {
            writer.Write(Materials.Count);
            writer.Write(Flags);
            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Write(writer);
        }
    }

    public class MaterialLight
    {
        public MaterialColor Color0 { get; set; }
        public MaterialColor Color1 { get; set; }
        public MaterialColor Color2 { get; set; }

        public MaterialLight(FileReader reader) {
            Color0 = new MaterialColor(reader);
            Color1 = new MaterialColor(reader);
            Color2 = new MaterialColor(reader);
            reader.ReadBytes(16); //FFFFFFFFFF
            uint count = reader.ReadUInt32();
            for (int i = 0; i < count; i++) {
                reader.ReadBytes(32);
            }
        }
    }

    public class MaterialColor
    {
        public STColor16 Color { get; set; }
        public uint Unknown { get; set; }
        public float Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }

        public MaterialColor(FileReader reader)
        {
            Color = reader.ReadColor16RGBA();
            Unknown = reader.ReadUInt32(); //0
            Unknown2 = reader.ReadSingle(); //1
            Unknown3 = reader.ReadUInt32(); //0
            Unknown4 = reader.ReadUInt32(); //0
        }
    }

    public class Material
    {
        public short[] TextureAttributeIndices = new short[8] { -1, -1, -1, -1, -1, -1, -1, -1, };

        public STColor8 DiffuseColor { get; set; } = STColor8.White;

        public byte TextureFlags { get; set; }

        public Material(FileReader reader)
        {
            ushort unk = reader.ReadUInt16();
            TextureFlags = reader.ReadByte();
            ushort tevflags = reader.ReadByte();
            short textureIndex = reader.ReadInt16();
            reader.ReadByte();
            TextureAttributeIndices[0] = reader.ReadSByte();
            DiffuseColor = reader.ReadColor8RGBA();

            if (tevflags != 0) {
                reader.ReadBytes(64);
                uint numStages = reader.ReadUInt32();
                uint padding = reader.ReadUInt32();
                for (int i = 0; i < numStages; i++) {
                    reader.ReadBytes(68);
                }
            }
        }

        public void Write(FileWriter writer)
        {

        }
    }
}
