using System;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.GUI;
using Toolbox.Core.ModelView;

namespace GCNLibrary.WW
{
    public class LightingData : Controls.Panel, IEditorDisplay
    {
        [BindGUI("Color")]
        [BindCategory("Ambient")]
        public STColor8 AmbientColor { get; set; }

        [BindGUI()]
        public FogData Fog { get; set; }

        public LightData[] Lights { get; set; }

        public object PropertyDisplay => this;

        public LightingData(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        void Read(FileReader reader) {
            reader.SetByteOrder(true);
            uint fogOffset = reader.ReadUInt32() * 4;
            uint ambientColorOffset = reader.ReadUInt32() * 4;
            uint numLights = reader.ReadUInt32();
            uint lightsOffset = reader.ReadUInt32() * 4;

            reader.SeekBegin(ambientColorOffset);
            AmbientColor = reader.ReadColor8RGBA();

            reader.SeekBegin(fogOffset);
            Fog = new FogData();
            Fog.Unknown = reader.ReadUInt32();
            Fog.Start = reader.ReadSingle();
            Fog.End = reader.ReadSingle();
            Fog.Color = reader.ReadColor8RGBA();

            reader.SeekBegin(lightsOffset);
            Lights = new LightData[numLights];
            for (int i = 0; i < numLights; i++)
            {
                Lights[i] = new LightData()
                {
                    Values = reader.ReadSingles(12),
                    Color = reader.ReadColor8RGBA(),
                };
            }
        }

        void Write(FileWriter writer) {
            writer.SetByteOrder(true);
            writer.Write(5); //Always offset as 5
            writer.Write(4); //Always offset as 4
            writer.Write(Lights.Length);
            writer.Write(9); //Always offset as 9
            writer.Write(AmbientColor);
            writer.Write(Fog.Unknown);
            writer.Write(Fog.Start);
            writer.Write(Fog.End);
            writer.Write(Fog.Color);
            for (int i = 0; i < Lights.Length; i++) {
                writer.Write(Lights[i].Values);
                writer.Write(Lights[i].Color);
            }
        }
    }

    public class LightData : Controls.Panel, IEditorDisplay
    {
        [BindGUI("Lights")]
        [BindCategory("Color")]
        public STColor8 Color { get; set; }

        [BindGUI("Lights")]
        [BindCategory("Values")]
        public float[] Values { get; set; }

        public object PropertyDisplay => this;
    }

    public class FogData : ObjectTreeNode
    {
        [BindGUI("Color")]
        [BindCategory("Fog")]
        public STColor8 Color { get; set; }

        [BindGUI("Start")]
        [BindCategory("Fog")]
        public float Start { get; set; }

        [BindGUI("End")]
        [BindCategory("Fog")]
        public float End { get; set; }

        [BindGUI("Unknown")]
        [BindCategory("Fog")]
        public uint Unknown { get; set; }
    }
}
