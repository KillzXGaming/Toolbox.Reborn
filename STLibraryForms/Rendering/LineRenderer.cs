using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;

namespace Toolbox.Core.Rendering
{
    public class LineRenderer : AbstractGlDrawable
    {
        public float Width = 4;

        public Color Color = Color.White;

        ShaderProgram defaultShaderProgram;

        VertexArrayObject vao;

        Vector3[] Vertices;
        public void UpdateVertexData(GLControl control, List<Vector3> points, Vector3 color)
        {
            Vertices = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                Vertices[i] = points[i];
            }

            List<float> list = new List<float>();
            for (int i = 0; i < Vertices.Length; i++)
            {
                list.Add(Vertices[i].X);
                list.Add(Vertices[i].Y);
                list.Add(Vertices[i].Z);
            }

            float[] data = list.ToArray();
            vao.Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
        }

        public override void Draw(GL_ControlModern control, Pass pass)
        {
            if (defaultShaderProgram == null)
                return;

            control.CurrentShader = defaultShaderProgram;
            control.UpdateModelMatrix(Matrix4.Identity);
            defaultShaderProgram.SetVector4("color", ColorUtility.ToVector4(Color));

            GL.LineWidth(Width);
            vao.Enable(control);
            if (Color.A != 255)
            {
                if (pass == Pass.TRANSPARENT)
                {
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.Blend);

                    vao.Use(control);
                    GL.DrawArrays(PrimitiveType.Lines, 0, Vertices.Length);
                }
            }
            else
            {
                vao.Use(control);
                GL.DrawArrays(PrimitiveType.Lines, 0, Vertices.Length);
            }
            GL.LineWidth(1f);
            GL.Disable(EnableCap.Blend);
        }

        public override void Draw(GL_ControlLegacy control, Pass pass)
        {
            if (pass == Pass.TRANSPARENT)
                return;
        }

        public override void Prepare(GL_ControlModern control)
        {
            var solidColorFrag = new FragmentShader(
                         @"#version 330
				uniform vec4 color;
                out vec4 FragColor;

				void main(){
					FragColor = color;
				}");
            var solidColorVert = new VertexShader(
              @"#version 330
				in vec3 vPosition;

	            uniform mat4 mtxMdl;
				uniform mat4 mtxCam;

				void main(){
					gl_Position = mtxMdl * mtxCam * vec4(vPosition.xyz, 1);
				}");

            defaultShaderProgram = new ShaderProgram(solidColorFrag, solidColorVert, control);

            int buffer = GL.GenBuffer();
            vao = new VertexArrayObject(buffer);
            vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 12, 0);
            vao.Initialize(control);
        }

        public override void Prepare(GL_ControlLegacy control)
        {

        }
    }
}
