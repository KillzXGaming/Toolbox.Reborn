using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.LM.BIN
{
    public class DrawElement 
    {
        public short MaterialIndex { get; set; }
        public short ShapeBatchIndex { get; set; }

        public Material Material { get; set; }
        public ShapeBatch Batch { get; set; }

        public DrawElement() { }

        public DrawElement(FileReader reader, BIN_Parser header) {
            MaterialIndex = reader.ReadInt16();
            ShapeBatchIndex = reader.ReadInt16();

            Material = header.ReadSection<Material>(reader, MaterialIndex);
            Batch = header.ReadSection<ShapeBatch>(reader, ShapeBatchIndex);
        }

        public void Write(FileWriter writer, BIN_Parser header) {
            writer.Write(MaterialIndex);
            writer.Write(ShapeBatchIndex);
        }
    }
}
