using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.OpenGL;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;

namespace Toolbox.Core.Rendering
{
    public class GenericModelRender : GenericRenderer
    {
        public ModelRenderer Render;

        private GL_EditorFramework.GL_Core.ShaderProgram pickingShader;
        public override GL_EditorFramework.GL_Core.ShaderProgram PickingShader => pickingShader;

        public GenericModelRender(ModelRenderer render) {
            CanPick = false;
            Render = render;
            ReloadMeshes();
        }


        private static string VertexShaderPicking = @"
            #version 330

            layout(location = 0) in vec3 vPosition;
            layout(location = 1) in vec3 vNormal;
            layout(location = 2) in vec2 vTexCoord;
            layout(location = 3) in vec4 vColor;
            layout(location = 4) in vec4 vBone;
            layout(location = 5) in vec4 vWeight;
            layout(location = 6) in vec4 vTangent;
            layout(location = 7) in vec4 vBinormal;

            uniform mat4 mtxMdl;
            uniform mat4 mtxCam;

            // Skinning uniforms
            uniform mat4 bones[200];

            // Bone Weight Display
            uniform sampler2D weightRamp1;
            uniform sampler2D weightRamp2;
            uniform int selectedBoneIndex;
            uniform int debugOption;

            uniform int RigidSkinning;
            uniform int SingleBoneIndex;
            uniform int NoSkinning;

            out vec2 f_texcoord0;
            out vec4 vertexColor;
            out vec3 normal;
            out vec3 boneWeightsColored;
            out vec4 tangent;
            out vec4 binormal;

            vec4 skin(vec3 pos, ivec4 index)
            {
                vec4 newPosition = vec4(pos.xyz, 1.0);

                newPosition = bones[index.x] * vec4(pos, 1.0) * vWeight.x;
                newPosition += bones[index.y] * vec4(pos, 1.0) * vWeight.y;
                newPosition += bones[index.z] * vec4(pos, 1.0) * vWeight.z;
                if (vWeight.w < 1) //Necessary. Bones may scale weirdly without
		            newPosition += bones[index.w] * vec4(pos, 1.0) * vWeight.w;

                return newPosition;
            }

            vec3 skinNRM(vec3 nr, ivec4 index)
            {
                vec3 newNormal = vec3(0);

	            newNormal =  mat3(bones[index.x]) * nr * vWeight.x;
	            newNormal += mat3(bones[index.y]) * nr * vWeight.y;
	            newNormal += mat3(bones[index.z]) * nr * vWeight.z;
	            newNormal += mat3(bones[index.w]) * nr * vWeight.w;

                return newNormal;
            }

            vec3 BoneWeightColor(float weights)
            {
	            float rampInputLuminance = weights;
	            rampInputLuminance = clamp((rampInputLuminance), 0.001, 0.999);
                if (debugOption == 1) // Greyscale
                    return vec3(weights);
                else if (debugOption == 2) // Color 1
	               return texture(weightRamp1, vec2(1 - rampInputLuminance, 0.50)).rgb;
                else // Color 2
                    return texture(weightRamp2, vec2(1 - rampInputLuminance, 0.50)).rgb;
            }

            float BoneWeightDisplay(ivec4 index)
            {
                float weight = 0;
                if (selectedBoneIndex == index.x)
                    weight += vWeight.x;
                if (selectedBoneIndex == index.y)
                    weight += vWeight.y;
                if (selectedBoneIndex == index.z)
                    weight += vWeight.z;
                if (selectedBoneIndex == index.w)
                    weight += vWeight.w;

                if (selectedBoneIndex == index.x && RigidSkinning == 1)
                    weight = 1;
               if (selectedBoneIndex == SingleBoneIndex && NoSkinning == 1)
                    weight = 1;

                return weight;
            }

