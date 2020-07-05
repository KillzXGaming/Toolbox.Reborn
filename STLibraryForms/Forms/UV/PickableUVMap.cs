using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using STLibrary.Forms.OpenGL2D;

namespace STLibrary.Forms
{
    public class PickableUVMap : IPickable2DObject
    {
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }

        public bool IsHit(float x, float y)
        {
            //Calcuate the total uv bounds

            return false;
        }

        public void PickTranslate(float x, float y, float z)
        {

        }

        public void PickScale(float x, float y, float z)
        {

        }

        public void PickRotate(float x, float y, float z)
        {

        }

        public void CalculateBoundry()
        {
         
        }

        #region Draw UVs

        public void DrawUVs(int PolygonGroupIndex, int UvChannelIndex, List<STGenericMesh> genericObjects, STGenericTextureMap textureMap)
        {
            if (genericObjects.Count == 0) return;

            foreach (var genericObject in genericObjects)
            {
                int divisions = 4;
                int lineWidth = 1;

                Color uvColor = Runtime.UVEditor.UVColor;
                Color gridColor = Color.Black;

                List<uint> f = new List<uint>();
                int displayFaceSize = 0;
                if (genericObject.PolygonGroups.Count > 0)
                {
                    if (PolygonGroupIndex == -1)
                    {
                        foreach (var group in genericObject.PolygonGroups)
                        {
                            f.AddRange(group.Faces);
                            displayFaceSize += group.Faces.Count;
                        }
                    }
                    else
                    {
                        if (genericObject.PolygonGroups.Count > PolygonGroupIndex)
                        {
                            f = genericObject.PolygonGroups[PolygonGroupIndex].Faces;
                            displayFaceSize = genericObject.PolygonGroups[PolygonGroupIndex].Faces.Count;
                        }
                    }
                }

                for (int v = 0; v < displayFaceSize; v += 3)
                {
                    if (displayFaceSize < 3 || genericObject.Vertices.Count < 3)
                        return;

                    Vector2 v1 = new Vector2(0);
                    Vector2 v2 = new Vector2(0);
                    Vector2 v3 = new Vector2(0);

                    if (f.Count <= v + 2)
                        continue;

                    if (genericObject.Vertices.Count > f[v + 2])
                    {
                        v1 = genericObject.Vertices[(int)f[v]].TexCoords[UvChannelIndex];
                        v2 = genericObject.Vertices[(int)f[v + 1]].TexCoords[UvChannelIndex];
                        v3 = genericObject.Vertices[(int)f[v + 2]].TexCoords[UvChannelIndex];

                        v1 = new Vector2(v1.X, 1 - v1.Y);
                        v2 = new Vector2(v2.X, 1 - v2.Y);
                        v3 = new Vector2(v3.X, 1 - v3.Y);

                        DrawUVTriangleAndGrid(v1, v2, v3, divisions, uvColor, lineWidth, gridColor, textureMap);
                    }
                }
            }
        }
        private void DrawUVTriangleAndGrid(Vector2 v1, Vector2 v2, Vector2 v3, int divisions,
            Color uvColor, int lineWidth, Color gridColor, STGenericTextureMap textureMap)
        {
            GL.UseProgram(0);

            float bounds = 1;
            Vector2 scaleUv = new Vector2(2);
            Vector2 transUv = new Vector2(-1f);

            if (textureMap != null && textureMap.Transform != null)
            {
                scaleUv *= textureMap.Transform.Scale;
                transUv += textureMap.Transform.Translate;
            }

            //Disable textures so they don't affect color
            GL.Disable(EnableCap.Texture2D);
            DrawUvTriangle(v1, v2, v3, uvColor, scaleUv, transUv);

            // Draw Grid
            GL.Color3(gridColor);
            GL.LineWidth(1);

            //  DrawHorizontalGrid(divisions, bounds, scaleUv);
            // DrawVerticalGrid(divisions, bounds, scaleUv);
        }

        private static void DrawUvTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color uvColor, Vector2 scaleUv, Vector2 transUv)
        {
            GL.Color3(uvColor);
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(3);
            GL.Vertex2(v1 * scaleUv + transUv);
            GL.Vertex2(v2 * scaleUv + transUv);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(3);
            GL.Vertex2(v2 * scaleUv + transUv);
            GL.Vertex2(v3 * scaleUv + transUv);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(3);
            GL.Vertex2(v3 * scaleUv + transUv);
            GL.Vertex2(v1 * scaleUv + transUv);
            GL.End();
        }

        private static void DrawVerticalGrid(int divisions, float bounds, Vector2 scaleUv)
        {
            int verticalCount = divisions;
            for (int i = 0; i < verticalCount * bounds; i++)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(new Vector2((1.0f / verticalCount) * i, -bounds) * scaleUv);
                GL.Vertex2(new Vector2((1.0f / verticalCount) * i, bounds) * scaleUv);
                GL.End();
            }
        }

        private static void DrawHorizontalGrid(int divisions, float bounds, Vector2 scaleUv)
        {
            int horizontalCount = divisions;
            for (int i = 0; i < horizontalCount * bounds; i++)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(new Vector2(-bounds, (1.0f / horizontalCount) * i) * scaleUv);
                GL.Vertex2(new Vector2(bounds, (1.0f / horizontalCount) * i) * scaleUv);
                GL.End();
            }
        }

        #endregion

    }
}
