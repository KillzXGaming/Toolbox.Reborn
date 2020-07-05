using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toolbox.Core.OpenGL
{
    public class SkeletonRenderer
    {
        private STGenericModel Model;
        private STSkeleton Skeleton => Model.Skeleton;

        public bool Visibile
        {
            get { return Skeleton.Visible; }
            set { Skeleton.Visible = value; }
        }

        Color boneColor = Color.FromArgb(255, 240, 240, 0);
        Color selectedBoneColor = Color.FromArgb(255, 240, 240, 240);

        int vbo_position;

        private static Matrix4 prismRotation = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), 1.5708f);

        public SkeletonRenderer(STGenericModel model)
        {
            Model = model;
        }

        ShaderProgram ShaderProgram;

        static string VertexShader = @"
            #version 330

            in vec4 point;

            uniform mat4 mtxCam;
            uniform mat4 mtxMdl;

            uniform mat4 bone;
            uniform mat4 parent;
            uniform mat4 rotation;
            uniform mat4 ModelMatrix;
            uniform int hasParent;
            uniform float scale;

            void main()
            {
                vec4 position = bone * rotation * vec4(point.xyz * scale, 1);
                if (hasParent == 1)
                {
                    if (point.w == 0)
                        position = parent * rotation * vec4(point.xyz * scale, 1);
                    else
                        position = bone * rotation * vec4((point.xyz - vec3(0, 1, 0)) * scale, 1);
                }
	            gl_Position =  mtxCam  * mtxMdl * ModelMatrix * vec4(position.xyz, 1);
            }";

        static string FragShader = @"
            #version 330
            uniform vec4 boneColor;

            out vec4 FragColor;

            void main(){
	            FragColor = boneColor;
            }";

        public void Prepare()
        {
            if (ShaderProgram != null)
                return;

            ShaderProgram = new ShaderProgram(
                new VertexShader(VertexShader),
                new FragmentShader(FragShader));
        }

        public void Render(Camera camera) {
            if (Skeleton == null || !Visibile || !Runtime.DisplayBones)
                return;

            CheckBuffers();

            if (!Runtime.OpenTKInitialized)
                return;

            Console.WriteLine($"Skeleton render {Skeleton.Bones.Count}");

            ShaderProgram.Enable();
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            ShaderProgram.EnableVertexAttributes();
            ShaderProgram.SetMatrix4x4("rotation", ref prismRotation);

            Matrix4 camMat = camera.ViewMatrix;
            Matrix4 mdlMat = camera.ModelMatrix;
            Matrix4 projMat = camera.ProjectionMatrix;
            Matrix4 computedCamMtx = camMat * projMat;

            ShaderProgram.SetMatrix4x4("mtxCam", ref computedCamMtx);
            ShaderProgram.SetMatrix4x4("mtxMdl", ref mdlMat);

            foreach (STBone bn in Skeleton.Bones)
            {
                if (!bn.Visible)
                    continue;

                Matrix4 modelMatrix = Matrix4.Identity;

                ShaderProgram.SetVector4("boneColor", ColorUtility.ToVector4(boneColor));
                ShaderProgram.SetFloat("scale", Runtime.BonePointSize * Skeleton.PreviewScale);
                ShaderProgram.SetMatrix4x4("ModelMatrix", ref modelMatrix);


                Matrix4 transform = bn.Transform;

                ShaderProgram.SetMatrix4x4("bone", ref transform);
                ShaderProgram.SetInt("hasParent", bn.ParentIndex != -1 ? 1 : 0);

                if (bn.ParentIndex != -1)
                {
                    var transformParent = ((STBone)bn.Parent).Transform;
                    ShaderProgram.SetMatrix4x4("parent", ref transformParent);
                }

                Draw(ShaderProgram);

                if (Runtime.SelectedBoneIndex == Skeleton.Bones.IndexOf(bn))
                    ShaderProgram.SetVector4("boneColor", ColorUtility.ToVector4(selectedBoneColor));

                ShaderProgram.SetInt("hasParent", 0);
                Draw(ShaderProgram);
            }

            ShaderProgram.DisableVertexAttributes();

            GL.UseProgram(0);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }

        private void Attributes(ShaderProgram shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(shader.GetAttribute("point"), 4, VertexAttribPointerType.Float, false, 16, 0);
        }

        private void Draw(ShaderProgram shader)
        {
            Attributes(shader);

            GL.DrawArrays(PrimitiveType.Lines, 0, Vertices.Length);
        }

        void Destroy()
        {
            bool buffersWereInitialized = vbo_position != 0;
            if (!buffersWereInitialized)
                return;

            GL.DeleteBuffer(vbo_position);
        }

        public void UpdateVertexData()
        {
            Prepare();

            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer,
                                   new IntPtr(Vertices.Length * Vector4.SizeInBytes),
                                   Vertices, BufferUsageHint.StaticDraw);
        }

        private void CheckBuffers()
        {
            if (!Runtime.OpenTKInitialized)
                return;

            bool buffersWereInitialized = vbo_position != 0;
            if (!buffersWereInitialized)
            {
                GL.GenBuffers(1, out vbo_position);
                UpdateVertexData();
            }
        }

        private static List<Vector4> screenPositions = new List<Vector4>()
        {
            // cube
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 0f, -1f, 0),

            //point top parentless
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 0),

            //point top
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 1),

            //point bottom
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, -1f, 0f, 0),
        };

        Vector4[] Vertices
        {
            get
            {
                return screenPositions.ToArray();
            }
        }
    }
}