            void main(){
                f_texcoord0 = vTexCoord;
                vertexColor = vColor;
                normal = vNormal;

                ivec4 index = ivec4(vBone);
                normal = vNormal;
                tangent = vTangent;
                binormal = vBinormal;

                vec4 objPos = mtxMdl * vec4(vPosition.xyz, 1.0);
	            if (vBone.x != -1.0)
		            objPos = skin(objPos.xyz, index);
	            if(vBone.x != -1.0)
		            normal = normalize((skinNRM(vNormal.xyz, index)).xyz);

                gl_Position = mtxCam*objPos;

                float totalWeight = BoneWeightDisplay(index);
                boneWeightsColored = BoneWeightColor(totalWeight).rgb;
            }";

        private void SetupPickingShader(GL_ControlModern control)
        {
            //solid shader
            var solidColorFrag = new GL_EditorFramework.GL_Core.FragmentShader(
                @"#version 330
                        uniform vec4 color;
                        void main(){
                            gl_FragColor = color;
                        }");
            var solidColorVert = new GL_EditorFramework.GL_Core.VertexShader(
               VertexShaderPicking);

            pickingShader = new GL_EditorFramework.GL_Core.ShaderProgram(
                solidColorFrag, solidColorVert, control);
        }

        public void ReloadMeshes()
        {
            PickableMeshes.Clear();
            foreach (var mesh in Render.Meshes)
            {
                Pass pass = Pass.OPAQUE;
                if (mesh.Mesh.PolygonGroups.Any(x => x.IsTransparentPass))
                    pass = Pass.TRANSPARENT;

                PickableMeshes.Add(new GenericPickableMesh(mesh.Mesh)
                {
                    Render = mesh,
                    Pass = pass,
                });
            }
            Console.WriteLine($"PickableMeshes {PickableMeshes.Count}");
        }

        public override void Prepare(GL_ControlModern control)
        {
            Render.PrepareShaders();
            foreach (var mesh in PickableMeshes)
                mesh.Render.Initialize();
            SetupPickingShader(control);

            base.Prepare(control);
        }

     /*   public override void UpdateMeshMatrix(GL_ControlModern control,
                EditorSceneBase editorScene, Matrix4 modelMatrix, GenericPickableMesh mesh)
        {
            control.ResetModelMatrix();

            if (mesh.IsSelected())
                Console.WriteLine($"meshpos {mesh.GlobalPosition}");

            var matrix = mesh.GetMatrix(editorScene);
            Render.ShaderProgram.SetMatrix4x4("mtxMdl", ref matrix);
            control.CurrentShader.SetMatrix4x4("mtxMdl", ref matrix);
        }*/

        public override void OnRender(GL_ControlModern control, GL_EditorFramework.GL_Core.ShaderProgram shaderProgram, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
            if (pass != Pass.PICKING)
            {
                control.CurrentShader = null;
                Render.OnRender(new Camera()
                {
                    ModelMatrix = control.ModelMatrix,
                    ViewMatrix = control.CameraMatrix,
                    ProjectionMatrix = control.ProjectionMatrix
                }, highlightColor);
            }
            else
            {
                base.OnRender(control, shaderProgram, editorScene, pass, highlightColor);
            }
        }

        public override void ReloadUniforms(GL_ControlBase control, GL_EditorFramework.GL_Core.ShaderProgram shader)
        {
            int i = 0;
            foreach (var bone in Render.Model.Skeleton.Bones)
            {
                Matrix4 transform = bone.Inverse * bone.Transform;
                GL.UniformMatrix4(GL.GetUniformLocation(shader.programs[control], String.Format("bones[{0}]", i++)), false, ref transform);
            }
        }

        public override void RenderMaterial(GL_ControlBase control, Pass pass, GenericPickableMesh mesh,
            GL_EditorFramework.GL_Core.ShaderProgram shader, Vector4 highlight_color)
        {
            foreach (var group in mesh.Mesh.PolygonGroups)
                Render.RenderMaterials(Render.ActiveShader, mesh.Mesh, group, group.Material, highlight_color);
        }

        public override void DrawMesh(GL_ControlBase control, GenericPickableMesh mesh)
        {
            foreach (var group in mesh.Mesh.PolygonGroups)
                Render.OnMeshDraw(mesh.Render, group);
        }
    }
}
