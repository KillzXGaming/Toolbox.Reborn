using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a single vertex point capable of storing vertex information such as 
    /// position, normal, texture coordinates, and more.
    /// </summary>
    public class STVertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2[] TexCoords { get; set; }
        public Vector4[] Colors { get; set; }
        public Vector4 Tangent { get; set; }
        public Vector4 Bitangent { get; set; }

        public List<float> BoneWeights = new List<float>();
        public List<int> BoneIndices = new List<int>();
        public List<string> BoneNames = new List<string>();

        public STVertex()
        {
            TexCoords = new Vector2[0];
            Colors = new Vector4[0]; 
        }

        public VertexStruct ToStruct()
        {
            Vector2 texCoord = Vector2.Zero;
            Vector4 color = Vector4.Zero;
            Vector4 bones = Vector4.Zero;
            Vector4 weights = Vector4.Zero;
            if (TexCoords.Length > 0) texCoord = TexCoords[0];
            if (Colors.Length > 0) color = Colors[0];

            if (BoneIndices.Count > 0) bones.X = BoneIndices[0];
            if (BoneIndices.Count > 1) bones.Y = BoneIndices[1];
            if (BoneIndices.Count > 2) bones.Z = BoneIndices[2];
            if (BoneIndices.Count > 3) bones.W = BoneIndices[3];
            if (BoneWeights.Count > 0) weights.X = BoneWeights[0];
            if (BoneWeights.Count > 1) weights.Y = BoneWeights[1];
            if (BoneWeights.Count > 2) weights.Z = BoneWeights[2];
            if (BoneWeights.Count > 3) weights.W = BoneWeights[3];

            return new VertexStruct()
            {
                Position = Position,
                Normal = Normal,
                TexCoord0 = texCoord,
                Color0 = color,
                BoneIndices = bones,
                BoneWeights = weights,
            };
        }

        public float DistanceTo(STVertex vertex)
        {
            float deltaX = vertex.Position.X - Position.X;
            float deltaY = vertex.Position.Y - Position.Y;
            float deltaZ = vertex.Position.Z - Position.Z;

            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public void SortBoneIndices()
        {
            BoneIndices.Sort();
            if (BoneWeights.Count == BoneIndices.Count)
                BoneWeights.OrderBy(x => BoneIndices[BoneWeights.IndexOf(x)]);
        }
    }

    public struct VertexStruct
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TexCoord0 { get; set; }
        public Vector4 Color0 { get; set; }
        public Vector4 BoneWeights { get; set; }
        public Vector4 BoneIndices { get; set; }
    }

    /// <summary>
    /// Represents a bone weight used for a vertex
    /// storing bone and weight rigging information.
    /// </summary>
    public class BoneWeight
    {
        public string Bone { get; set; }
        public int Index { get; set; }
        public float Weight { get; set; }
    }
}
