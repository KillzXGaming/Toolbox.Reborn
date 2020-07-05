using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Core.Nitro
{
    //Ported from https://github.com/magcius/noclip.website/blob/e6b0b8d4f262b7eca5c0e04aaf7b76b35814bd0e/src/gfx/helpers/TopologyHelpers.ts
    //While I do have triangle conversion code, this helper does a better job at it
    public class TriangleHelper
    {
        public enum PolygonType
        {
            Triangle,
            TriangleFan,
            TriangleStrip,
            Quad,
            QuadStrip,
        }

        private static ushort[] FromRange(int start, int count)
        {
            ushort[] buffer = new ushort[count];
            for (int i = 0; i < count; i++)
                buffer[i] = (ushort)(start + i);
            return buffer;
        }

        public static ushort[] CreateIndexBuffer(PolygonType type, int baseVertex, int count) {
            return ConvertTrianglesIndexBuffer(type, FromRange(baseVertex, count));
        }

        public static ushort[] ConvertTrianglesIndexBuffer(PolygonType type, ushort[] indexBuffer)
        {
            if (type == PolygonType.Triangle)
                return indexBuffer;

            var newsize = GetTriangleIndexCount(type, indexBuffer.Length);
            ushort[] buffer = new ushort[newsize];
            ConvertTriangles(ref buffer, 0, type, indexBuffer);
            return buffer;
        }

        private static int GetTriangleIndexCount(PolygonType type, int count)
        {
            return 3 * GetSize(type, count);
        }

        private static int GetSize(PolygonType type, int count)
        {
            switch (type)
            {
                case PolygonType.Triangle:
                    return count / 3;
                case PolygonType.TriangleFan:
                case PolygonType.TriangleStrip:
                    return count - 2;
                case PolygonType.Quad:
                case PolygonType.QuadStrip:
                    return 2 * (count / 4);
            }
            return count / 3;
        }

        public static void ConvertTriangles(ref ushort[] dstBuffer, int dstOffs, PolygonType type, ushort[] indexBuffer)
        {
            int dst = dstOffs;
            if (type == PolygonType.Quad)
            {
                for (int i = 0; i < indexBuffer.Length; i += 4)
                {
                    dstBuffer[dst++] = indexBuffer[i + 0];
                    dstBuffer[dst++] = indexBuffer[i + 1];
                    dstBuffer[dst++] = indexBuffer[i + 2];
                    dstBuffer[dst++] = indexBuffer[i + 0];
                    dstBuffer[dst++] = indexBuffer[i + 2];
                    dstBuffer[dst++] = indexBuffer[i + 3];
                }
            }
            else if (type == PolygonType.TriangleStrip)
            {
                for (int i = 0; i < indexBuffer.Length - 2; i++)
                {
                    if (i % 2 == 0)
                    {
                        dstBuffer[dst++] = indexBuffer[i + 0];
                        dstBuffer[dst++] = indexBuffer[i + 1];
                        dstBuffer[dst++] = indexBuffer[i + 2];
                    }
                    else
                    {
                        dstBuffer[dst++] = indexBuffer[i + 1];
                        dstBuffer[dst++] = indexBuffer[i + 0];
                        dstBuffer[dst++] = indexBuffer[i + 2];
                    }
                }
            }
            else if (type == PolygonType.TriangleFan)
            {
                for (int i = 0; i < indexBuffer.Length - 2; i++)
                {
                    dstBuffer[dst++] = indexBuffer[0];
                    dstBuffer[dst++] = indexBuffer[i + 1];
                    dstBuffer[dst++] = indexBuffer[i + 2];
                }
            }
            else if (type == PolygonType.QuadStrip)
            {
                for (int i = 0; i < indexBuffer.Length - 2; i += 2)
                {
                    dstBuffer[dst++] = indexBuffer[i + 0];
                    dstBuffer[dst++] = indexBuffer[i + 1];
                    dstBuffer[dst++] = indexBuffer[i + 2];
                    dstBuffer[dst++] = indexBuffer[i + 2];
                    dstBuffer[dst++] = indexBuffer[i + 1];
                    dstBuffer[dst++] = indexBuffer[i + 3];
                }
            }
            else if (type == PolygonType.Triangle)
            {
                for (int i = 0; i < indexBuffer.Length; i++)
                    dstBuffer[dst++] = indexBuffer[i];
            }
        }
    }
}
