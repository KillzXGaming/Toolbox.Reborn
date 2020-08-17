using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.GX
{
    public class GXModelHelper
    {
        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Binormals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();

        public List<Vector2[]> TexCoords = new List<Vector2[]>();
        public List<Vector4[]> Colors = new List<Vector4[]>();

        public List<GXEnvelope> Envelopes = new List<GXEnvelope>();

        public List<GXShape> Shapes = new List<GXShape>();
    }

    public class GXShape
    {
        public List<GXPacket> Packets = new List<GXPacket>();
    }

    public class GXPacket
    {
        public ushort[] MatrixIndices { get; set; }

        public List<GXDisplayList> DisplayList= new List<GXDisplayList>();
    }

    public class GXDisplayList
    {
        List<GXVertexLayout> layouts = new List<GXVertexLayout>();


    }

    public class GXEnvelope
    {
        public List<float> Weights = new List<float>();
        public List<ushort> BoneIndices = new List<ushort>();
    }
}
