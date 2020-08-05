using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace GCNLibrary.LM.MDL
{
    public class TevStage 
    {
        public ushort Unknown { get; set; }
        public ushort SamplerIndex { get; set; }
        public float[] Unknowns2 { get; set; }

        public TevStage()
        {
            Unknown = 0;
            SamplerIndex = ushort.MaxValue;
            Unknowns2 = new float[7];
        }

        public TevStage(FileReader reader)
        {
            Unknown = reader.ReadUInt16();
            SamplerIndex = reader.ReadUInt16();
            Unknowns2 = reader.ReadSingles(7);
        }

        public void Write(FileWriter writer)
        {
            writer.Write(Unknown);
            writer.Write(SamplerIndex);
            writer.Write(Unknowns2);
        }
    }
}
