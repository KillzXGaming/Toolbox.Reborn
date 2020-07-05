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
    public class ColorSphereRenderer
    {
        public static ShaderProgram DefaultShaderProgram { get; private set; }
        public static ShaderProgram SolidColorShaderProgram { get; private set; }

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
                        out vec4 fragColor;

                        void main(){
                            fragColor = color;
                        }");
                var solidColorVert = new VertexShader(
                    @"#version 330
                        layout(location = 0) in vec4 position;
                        uniform mat4 mtxMdl;
                        uniform mat4 mtxCam;
                        void main(){
                            gl_Position = mtxCam*mtxMdl*position;
                        }");

                var defaultFrag = new FragmentShader(
                    @"#version 330
                uniform vec4 color;
                in vec3 viewPosition;
                in vec3 normal;
                void main(){
                    gl_FragColor = color;
                }");
                var defaultVert = new VertexShader(
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

                DefaultShaderProgram = new ShaderProgram(defaultFrag, defaultVert, control);
                SolidColorShaderProgram = new ShaderProgram(solidColorFrag, solidColorVert, control);

                int buffer = GL.GenBuffer();
                sphereVao = new VertexArrayObject(buffer);
                sphereVao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 24, 0);
                sphereVao.AddAttribute(1, 3, VertexAttribPointerType.Float, false, 24, 12);
                sphereVao.Initialize(control);

                List<float> list = new List<float>();
                Vertices = GetVertices(1, 32);
                for (int i = 0; i < Vertices.Length; i++)
                {
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
                SolidColorShaderProgram.Link(control);
            }
        }

        private static Vertex[] GetVertices(float radius, float subdiv)
        {
            List<Vertex> vertices = new List<Vertex>();

            float halfPi = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / subdiv;
            float twoPiThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < subdiv / 2; j++)
            {
                theta1 = (j * twoPiThroughPrecision) - halfPi;
                theta2 = ((j + 1) * twoPiThroughPrecision) - halfPi;

                for (uint i = 0; i <= subdiv; i++)
                {
                    theta3 = i * twoPiThroughPrecision;

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new Vertex() { Position = pos, Normal = norm });

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new Vertex() { Position = pos, Normal = norm });
                }
            }

            return vertices.ToArray();
        }

        public struct Vertex
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
        }

        public static void Draw(GL_ControlModern control, Pass pass,
            Vector4 sphereColor, Vector4 outlineColor, bool xray = false)
        {
            if (DefaultShaderProgram == null)
                return;

            Initialize(control);

            sphereVao.Enable(control);

            if (pass == Pass.PICKING)
            {
                control.CurrentShader = SolidColorShaderProgram;
                control.CurrentShader.SetVector4("color", control.NextPickingColor());

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
                return;
            }

            if (pass == Pass.OPAQUE && outlineColor.W != 0)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.Clear(ClearBufferMask.StencilBufferBit);
                GL.ClearStencil(0);
                GL.StencilFunc(StencilFunction.Always, 0x1, 0x1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
            }

            if (pass == Pass.OPAQUE && !xray || pass == Pass.TRANSPARENT && xray)
            {
                control.CurrentShader = DefaultShaderProgram;
                DefaultShaderProgram.SetVector4("color", sphereColor);

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
            }

            if (pass == Pass.OPAQUE && outlineColor.W != 0)
            {
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);

                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);
            }

            GL.Disable(EnableCap.Blend);

            if (pass == Pass.OPAQUE && outlineColor.W != 0)
            {
                control.CurrentShader = SolidColorShaderProgram;
                control.CurrentShader.SetVector4("color", new Vector4(outlineColor.Xyz, 1));

                GL.LineWidth(3.0f);
                GL.StencilFunc(StencilFunction.Equal, 0x0, 0x1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

                //GL.DepthFunc(DepthFunction.Always);

                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

                //GL.DepthFunc(DepthFunction.Lequal);

                GL.Disable(EnableCap.StencilTest);
                GL.LineWidth(2);
            }
        }
    }
}
