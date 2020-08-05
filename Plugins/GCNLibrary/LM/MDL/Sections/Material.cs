using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using Newtonsoft.Json;

namespace GCNLibrary.LM.MDL
{
    public class Material : ISection
    {
        public STColor8 Color { get; set; }
        public ushort Unknown1 { get; set; }
        public byte AlphaFlags { get; set; } //Value of 1 has alpha
        public byte NumTevStages { get; set; }
        public byte Unknown3 { get; set; }

        public byte[] Padding { get; set; } 

        public TevStage[] TevStages { get; set; }

        public Material()
        {
            TevStages = new TevStage[8];
            Color = STColor8.White;
            Unknown1 = 0;
            AlphaFlags = 0;
            NumTevStages = 1;
            Unknown3 = 0;
            Padding = new byte[23];

            for (int i = 0; i < 8; i++)
                TevStages[i] = new TevStage();
        }

        public void Read(FileReader reader)
        {
            Color = reader.ReadColor8RGBA();
            Unknown1 = reader.ReadUInt16();
            AlphaFlags = reader.ReadByte();
            NumTevStages = reader.ReadByte();
            Unknown3 = reader.ReadByte();
            Padding = reader.ReadBytes(23); //padding
            TevStages = new TevStage[8];
            for (int i = 0; i < 8; i++)
                TevStages[i] = new TevStage(reader);
        }

        public void Write(FileWriter writer)
        {
            writer.Write(Color);
            writer.Write(Unknown1);
            writer.Write(AlphaFlags);
            writer.Write(NumTevStages);
            writer.Write(Unknown3);
            writer.Write(Padding);

            for (int i = 0; i < 8; i++)
                TevStages[i].Write(writer);
        }
    }
}
