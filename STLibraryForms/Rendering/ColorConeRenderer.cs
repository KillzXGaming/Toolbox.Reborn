using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;

namespace Toolbox.Core.Rendering
{
    public class ColorConeRenderer
    {
        public static ShaderProgram DefaultShaderProgram { get; private set; }

        private static VertexArrayObject sphereVao;

        private static Vertex[] Vertices;

        public static void Initialize(GL_ControlModern control)
        {
            if (DefaultShaderProgram != null && DefaultShaderProgram.programs.ContainsKey(control))
                return;

            if (DefaultShaderProgram == null)
            {
                var solidColorFrag = new FragmentShader(
                    @"#version 330
                uniform vec4 color;
                in vec3 normal;
                in vec3 viewPosition;
                void main(){
                    vec3 xTangent = dFdx( viewPosition );
                    vec3 yTangent = dFdy( viewPosition );
                    vec3 faceNormal = normalize( cross( xTangent, yTangent ) );

                    vec3 displayNormal = (faceNormal.xyz * 0.5) + 0.5;
		            float shading = max(displayNormal.y,0.5);

                    gl_FragColor = color;
                }");
                var solidColorVert = new VertexShader(
                    @"#version 330
                layout(location = 0) in vec3 position;
                layout(location = 1) in vec3 vNormal;
                uniform mat4 mtxMdl;
                uniform mat4 mtxCam;
                out vec3 normal;
                out vec3 viewPosition;
                void main(){
                    normal = vNormal;
                    viewPosition = position;
                    gl_Position = mtxCam*mtxMdl*vec4(position, 1);
                }");

                DefaultShaderProgram = new ShaderProgram(solidColorFrag, solidColorVert, control);

                int buffer = GL.GenBuffer();
                sphereVao = new VertexArrayObject(buffer);
                sphereVao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 24, 0);
                sphereVao.AddAttribute(1, 3, VertexAttribPointerType.Float, false, 24, 12);
                sphereVao.Initialize(control);

                List<float> list = new List<float>();
                Vertices = GetVertices(10, 2, 15, 32);
                for (int i = 0; i < Vertices.Length; i++)
                {
                    var mat = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
                    Vertices[i].Position = Vector3.TransformPosition(Vertices[i].Position, mat);
                    Vertices[i].Normal = Vector3.TransformNormal(Vertices[i].Normal, mat);

                    list.Add(Vertices[i].Position.X);
                    list.Add(Vertices[i].Position.Y);
                    list.Add(Vertices[i].Position.Z);
                    list.Add(Vertices[i].Normal.X);
                    list.Add(Vertices[i].Normal.Y);
                    list.Add(Vertices[i].Normal.Z);
                }

                float[] data = list.ToArray();
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
            }
            else
            {
                sphereVao.Initialize(control);
                DefaultShaderProgram.Link(control);
            }
        }

        private static Vertex[] GetVertices(float radiusBottom, float radiusTop, float height, float slices)
        {
            List<Vertex> vertices = new List<Vertex>();

            List<Vector3> discPointsBottom = new List<Vector3>();
            List<Vector3> discPointsTop = new List<Vector3>();

            float sliceArc = 360.0f / (float)slices;
            float angle = 0;
            for (int i = 0; i < slices; i++)
            {
                float x = radiusBottom * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                float z = radiusBottom * (float)Math.Sin(MathHelper.DegreesToRadians(angle));
                discPointsBottom.Add(new Vector3(x, 0, z));

                x = radiusTop * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                z = radiusTop * (float)Math.Sin(MathHelper.DegreesToRadians(angle));

                discPointsTop.Add(new Vector3(x, height, z));
                angle += sliceArc;
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p2 = discPointsBottom[i];
                Vector3 p1 = new Vector3(discPointsBottom[(i + 1) % discPointsBottom.Count]);

                vertices.Add(new Vertex() { Position = new Vector3(0, 0, 0) });
                vertices.Add(new Vertex() { Position = new Vector3(p2.X, 0, p2.Z) });
                vertices.Add(new Vertex() { Position = new Vector3(p1.X, 0, p1.Z) });

                p2 = discPointsTop[i % discPointsTop.Count];
                p1 = discPointsTop[(i + 1) % discPointsTop.Count];

                vertices.Add(new Vertex() { Position = new Vector3(0, height, 0) });
                vertices.Add(new Vertex() { Position = new Vector3(p1.X, height, p1.Z) });
                vertices.Add(new Vertex() { Position = new Vector3(p2.X, height, p2.Z) });
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p1 = discPointsBottom[i];
                Vector3 p2 = discPointsBottom[((i + 1) % discPointsBottom.Count())];
                Vector3 p3 = discPointsTop[i];
                Vector3 p4 = discPointsTop[(i + 1) % discPointsTop.Count()];

                vertices.Add(new Vertex() { Position = p1 });
                vertices.Add(new Vertex() { Position = p3 });
                vertices.Add(new Vertex() { Position = p4 });

                vertices.Add(new Vertex() { Position = p1 });
                vertices.Add(new Vertex() { Position = p4 });
                vertices.Add(new Vertex() { Position = p2 });
            }

            return vertices.ToArray();
        }

        public struct Vertex
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
        }

        public static void Draw(GL_ControlModern control, Pass pass,
            Vector4 sphereColor, Vector4 outlineColor, Vector4 pickingColor, bool xray = false)
        {
            Initialize(control);

            if (pass == Pass.OPAQUE && !xray || pass == Pass.TRANSPARENT && xray)
            {
                control.CurrentShader = DefaultShaderProgram;
                DefaultShaderProgram.SetVector4("color", sphereColor);

                sphereVao.Enable(control);
                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
            }
        }
    }
}
