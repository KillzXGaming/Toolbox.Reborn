using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using Toolbox.Core;

namespace Toolbox.Core.Rendering
{
    public class ColorPlaneRenderer
    {
        public static ShaderProgram DefaultShaderProgram { get; private set; }
        public static ShaderProgram SolidColorShaderProgram { get; private set; }

        public STGenericTexture Texture;

        public float Scale = 1;

        public Pass PlanePass = Pass.TRANSPARENT;

        private static VertexArrayObject sphereVao;

        private static Vertex[] Vertices;

        public static void Initialize(GL_ControlModern control, float scale)
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
                uniform vec4 highlight_color;
                uniform sampler2D texture0;

                in vec3 viewPosition;
                in vec3 normal;
                in vec2 f_texcoord0;

                out vec4 fragColor;

                void main(){
                    vec4 color = texture(texture0,f_texcoord0);

                    float hc_a   = highlight_color.w;
                    vec4 colorComb = vec4(color.rgb * (1-hc_a) + highlight_color.rgb * hc_a, color.a);

                    vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                    float halfLambert = max(displayNormal.y,0.5);

                    vec4 colorOutput = vec4(colorComb.rgb * halfLambert, colorComb.a);
                    fragColor = colorOutput;
                }");
                var defaultVert = new VertexShader(
                    @"#version 330
                layout(location = 0) in vec3 position;
                layout(location = 1) in vec3 vNormal;
                layout(location = 2) in vec2 vTexCoord;

                uniform mat4 mtxMdl;
                uniform mat4 mtxCam;
                out vec3 normal;
                out vec3 viewPosition;
                out vec2 f_texcoord0;

                void main(){
                    normal = vNormal;
                    viewPosition = position;
                    f_texcoord0 = vTexCoord;
                    gl_Position = mtxCam*mtxMdl*vec4(position, 1);
                }");

                DefaultShaderProgram = new ShaderProgram(defaultFrag, defaultVert, control);
                SolidColorShaderProgram = new ShaderProgram(solidColorFrag, solidColorVert, control);

                int buffer = GL.GenBuffer();
                sphereVao = new VertexArrayObject(buffer);
                sphereVao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 32, 0);
                sphereVao.AddAttribute(1, 3, VertexAttribPointerType.Float, false, 32, 12);
                sphereVao.AddAttribute(2, 2, VertexAttribPointerType.Float, false, 32, 24);
                sphereVao.Initialize(control);

                List<float> list = new List<float>();
                Vertices = GetVertices(scale);
                for (int i = 0; i < Vertices.Length; i++)
                {
                    list.Add(Vertices[i].Position.X);
                    list.Add(Vertices[i].Position.Y);
                    list.Add(Vertices[i].Position.Z);
                    list.Add(Vertices[i].Normal.X);
                    list.Add(Vertices[i].Normal.Y);
                    list.Add(Vertices[i].Normal.Z);
                    list.Add(Vertices[i].TexCoord.X);
                    list.Add(Vertices[i].TexCoord.Y);
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

        private static Vertex[] GetVertices(float scale)
        {
            Vector3[] positions = new Vector3[4]
            {
                new Vector3(-1,0,1),
                new Vector3(1,0,1),
                new Vector3(1,0,-1),
                new Vector3(-1,0,-1),
            };
            Vector3[] normals = new Vector3[4]
            {
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
            };
            Vector2[] texCoords = new Vector2[] {
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),
                new Vector2(0,0)
            };

            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < 4; i++)
                vertices.Add(new Vertex()
                {
                    Position = positions[i],
                    Normal = normals[i],
                    TexCoord = texCoords[i],
                });
            return vertices.ToArray();
        }

        public struct Vertex
        {
            public Vector3 Position { get; set; }
            public Vector2 TexCoord { get; set; }
            public Vector3 Normal { get; set; }
        }

        public void Draw(GL_ControlModern control, Pass pass,
            Vector4 sphereColor, Vector4 outlineColor)
        {
            Initialize(control, Scale);

            if (pass == Pass.TRANSPARENT)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
            }

            sphereVao.Enable(control);

            if (pass == Pass.PICKING)
            {
                control.CurrentShader = SolidColorShaderProgram;
                control.CurrentShader.SetVector4("color", control.NextPickingColor());

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.Quads, 0, Vertices.Length);
                return;
            }
            else
            {
            }

            if (pass == Pass.OPAQUE && outlineColor.W != 0)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.Clear(ClearBufferMask.StencilBufferBit);
                GL.ClearStencil(0);
                GL.StencilFunc(StencilFunction.Always, 0x1, 0x1);
                GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
            }

            if (pass == PlanePass)
            {
                control.CurrentShader = DefaultShaderProgram;
                DefaultShaderProgram.SetVector4("highlight_color", sphereColor);
                if (Texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + 1);
                    DefaultShaderProgram.SetInt("texture0", 1);
                    if (Texture.RenderableTex == null || !Texture.RenderableTex.GLInitialized)
                        Texture.LoadOpenGLTexture();

                    GL.BindTexture(TextureTarget.Texture2D, Texture.RenderableTex.TexID);
                }

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.Quads, 0, Vertices.Length);
            }

            if (pass == Pass.OPAQUE && outlineColor.W != 0)
            {
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);

                sphereVao.Use(control);
                GL.DrawArrays(PrimitiveType.Quads, 0, Vertices.Length);

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
                GL.DrawArrays(PrimitiveType.Quads, 0, Vertices.Length);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

                //GL.DepthFunc(DepthFunction.Lequal);

                GL.Disable(EnableCap.StencilTest);
                GL.LineWidth(2);
            }
        }
    }
}
