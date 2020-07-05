using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace STLibrary.Forms.MapEditor
{
    public class DrawableSkybox : AbstractGlDrawable
    {
        public bool ForceDisplay = false;

        ShaderProgram defaultShaderProgram;

        private RenderableTex CustomCubemap = null;
        public void LoadCustomTexture(STGenericTexture GenericTexture)
        {
            if (GenericTexture.RenderableTex == null || !GenericTexture.RenderableTex.GLInitialized)
                GenericTexture.LoadOpenGLTexture();

            CustomCubemap = GenericTexture.RenderableTex;
        }

        static string VertexShader = @"
            #version 330 core
            layout (location = 0) in vec3 aPos;

            uniform mat4 projection;
            uniform mat4 rotView;
            uniform mat4 mtxCam;

            out vec3 TexCoords;

            void main()
            {
                TexCoords = aPos;
                vec4 clipPos = projection * rotView * vec4(aPos, 1.0);

                gl_Position = clipPos.xyww;
            }";

        static string FragmentShader = @"
            #version 330 core
            out vec4 FragColor;

            in vec3 TexCoords;
  
            uniform samplerCube environmentMap;
  
            void main()
            {
                vec3 envColor = textureLod(environmentMap, TexCoords, 0.0).rgb;

                envColor = envColor / (envColor + vec3(1.0));
                envColor = pow(envColor, vec3(1.0/2.2)); 

                FragColor = vec4(envColor, 1.0);
            }";

        public override void Prepare(GL_ControlModern control)
        {
            var defaultFrag = new FragmentShader(FragmentShader);
            var defaultVert = new VertexShader(VertexShader);

            defaultShaderProgram = new ShaderProgram(defaultFrag, defaultVert, control);
        }
        public override void Prepare(GL_ControlLegacy control)
        {

        }
        public override void Draw(GL_ControlLegacy control, Pass pass)
        {
            if (!Runtime.OpenTKInitialized || pass == Pass.TRANSPARENT)
                return;

            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);


        }


        RenderableTex specularPbr;
        public override void Draw(GL_ControlModern control, Pass pass)
        {
            if (!Runtime.OpenTKInitialized || pass != Pass.OPAQUE)
                return;

            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            //    GL.Enable(EnableCap.LineSmooth);

            //   GL.Enable(EnableCap.StencilTest);
            //  GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);


            control.CurrentShader = defaultShaderProgram;
            // enable seamless cubemap sampling for lower mip levels in the pre-filter map.
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            Matrix4 proj = control.ProjectionMatrix;
            //  Matrix4 rot = Matrix4.CreateFromQuaternion(control.ModelMatrix.ExtractRotation());
            Matrix4 rot = new Matrix4(new Matrix3(control.CameraMatrix));

            defaultShaderProgram.SetMatrix4x4("projection", ref proj);
            defaultShaderProgram.SetMatrix4x4("rotView", ref rot);

            if (CustomCubemap != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                CustomCubemap.Bind();
            }
            else
            {
              /*  if (Runtime.PBR.UseDiffuseSkyTexture)
                {
                    //Load Cubemap
                    if (RenderTools.diffusePbr != null)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        RenderTools.diffusePbr.Bind();
                    }
                }
                else
                {
                    //Load Cubemap
                    if (RenderTools.specularPbr != null)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        RenderTools.specularPbr.Bind();
                    }
                }*/
            }

            int cubeVBO = 0;

            if (cubeVBO == 0)
            {
                float[] vertices = {
            // back face
            -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f, // bottom-left
             1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f, // top-right
             1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f, // bottom-right         
             1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f, // top-right
            -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f, // bottom-left
            -1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f, // top-left
            // front face
            -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, // bottom-left
             1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f, // bottom-right
             1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, // top-right
             1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, // top-right
            -1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f, // top-left
            -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, // bottom-left
            // left face
            -1.0f,  1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-right
            -1.0f,  1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f, // top-left
            -1.0f, -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-left
            -1.0f, -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-left
            -1.0f, -1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f, // bottom-right
            -1.0f,  1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-right
            // right face
             1.0f,  1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-left
             1.0f, -1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-right
             1.0f,  1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f, // top-right         
             1.0f, -1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-right
             1.0f,  1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-left
             1.0f, -1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f, // bottom-left     
            // bottom face
            -1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f, // top-right
             1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f, // top-left
             1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, // bottom-left
             1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, // bottom-left
            -1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f, // bottom-right
            -1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f, // top-right
            // top face
            -1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f, // top-left
             1.0f,  1.0f , 1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, // bottom-right
             1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f, // top-right     
             1.0f,  1.0f,  1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, // bottom-right
            -1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f, // top-left
            -1.0f,  1.0f,  1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f  // bottom-left        
        };

                GL.GenVertexArrays(1, out cubeVBO);
                GL.GenBuffers(1, out cubeVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, 4 * vertices.Length, vertices, BufferUsageHint.StaticDraw);
                GL.BindVertexArray(cubeVBO);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

            }
            GL.BindVertexArray(cubeVBO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
    }
}
