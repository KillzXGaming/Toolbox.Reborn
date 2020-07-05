using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using GL_EditorFramework.EditorDrawables;
using OpenTK;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework;
using System.Drawing;

namespace Toolbox.Core.Rendering
{
    public class BonePoint : RenderablePathPoint
    {
        public STBone Bone { get; set; }

        public BonePoint(RenderableConnectedPath path, Vector4 color,
            Vector3 pos, Vector3 rot, Vector3 sca) : base(path, color, pos, rot, sca)
        {

        }
    }

    public class GenericSkeletonRenderer : RenderableConnectedPath
    {
        public override int LimitParentCount => 1;

        public ShaderProgram ShaderProgram;

        public bool CanPick = false;

        public float PointSize = 1;

        public STSkeleton Skeleton { get; set; }

        public GenericSkeletonRenderer(STSkeleton skeleton)
            : base(Color.Yellow, Color.Yellow, Color.Yellow)
        {
            Skeleton = skeleton;
            var points = new RenderablePathPoint[skeleton.Bones.Count];
            for (int i = 0; i < skeleton.Bones.Count; i++) {
                STBone bone = skeleton.Bones[i];
                points[i] = FromBone(bone);
            }
            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                if (skeleton.Bones[i].ParentIndex != -1)
                    points[skeleton.Bones[i].ParentIndex].AddChild(points[i]);
            }

            PathPoints = points.ToList();
        }

        private BonePoint FromBone(STBone bone)
        {
            return new BonePoint(this, ColorUtility.ToVector4(Color.Yellow),
                bone.Position, bone.EulerRotation, bone.Scale)
            { Bone = bone };
        }

        public override void Prepare(GL_ControlModern control)
        {
          /*  int[] buffers = new int[1];
            GL.GenBuffers(1, buffers);

            vaoBuffer = buffers[1];

            VertexArrayObject vao = new VertexArrayObject();
            vao.AddAttribute(0, 16, VertexAttribPointerType.Float, false, 16, 0);
            vao.Initialize(control);*/

            ShaderProgram = new ShaderProgram(
                new Shader(@"#version 330
                uniform vec4 boneColor;

                out vec4 FragColor;

                void main(){
                FragColor = boneColor;
                }", ShaderType.FragmentShader),
                new Shader(@"#version 330
                layout(location = 0) in vec4 point;

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
	                gl_Position =  mtxCam  * mtxMdl * vec4(position.xyz, 1);
                }", ShaderType.VertexShader), control);
        }

        public override void Prepare(GL_ControlLegacy control)
        {

        }

        int vbo_position;
        public void Destroy()
        {
            bool buffersWereInitialized = vbo_position != 0;
            if (!buffersWereInitialized)
                return;

            GL.DeleteBuffer(vbo_position);
        }

        private void CheckBuffers()
        {
            bool buffersWereInitialized = vbo_position != 0;
            if (!buffersWereInitialized)
            {
                GL.GenBuffers(1, out vbo_position);
                UpdateVertexData();
            }
        }

        private static Matrix4 prismRotation = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), 1.5708f);

        public void UpdateVertexData()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer,
                                   new IntPtr(Vertices.Length * Vector4.SizeInBytes),
                                   Vertices, BufferUsageHint.StaticDraw);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (pass != Pass.OPAQUE || ShaderProgram == null)
                return;

            if (!ObjectRenderState.ShouldBeDrawn(this))
                return;

            bool hovered = editorScene.Hovered == this;

            CheckBuffers();
             if (pass == Pass.PICKING && CanPick)
             {
                 int part = 0;
                 foreach (var drawable in PathPoints)
                 {
                     if (drawable.GetPickableSpan() == 0)
                         continue;

                     drawable.Color = ColorUtility.ToVector4(SphereColor);
                     drawable.Hovered = hovered && (editorScene.HoveredPart == part);
                     drawable.Draw(control, pass, editorScene);

                     part++;
                 }
                 return;
             }

            if (pass != Pass.PICKING)
            {
                control.CurrentShader = ShaderProgram;

                ShaderProgram.EnableVertexAttributes();
                ShaderProgram.SetMatrix4x4("rotation", ref prismRotation);

                int part = 0;

                Matrix4 modelMatrix = Matrix4.Identity;
                foreach (BonePoint pickObject in PathPoints)
                {
                    STBone bone = pickObject.Bone;

                    if (pickObject.GetPickableSpan() == 0)
                        continue;

                    pickObject.Hovered = hovered && (editorScene.HoveredPart == part);

                    var boneColor = ColorUtility.ToVector4(this.SphereColor);
                    if (pickObject.Hovered && pickObject.IsSelected())
                        boneColor = hoverSelectColor;
                    else if (pickObject.IsSelected())
                        boneColor = selectColor;
                    else if (pickObject.Hovered)
                        boneColor = hoverColor;

                    ShaderProgram.SetVector4("boneColor", boneColor);
                    ShaderProgram.SetFloat("scale", Runtime.BonePointSize * PointSize);
                    ShaderProgram.SetMatrix4x4("ModelMatrix", ref modelMatrix);

                    Matrix4 transform = bone.Transform;

                    ShaderProgram.SetMatrix4x4("bone", ref transform);
                    ShaderProgram.SetBoolToInt("hasParent", bone.ParentIndex != -1);

                    if (bone.ParentIndex != -1)
                    {
                        var transformParent = bone.Parent.Transform;
                        ShaderProgram.SetMatrix4x4("parent", ref transformParent);
                    }

                    Draw(ShaderProgram);
                    ShaderProgram.SetInt("hasParent", 0);
                    Draw(ShaderProgram);

                    part++;
                }

                ShaderProgram.DisableVertexAttributes();
                control.CurrentShader = null;

                GL.UseProgram(0);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);
            }
        }

        private void Draw(ShaderProgram shader)
        {
            Attributes(shader);

            GL.DrawArrays(PrimitiveType.Lines, 0, Vertices.Length);
        }

        private void Attributes(ShaderProgram shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(shader.GetAttribute("point"), 4, VertexAttribPointerType.Float, false, 16, 0);
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
