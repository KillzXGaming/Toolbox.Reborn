using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Toolbox.Core.IO;

namespace GCNLibrary.Pikmin1.Model
{
    public class Joint : IModChunk
    {
        public int ParentIndex;

        public bool UseVolume { get; set; }
        public bool FoundLightGroup { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public float VolumeRadius { get; set; }

        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }

        public List<PolygonGroup> PolygonGroups = new List<PolygonGroup>();

        private uint flags;

        public void Read(FileReader reader)
        {
            ParentIndex = reader.ReadInt32();
            flags = reader.ReadUInt32();
            ushort usingIdentifier = (ushort)flags;
            UseVolume = usingIdentifier > 0;
            FoundLightGroup = (usingIdentifier & 0x4000) != 0;
            BoundingBox = new BoundingBox()
            {
                Min = reader.ReadVec3(),
                Max = reader.ReadVec3(),
            };
            VolumeRadius = reader.ReadSingle();
            Scale = reader.ReadVec3();
            Rotation = reader.ReadVec3();
            Position = reader.ReadVec3();

            uint numMatPolys = reader.ReadUInt32();
            for (int i = 0; i < numMatPolys; i++)
            {
                PolygonGroups.Add(new PolygonGroup()
                {
                    MaterialIndex = reader.ReadUInt16(),
                    ShapeIndex = reader.ReadUInt16(),
                });
                Console.WriteLine($"PolygonGroups {PolygonGroups[i].MaterialIndex} {PolygonGroups[i].ShapeIndex}");
            }
        }

        public void Write(FileWriter writer)
        {
            writer.Write(ParentIndex);
            writer.Write(flags);
            writer.Write(BoundingBox.Min);
            writer.Write(BoundingBox.Max);
            writer.Write(VolumeRadius);
            writer.Write(Scale);
            writer.Write(Rotation);
            writer.Write(Position);
            foreach (var matPoly in PolygonGroups)
            {
                writer.Write(matPoly.MaterialIndex);
                writer.Write(matPoly.ShapeIndex);
            }
        }
    }
}
