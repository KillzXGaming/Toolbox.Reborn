using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toolbox.Core.Nitro
{
    //All ported from NOCLIP
    //https://github.com/magcius/noclip.website/blob/e5c302ff52ad72429e5d0dc64062420546010831/src/SuperMario64DS/nitro_gx.ts
    public class NitroGX
    {
        enum CmdType
        {
            MTX_PUSH = 0x12,
            MTX_POP = 0x13,
            MTX_RESTORE = 0x14,
            MTX_IDENTITY = 0x15,
            MTX_4x4 = 0x16,
            MTX_4x3 = 0x17,
            MTX_4x4_MUL = 0x18,
            MTX_4x3_MUL = 0x19,
            MTX_3x3_MUL = 0x1A,
            SCALE = 0x1B,
            TRANSLATE = 0x1C,

            COLOR = 0x20,
            NORMAL = 0x21,
            TEXCOORD = 0x22,
            VTX_16 = 0x23,
            VTX_10 = 0x24,
            VTX_XY = 0x25,
            VTX_XZ = 0x26,
            VTX_YZ = 0x27,
            VTX_DIFF = 0x28,
            POLYGON_ATTR = 0x29,
            TEX_IMAGE_PARAMS = 0x2A,
            TEX_PALETTE_BASE = 0x2B,

            DIF_AMB = 0x30,
            MATERIAL_COLOR1 = 0x31,
            LIGHT_COLOR0 = 0x32,
            LIGHT_COLOR1 = 0x33,
            SHININESS = 0x34,

            BEGIN_VTXS = 0x40,
            END_VTXS = 0x41,

            SWAP_BUFFERS = 0x50,
            VIEWPORT = 0x60,
            BOX_TEST = 0x70,
            POSITON_TEST = 0x71,
            VECTOR_TEST = 0x72,
        }

        public enum PolyType
        {
            TRIANGLES = 0,
            QUADS = 1,
            TRIANGLE_STRIP = 2,
            QUAD_STRIP = 3,
        }

        // 3 pos + 4 color + 2 uv + 3 nrm + 1 matrix ID
        const uint  VERTEX_SIZE = 13;
        const uint VERTEX_BYTES = VERTEX_SIZE * 4;

        static void Cmd_Mtx_Restore(Context ctx) {
            ctx.s_MtxID = ctx.readParam();
        }

        static Vector3 bgr5(uint pixel) {
            byte[] data = new byte[3];
            NitroTex.bgr5(data, 0, (int)pixel);
            return new Vector3(data[0],data[1],data[2]);
        }

        static void Cmd_Color(Context ctx) {
            uint param = ctx.readParam();
            ctx.sColor = bgr5(param);
        }

        static void Cmd_Normal(Context ctx) {
            int param = (int)ctx.readParam();
            float X = ((short)(((param >> 0) & 0x3FF) << 6) >> 6) / 512f;
            float Y = ((short)(((param >> 10) & 0x3FF) << 6) >> 6) / 512f;
            float Z = ((short)(((param >> 20) & 0x3FF) << 6) >> 6) / 512f;
            ctx.sNormal = new Vector3(X, Y, Z);
        }

        static void Cmd_TexCoord(Context ctx) {
            uint param = ctx.readParam();
            var s = ((short)((param >> 0) & 0xFFFF)) / 16f;
            var t = ((short)((param >> 16) & 0xFFFF)) / 16f;
            ctx.sTexCoord = new Vector2(s, t);
        }

        static void Cmd_Vertex_16(Context ctx) {
            uint param1 = ctx.readParam();
            uint param2 = ctx.readParam();
            float X = ((short)((param1 >> 0) & 0xFFFF)) / 4096f;
            float Y = ((short)((param1 >> 16) & 0xFFFF)) / 4096f;
            float Z = ((short)((param2 >> 0) & 0xFFFF)) / 4096f;
            ctx.sPosition = new Vector3(X,Y,Z);
            ctx.PushVertex();
        }

        static void Cmd_Vertex_10(Context ctx) {
            uint param = ctx.readParam();

            float X = ((short)(((param >> 0) & 0x3FF) << 6) >> 6) / 64.0f;
            float Y = ((short)(((param >> 10) & 0x3FF) << 6) >> 6) / 64.0f;
            float Z = ((short)(((param >> 20) & 0x3FF) << 6) >> 6) / 64.0f;
            ctx.sPosition = new Vector3(X,Y,Z);
            ctx.PushVertex();
        }

        static void Cmd_Vertex_XY(Context ctx) {
            uint param = ctx.readParam();
            float X = ((short)((param >> 0) & 0xFFFF)) / 4096f;
            float Y = ((short)((param >> 16) & 0xFFFF)) / 4096f;

            ctx.sPosition = new Vector3(X, Y, ctx.sPosition.Z);
            ctx.PushVertex();
        }

        static void Cmd_Vertex_XZ(Context ctx) {
            uint param = ctx.readParam();
            float X = ((short)((param >> 0) & 0xFFFF)) / 4096f;
            float Z = ((short)((param >> 16) & 0xFFFF)) / 4096f;

            ctx.sPosition = new Vector3(X, ctx.sPosition.Y, Z);
            ctx.PushVertex();
        }

        static void Cmd_Vertex_YZ(Context ctx) {
            uint param = ctx.readParam();
            float Y = ((short)((param >> 0) & 0xFFFF)) / 4096f;
            float Z = ((short)((param >> 16) & 0xFFFF)) / 4096f;

            ctx.sPosition = new Vector3(ctx.sPosition.X, Y, Z);
            ctx.PushVertex();
        }

        static void Cmd_Vertex_DIFF(Context ctx) {
            uint param = ctx.readParam();

            float X = ((short)(((param >> 0) & 0x3FF) << 6) >> 6) / 4096f;
            float Y = ((short)(((param >> 10) & 0x3FF) << 6) >> 6) / 4096f;
            float Z = ((short)(((param >> 20) & 0x3FF) << 6) >> 6) / 4096f;
            ctx.sPosition = new Vector3(X, Y, Z) + ctx.sPosition;
            ctx.PushVertex();
        }

        static void Cmd_Dif_Amb(Context ctx) {
            uint param = ctx.readParam();
        }

        static void Cmd_Begin_Vtxs(Context ctx) {
            uint param = ctx.readParam();
            var polyType = param & 0x03;
            ctx.sPolyType = (PolyType)polyType;
            ctx.sVertexStartIndex = ctx.vertices.Count;
        }

        static void Cmd_End_Vtxs(Context ctx) {
            var baseVertex = ctx.sVertexStartIndex;
            var numVertices = ctx.vertices.Count - baseVertex;

            var indexBuffer = TriangleHelper.CreateIndexBuffer(
                ConvertType(ctx.sPolyType), baseVertex, numVertices);
            ctx.PushIndcies(indexBuffer);
        }

       static TriangleHelper.PolygonType ConvertType(PolyType type)
        {
            switch (type)
            {
                case PolyType.QUADS: return TriangleHelper.PolygonType.Quad;
                case PolyType.QUAD_STRIP: return TriangleHelper.PolygonType.QuadStrip;
                case PolyType.TRIANGLES: return TriangleHelper.PolygonType.Triangle;
                case PolyType.TRIANGLE_STRIP: return TriangleHelper.PolygonType.TriangleStrip;
            }
            return TriangleHelper.PolygonType.Triangle;
        }

        private static void RunCMD(Context ctx, byte cmd)
        {
            switch ((CmdType)cmd)
            {
                case 0: return;
                case CmdType.MTX_RESTORE: Cmd_Mtx_Restore(ctx); break;
                case CmdType.COLOR: Cmd_Color(ctx); break;
                case CmdType.NORMAL: Cmd_Normal(ctx); break;
                case CmdType.TEXCOORD: Cmd_TexCoord(ctx); break;
                case CmdType.VTX_16: Cmd_Vertex_16(ctx); break;
                case CmdType.VTX_10: Cmd_Vertex_10(ctx); break;
                case CmdType.VTX_XY: Cmd_Vertex_XY(ctx); break;
                case CmdType.VTX_XZ: Cmd_Vertex_XZ(ctx); break;
                case CmdType.VTX_YZ: Cmd_Vertex_YZ(ctx); break;
                case CmdType.VTX_DIFF: Cmd_Vertex_DIFF(ctx); break;
                case CmdType.DIF_AMB: Cmd_Dif_Amb(ctx); break;
                case CmdType.BEGIN_VTXS: Cmd_Begin_Vtxs(ctx); break;
                case CmdType.END_VTXS: Cmd_End_Vtxs(ctx); break;
                default:
                    Console.WriteLine($"Missing command! {cmd}");
                    break;
            }
        }

        public static Context ReadCmds(byte[] buffer)
        {
            Context ctx = new Context(buffer);
            while (ctx.offset < buffer.Length)
            {
                //Commands packed 4 at a time
                byte cmd1 = buffer[ctx.offset++];
                byte cmd2 = buffer[ctx.offset++];
                byte cmd3 = buffer[ctx.offset++];
                byte cmd4 = buffer[ctx.offset++];

                RunCMD(ctx, cmd1);
                RunCMD(ctx, cmd2);
                RunCMD(ctx, cmd3);
                RunCMD(ctx, cmd4);
            }
            return ctx;
        }

        public class Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Color;
            public float Alpha;
            public Vector2 TexCoord;
            public int mtxID;
        }

        public class Context
        {
            internal uint offset;
            internal uint s_MtxID;

            internal byte[] buffer;

            internal int sVertexStartIndex = 0;
            internal PolyType sPolyType;

            internal Vector3 sPosition;
            internal Vector3 sNormal;
            internal Vector2 sTexCoord;
            internal Vector3 sColor;
            internal float Alpha;
            internal int sMtxID = 0;

            public List<Vertex> vertices = new List<Vertex>();
            public List<ushort> indices = new List<ushort>();

            public Context(byte[] buf) {
                buffer = buf;
            }

            internal void PushVertex() {
                vertices.Add(new Vertex()
                {
                    Position = sPosition,
                    Normal = sNormal,
                    TexCoord = sTexCoord,
                    Color = sColor,
                    Alpha = Alpha,
                    mtxID = sMtxID,
                });
            }

            internal void PushIndcies(ushort[] faces) {
                foreach (var f in faces)
                    indices.AddRange(faces);
            }

            internal uint readParam() {

                uint param = buffer.GetUint(offset, false); 
                offset += 4;
                return param;
            }
        }
    }
}
