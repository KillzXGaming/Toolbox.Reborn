using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core.IO;

namespace GCNLibrary.WW
{
    /// <summary>
    /// Scene data that stores placements for objects.
    /// </summary>
    public class SceneData
    {
        /// <summary>
        /// A group of objects. 
        /// A single group determines what type of object to use.
        /// The type of group is hardcoded per map.
        /// </summary>
        public class Group
        {
            public List<Object> Objects = new List<Object>();
        }

        public List<Group> Groups = new List<Group>();  

        public SceneData(Stream stream)
        {
            using (var reader = new FileReader(stream)) {
                Read(reader);
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                Write(writer);
            }
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            uint numGroups = reader.ReadUInt32();
            uint groupsOffset = reader.ReadUInt32() * 4;

            for (int i = 0; i < numGroups; i++)
            {
                reader.SeekBegin(groupsOffset + (i * 8));
                uint numObjects = reader.ReadUInt32();
                uint objectsOffset = reader.ReadUInt32() * 4;

                Group group = new Group();
                reader.SeekBegin(objectsOffset);
                for (int j = 0; j < numObjects; j++)
                    group.Objects.Add(new Object(reader));
                Groups.Add(group);
            }
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);
            writer.Write(Groups.Count);
            writer.Write(2); //Always 2 (offset at 0x8, times 4)
            for (int i = 0; i < Groups.Count; i++)
            {
                writer.Write(Groups[i].Objects.Count);
                writer.Write(0);
            }
            for (int i = 0; i < Groups.Count; i++) {
                writer.WriteOffset(12 + (i * 8));

                foreach (var obj in Groups[i].Objects)
                    obj.Write(writer);
            }
        }

        /// <summary>
        /// Determines the placement of a map object.
        /// </summary>
        public class Object
        {
            public Matrix4 Transform { get; set; }
            public ushort GroupID { get; set; }
            public ushort UnknownID { get; set; }
            public ushort Unknown2ID { get; set; }
            public ushort Unknown3ID { get; set; }
            public ushort Unknown4ID { get; set; }
            public ushort Unknown5ID { get; set; }
            public ushort Unknown6ID { get; set; }
            public ushort Unknown7ID { get; set; }

            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }

            public Object()
            {

            }

            public Object(FileReader reader)
            {
                float[] matrix = reader.ReadSingles(12);
                GroupID = reader.ReadUInt16();
                UnknownID = reader.ReadUInt16();
                Unknown2ID = reader.ReadUInt16();
                Unknown3ID = reader.ReadUInt16();
                Unknown4ID = reader.ReadUInt16();
                Unknown5ID = reader.ReadUInt16();
                Unknown6ID = reader.ReadUInt16();
                Unknown7ID = reader.ReadUInt16();

                Position = new Vector3(matrix[3], matrix[7], matrix[11]);
                Rotation = new Vector3(matrix[0], matrix[4], matrix[8]);

                //Matrix 3x4 turn into matrix 4x4
                Transform = new Matrix4()
                {
                    Row0 = new Vector4(matrix[0], matrix[1], matrix[2], matrix[3]),
                    Row1 = new Vector4(matrix[4], matrix[5], matrix[6], matrix[7]),
                    Row2 = new Vector4(matrix[8], matrix[9], matrix[10], matrix[11]),
                    Row3 = new Vector4(0, 0, 0, 1),
                };
            }

            public void Write(FileWriter writer)
            {
                writer.Write(Transform.Row0);
                writer.Write(Transform.Row1);
                writer.Write(Transform.Row2);
                writer.Write(GroupID);
                writer.Write(UnknownID);
                writer.Write(Unknown2ID);
                writer.Write(Unknown3ID);
                writer.Write(Unknown4ID);
                writer.Write(Unknown5ID);
                writer.Write(Unknown6ID);
                writer.Write(Unknown7ID);
            }
        }
    }
}
