using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;
using Toolbox.Core;
using OpenTK;

namespace GCNLibrary.LM.BIN
{
    public class SceneGraphNode
    {
        public int Index { get; set; }

        public short ParentIndex { get; set; }
        public short FirstChildIndex { get; set; }
        public short NextSiblingIndex { get; set; }
        public short PrevSiblingIndex { get; set; }

        public GraphObjectRenderFlags RenderFlags { get; set; }

        public Vector3 Scale { get; set; }
        public Vector3 Rotate { get; set; }
        public Vector3 Translate { get; set; }

        public Vector3 BoundingMin { get; set; }
        public Vector3 BoundingMax { get; set; }


        public float Unknown { get; set; }

        public SceneGraphNode Parent { get; set; }
        public List<SceneGraphNode> Children = new List<SceneGraphNode>();

        public List<DrawElement> DrawnParts = new List<DrawElement>();

        public SceneGraphNode(FileReader reader, int index, BIN_Parser header)
        {
            Index = index;
            ParentIndex = reader.ReadInt16();
            FirstChildIndex = reader.ReadInt16();
            NextSiblingIndex = reader.ReadInt16();
            PrevSiblingIndex = reader.ReadInt16();
            reader.ReadByte();
            RenderFlags = (GraphObjectRenderFlags)reader.ReadByte();
            reader.ReadInt16();
            Scale = reader.ReadVec3();
            Rotate = reader.ReadVec3();
            Translate = reader.ReadVec3();
            BoundingMin = reader.ReadVec3();
            BoundingMax = reader.ReadVec3();
            Unknown = reader.ReadSingle();
            ushort drawElementCount = reader.ReadUInt16();
            reader.ReadUInt16(); //padding
            uint drawElementOffset = reader.ReadUInt32();

            if (drawElementCount > 0) {
                for (int i = 0; i < drawElementCount; i++) {
                    reader.SeekBegin(header.SceneGraphOffset + drawElementOffset + (i * 4));
                    DrawnParts.Add(new DrawElement(reader, header));
                }
            }

            if (FirstChildIndex >= 0)
                AddChild(header.ReadSection<SceneGraphNode>(reader, FirstChildIndex));
            if (NextSiblingIndex >= 0)
            {
                if (Parent != null)
                    Parent.AddChild(header.ReadSection<SceneGraphNode>(reader, NextSiblingIndex));
                else
                    AddChild(header.ReadSection<SceneGraphNode>(reader, NextSiblingIndex));
            }
        }

        public void Write(FileWriter writer, BIN_Parser header)
        {
            writer.Write(ParentIndex);
            writer.Write(FirstChildIndex);
            writer.Write(NextSiblingIndex);
            writer.Write(PrevSiblingIndex);
            writer.Write((byte)0);
            writer.Write((byte)RenderFlags);
            writer.Write((ushort)0);
            writer.Write(Scale);
            writer.Write(Rotate);
            writer.Write(Translate);
            writer.Write(BoundingMin);
            writer.Write(BoundingMax);
            writer.Write(Unknown);
            writer.Write((ushort)DrawnParts.Count);
            writer.Write((ushort)0);
            writer.Write(0);
            writer.Write(new byte[56]);
        }

        public void AddChild(SceneGraphNode sceneGraph)
        {
            sceneGraph.Parent = this;
            Children.Add(sceneGraph);
        }

        public enum GraphObjectRenderFlags : byte
        {
            FourthWall = 0x04,              // invisible (except in GBH view)
            Transparent = 0x08,             // transparent (when luigi is behind it)?
            FullBright = 0x40,              // fullbright (ignore lighting)
            Ceiling = 0x80,                 // transparent/ceiling (will fade out when luigi vaccuums the floor)
        }
    }
}
