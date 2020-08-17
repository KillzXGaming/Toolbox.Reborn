using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Toolbox.Core.OpenGL
{
    /// <summary>
    /// Represents a renderable model.
    /// </summary>
    public class ModelRenderer : IDisposable
    {
        public virtual bool SupportsTangents { get; set; }
        public virtual bool SupportsBinormals { get; set; }
        public virtual bool DisplayVertexColors { get; set; } = true;
        public virtual bool DisplayVertexColorAlpha { get; set; } = true;

        public virtual float PreviewScale { get; set; } = 1.0f;

        /// <summary>
        /// The main shader to use when rendering the model.
        /// </summary>
        public ShaderProgram ShaderProgram = null;

        /// <summary>
        /// The shader to use when rendering the model with debug rendering.
        /// </summary>
        public ShaderProgram ShaderProgramDebug = null;

        public ShaderProgram ActiveShader
        {
            get
            {
                if (Runtime.DebugRendering != Runtime.DebugRender.Default)
                    return ShaderProgramDebug;
                else
                    return ShaderProgram;
            }
        }

        public string Name
        {
            get
            {
                if (Scene.Name != null) return Scene.Name;
                else
                    return Scene.Models[0].Name;
            }
        }

        /// <summary>
        /// A list of meshes used for rendering.
        /// </summary>
        public List<MeshRender> Meshes = new List<MeshRender>();

        /// <summary>
        /// The scene object containing multiple models.
        /// </summary>
        public STGenericScene Scene;

        /// <summary>
        /// The skeleton renderer for rendering skeletons from the model.
        /// </summary>
        public SkeletonRenderer SkeletonRender;

        public ModelRenderer(STGenericScene scene) {
            Scene = scene;

            foreach (var model in Scene.Models)
            {
                foreach (var mesh in model.Meshes)
                    Meshes.Add(new MeshRender(mesh, this));

                SkeletonRender = new SkeletonRenderer(model);
            }
        }

        public ModelRenderer(STGenericModel model) {
            Scene = new STGenericScene();
            Scene.Models.Add(model);

            foreach (var mesh in model.Meshes)
                Meshes.Add(new MeshRender(mesh, this));

            SkeletonRender = new SkeletonRenderer(model);
        }

        public void OnRender(Camera camera, Vector4 highlightColor)
        {
            if (ShaderProgram == null)
                PrepareShaders();

            if (ShaderProgramDebug == null)
                PrepareDebugShaders();

            //SkeletonRender.Render(camera);

            Matrix4 mtxMdl = camera.ModelMatrix * Matrix4.CreateScale(PreviewScale);
            Matrix4 mtxCam = camera.ViewMatrix * camera.ProjectionMatrix;

            if (Runtime.DebugRendering != Runtime.DebugRender.Default)
            {
                ShaderProgramDebug.Enable();
                ShaderProgramDebug.SetVector4("highlight_color", highlightColor);
                ShaderProgramDebug.SetMatrix4x4("mtxMdl", ref mtxMdl);
                ShaderProgramDebug.SetMatrix4x4("mtxCam", ref mtxCam);
                ReloadUniforms(ShaderProgramDebug);
            }
            else
            {
                ShaderProgram.Enable();
                ShaderProgram.SetVector4("highlight_color", highlightColor);
                ShaderProgram.SetMatrix4x4("mtxMdl", ref mtxMdl);
                ShaderProgram.SetMatrix4x4("mtxCam", ref mtxCam);
                ReloadUniforms(ShaderProgram);
            }
        }

        public void Dispose()
        {

        }

        public virtual void ReloadUniforms(ShaderProgram shader) {
            SetDefaultUniforms(shader);
        }

        public void SetDefaultUniforms(ShaderProgram shader)
        {
            foreach (var model in Scene.Models) {
                SetBoneUniforms(shader, model.Skeleton);
                SetRenderSettings(shader);
            }
        }

        private void SetBoneUniforms(ShaderProgram shader, STSkeleton Skeleton)
        {
            int i = 0;
            foreach (var bone in Skeleton.Bones)
            {
                Matrix4 transform = bone.Inverse * bone.Transform;
                GL.UniformMatrix4(GL.GetUniformLocation(shader.program, String.Format("bones[{0}]", i++)), false, ref transform);
            }
        }

        private void SetRenderSettings(ShaderProgram shader)
        {
            shader.SetInt("renderType", (int)Runtime.DebugRendering);
            shader.SetBoolToInt("hasDiffuse", false);
            shader.SetInt("selectedBoneIndex", Runtime.SelectedBoneIndex);
            shader.SetBoolToInt("renderVertColor", DisplayVertexColors);
            shader.SetBoolToInt("renderVertAlpha", DisplayVertexColorAlpha);
            shader.SetColor("diffuseColor", STColor8.White.Color);
            shader.SetBoolToInt("HasSkeleton", false);
            if (Scene.Models.Count > 0)
                shader.SetBoolToInt("HasSkeleton", Scene.Models.Any(x => x.Skeleton.Bones.Count > 0));
        }

        public virtual void SetMaterialUniforms(ShaderProgram shader, STGenericMaterial material, STGenericMesh mesh) {
            shader.SetColor("diffuseColor", STColor8.White.Color);
            if (material == null) return;

            shader.SetColor("diffuseColor", material.DiffuseColor.Color);
        }

        public virtual void RenderMaterials(ShaderProgram shader, 
            STGenericMesh mesh,  STPolygonGroup group, STGenericMaterial material, Vector4 highlight_color)
        {
            foreach (var model in Scene.Models)
            {
                if (material == null && group.MaterialIndex != -1 && model.Materials.Count > group.MaterialIndex)
                    material = model.Materials[group.MaterialIndex];

                shader.SetVector4("highlight_color", highlight_color);

                SetTextureUniforms(shader);
                SetMaterialUniforms(shader, material, mesh);
                if (material == null) return;

                int textureUintID = 1;
                foreach (var textureMap in material.TextureMaps)
                {
                    var tex = textureMap.GetTexture();
                    if (textureMap.Type == STTextureType.Diffuse)
                    {
                        shader.SetBoolToInt("hasDiffuse", true);
                        BindTexture(shader, model.GetMappedTextures(), textureMap, textureUintID);
                        shader.SetInt($"tex_Diffuse", textureUintID);
                    }

                    textureUintID++;
                }
            }
        }

        public void SetTextureUniforms(ShaderProgram shader)
        {
            shader.SetInt("debugOption", 2);
            
            GL.ActiveTexture(TextureUnit.Texture11);
            shader.SetInt("weightRamp1", 11);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.BoneWeightGradient.RenderableTex.TexID);

            GL.ActiveTexture(TextureUnit.Texture12);
            shader.SetInt("weightRamp2", 12);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.BoneWeightGradient2.RenderableTex.TexID);


            GL.ActiveTexture(TextureUnit.Texture10);
            shader.SetInt("UVTestPattern", 10);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.uvTestPattern.RenderableTex.TexID);
        }

        public static bool BindTexture(ShaderProgram shader, List<STGenericTexture> textures,
               STGenericTextureMap texture, int id)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + id);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.RenderableTex.TexID);
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i].Name == texture.Name)
                    BindGLTexture(textures[i], texture, shader);
            }
            for (int i = 0; i < Runtime.TextureCache.Count; i++)
            {
                if (Runtime.TextureCache[i].Name == texture.Name)
                    BindGLTexture(Runtime.TextureCache[i], texture, shader);
            }

            return false;
        }

        private static void BindGLTexture(STGenericTexture texture, STGenericTextureMap matTex, ShaderProgram shader)
        {
            if (texture.RenderableTex == null || !texture.RenderableTex.GLInitialized)
                texture.LoadOpenGLTexture();

            //If the texture is still not initialized then return
            if (!texture.RenderableTex.GLInitialized)
                return;

            GL.BindTexture(TextureTarget.Texture2D, texture.RenderableTex.TexID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenGLHelper.WrapMode[matTex.WrapU]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenGLHelper.WrapMode[matTex.WrapV]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)OpenGLHelper.MinFilter[matTex.MinFilter]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)OpenGLHelper.MagFilter[matTex.MagFilter]);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 0.0f);
        }

        public virtual void OnMeshDraw(MeshRender msh, STPolygonGroup group)
        {
            PrimitiveType mode = PrimitiveType.Triangles;
            switch (group.PrimitiveType)
            {
                case STPrimitiveType.TriangleStrips:
                    mode = PrimitiveType.TriangleStrip;
                    break;
                case STPrimitiveType.TriangleFans:
                    mode = PrimitiveType.TriangleFan;
                    break;
                case STPrimitiveType.Lines:
                    mode = PrimitiveType.Lines;
                    break;
                case STPrimitiveType.LineLoop:
                    mode = PrimitiveType.LineLoop;
                    break;
                case STPrimitiveType.Points:
                    mode = PrimitiveType.Points;
                    break;
                case STPrimitiveType.QuadStrips:
                    mode = PrimitiveType.QuadStrip;
                    break;
                case STPrimitiveType.Quad:
                    mode = PrimitiveType.Quads;
                    break;
            }

            GL.Disable(EnableCap.CullFace);

            msh.vao.Enable();
            msh.vao.Use();
            GL.DrawElements(mode,
                group.Faces.Count,
                DrawElementsType.UnsignedInt,
                group.FaceOffset);

            GL.Enable(EnableCap.CullFace);
        }

        public virtual void PrepareShaders()
        {
            if (ShaderProgram != null)
                return;

            ShaderProgram = new ShaderProgram(
                new VertexShader(VertexShaderBasic),
                new FragmentShader(FragmentShaderBasic));

            PrepareDebugShaders();
        }

        public virtual void PrepareDebugShaders()
        {
            if (ShaderProgramDebug != null)
                return;

            ShaderProgramDebug = new ShaderProgram(
                new VertexShader(VertexShaderBasic),
                new FragmentShader(FragmentShaderDebug));
        }

        private static string FragmentShaderDebug = @"
            #version 330

            uniform vec4 highlight_color;

            //Samplers
            uniform sampler2D tex_Diffuse;
            uniform sampler2D UVTestPattern;

            uniform int hasDiffuse;
            uniform int renderVertColor;

            uniform int renderType;

            in vec2 f_texcoord0;
            in vec3 fragPosition;

            in vec4 vertexColor;
            in vec3 normal;
            in vec3 boneWeightsColored;

            in vec4 tangent;
            in vec4 binormal;

            out vec4 FragColor;

            const int DisplayNormals = 1;
            const int DisplayLighting = 2;
            const int DisplayDiffuse = 3;
            const int DisplayColors = 4;
            const int DisplayUVCoords = 5;
            const int DisplayUVPattern = 6;

            void main(){
                FragColor = vec4(1);
                vec3 displayNormal = (normal.xyz * 0.5) + 0.5;

	            vec3 lightDir = vec3(0, 0, 1);
	            float light = 0.6 + abs(dot(normal, lightDir)) * 0.5;

                vec2 displayTexCoord = f_texcoord0;

                if (renderType == DisplayNormals)
                    FragColor.rgb = displayNormal.rgb;
                if (renderType == DisplayLighting)
                    FragColor.rgb = vec3(light);
                if (renderType == DisplayDiffuse)
                    FragColor.rgb = texture(tex_Diffuse,displayTexCoord).rgb;
                if (renderType == DisplayColors)
                    FragColor.rgb = vertexColor.rgb;
                if (renderType == DisplayUVCoords)
                    FragColor.rgb = vec3(displayTexCoord.y,displayTexCoord.x, 1.0f);
                if (renderType == DisplayUVPattern)
                    FragColor.rgb = texture(UVTestPattern,displayTexCoord).rgb;

                float hc_a   = highlight_color.w;
                FragColor = vec4(FragColor.rgb * (1-hc_a) + highlight_color.rgb * hc_a, FragColor.a);
            }
        ";

        private static string FragmentShaderBasic = @"
            #version 330

            uniform vec4 highlight_color;

            uniform vec4 diffuseColor;

            //Samplers
            uniform sampler2D tex_Diffuse;

            uniform int hasDiffuse;
            uniform int renderVertColor;

            in vec2 f_texcoord0;
            in vec3 fragPosition;

            in vec4 vertexColor;
            in vec3 normal;
            in vec3 boneWeightsColored;
            in vec4 tangent;
            in vec4 binormal;

            out vec4 FragColor;

            void main(){
                vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                float hc_a   = highlight_color.w;

                vec4 color = vec4(0.8f);
                if (hasDiffuse == 1)
                    color = texture(tex_Diffuse,f_texcoord0);

                color *= diffuseColor;

                float halfLambert = max(displayNormal.y,0.5);
                vec4 colorComb = vec4(color.rgb * (1-hc_a) + highlight_color.rgb * hc_a, color.a);

	            vec3 lightDir = vec3(0, 0, 1);
	            float light = 0.6 + abs(dot(normal, lightDir)) * 0.5;

                FragColor = vec4(colorComb.rgb * light, colorComb.a);

                if (renderVertColor == 1)
                    FragColor *= min(vertexColor, vec4(1));

                FragColor.rgb *= min(boneWeightsColored, vec3(1));
         }";

        private static string VertexShaderBasic = @"
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
            uniform mat4 bones[230];

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

        public static Dictionary<string, int> LocationList = new Dictionary<string, int>()
        {
            { "vPosition", 0 },
            { "vNormal", 1 },
            { "vTexCoord", 2 },
            { "vColor", 3 },
            { "vBone", 4 },
            { "vWeight", 5 },
            { "vTangent", 6 },
            { "vBinormal", 7 },
            { "vTexCoord1", 8 },
            { "vTexCoord2", 9 },
            { "vTexCoord3", 10 },
            { "vTexCoord4", 11 },
            { "vTexCoord5", 12 },
        };
    }

    public class VertexAttribute
    {
        public string Attribute;

        public int ElementCount { get; set; }

        public VertexAttribPointerType Format { get; set; }

        public bool Normalized { get; set; }

        public VertexAttribute(string attribute, int numElements,
            VertexAttribPointerType format, bool normalized)
        {
            Attribute = attribute;
            ElementCount = numElements;
            Format = format;
            Normalized = normalized;
        }

        public int Location => ModelRenderer.LocationList[Attribute];

        public int Size => FormatStride * ElementCount;

        public int FormatStride
        {
            get
            {
                switch (Format)
                {
                    case VertexAttribPointerType.Float:
                    case VertexAttribPointerType.UnsignedInt:
                    case VertexAttribPointerType.Int:
                        return 4;
                    case VertexAttribPointerType.HalfFloat:
                    case VertexAttribPointerType.UnsignedShort:
                    case VertexAttribPointerType.Short:
                        return 2;
                    case VertexAttribPointerType.UnsignedByte:
                    case VertexAttribPointerType.Byte:
                        return 1;
                    default:
                        throw new Exception($"Unsupported attribute format! {Format}");
                }
            }
        }
    }

    public class MeshRender
    {
        public VertexArrayObject vao;
        public STGenericMesh Mesh;

        public List<VertexAttribute> Attributes = new List<VertexAttribute>();

        ModelRenderer Renderer;
        int indexBuffer;
        int vaoBuffer;

        public MeshRender(STGenericMesh mesh, ModelRenderer render) {
            Mesh = mesh;
            Renderer = render;
        }

        public void Initialize()
        {
            int[] buffers = new int[2];
            GL.GenBuffers(2, buffers);

            indexBuffer = buffers[0];
            vaoBuffer = buffers[1];

            UpdateVertexBuffer();

            vao = new VertexArrayObject(vaoBuffer, indexBuffer);

            int offset = 0;
            int totalstride = Attributes.Sum(x => x.Size);
            foreach (var att in Attributes) {
                Console.WriteLine($"Attribute {att.Attribute} {att.Format} {att.Size} {att.ElementCount} {offset} {att.Normalized} {totalstride}");
                vao.AddAttribute(att.Location, att.ElementCount, att.Format, att.Normalized, totalstride, offset);
                offset += att.Size;
            }

            vao.Initialize();
        }

        private void UpdateVertexBuffer()
        {
            UpdateAttributes();

            int[] indexData = CreateIndexBuffer(Mesh);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(int), indexData, BufferUsageHint.StaticDraw);

            float[] data = CreateVertexBuffer(Mesh);
            Console.WriteLine($"Vertex data {data.Length}");

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
        }

        private void UpdateAttributes()
        {
            Attributes.Clear();
            Attributes.Add(new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, false));
            Attributes.Add(new VertexAttribute("vNormal", 3, VertexAttribPointerType.Float, false));
            Attributes.Add(new VertexAttribute("vTexCoord", 2, VertexAttribPointerType.Float, false));
            Attributes.Add(new VertexAttribute("vColor", 4, VertexAttribPointerType.UnsignedByte, true));
            Attributes.Add(new VertexAttribute("vBone", 4, VertexAttribPointerType.Int, false));
            Attributes.Add(new VertexAttribute("vWeight", 4, VertexAttribPointerType.Float, false));
            if (Renderer.SupportsTangents)
                Attributes.Add(new VertexAttribute("vTangent", 4, VertexAttribPointerType.Float, false));
            if (Renderer.SupportsBinormals)
                Attributes.Add(new VertexAttribute("vBinormal", 4, VertexAttribPointerType.Float, false));

            if (Mesh.Vertices.Count > 0)
            {
                int numTexCoords = Mesh.Vertices[0].TexCoords.Length;
                Console.WriteLine($"numTexCoords {numTexCoords}");
                for (int i = 0; i < numTexCoords - 1; i++)
                {
                    Attributes.Add(new VertexAttribute($"vTexCoord{i + 1}", 2, VertexAttribPointerType.Float, false));
                }
            }
        }

        private int[] CreateIndexBuffer(STGenericMesh mesh)
        {
            int polyOffset = 0;

            List<int> indices = new List<int>();
            foreach (var poly in mesh.PolygonGroups) {
                poly.FaceOffset = polyOffset * sizeof(int);

                for (int f = 0; f < poly.Faces.Count; f++)
                    indices.Add((int)poly.Faces[f]);

                polyOffset += poly.Faces.Count;
            }
            return indices.ToArray();
        }

        private float[] CreateVertexBuffer(STGenericMesh mesh)
        {
            List<float> list = new List<float>();
            Console.WriteLine($"Vertex Count {mesh.Vertices.Count}");
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                list.Add(mesh.Vertices[i].Position.X);
                list.Add(mesh.Vertices[i].Position.Y);
                list.Add(mesh.Vertices[i].Position.Z);
                list.Add(mesh.Vertices[i].Normal.X);
                list.Add(mesh.Vertices[i].Normal.Y);
                list.Add(mesh.Vertices[i].Normal.Z);
                for (int t = 0; t < 1; t++)
                {
                    if (mesh.Vertices[i].TexCoords.Length > t) {
                        list.Add(mesh.Vertices[i].TexCoords[t].X);
                        list.Add(mesh.Vertices[i].TexCoords[t].Y);
                    }
                    else {
                        list.Add(0);
                        list.Add(0);
                    }
                }

                Vector4 color = new Vector4(255,255,255,255);
                if (mesh.Vertices[i].Colors.Length > 0)
                   color = mesh.Vertices[i].Colors[0] * 255;

                list.Add(BitConverter.ToSingle(new byte[4]
                {
                    (byte)color.X,
                    (byte)color.Y,
                    (byte)color.Z,
                    (byte)color.W
                }, 0));

                for (int j = 0; j < 4; j++)
                {
                    int index = -1;
                    if (mesh.Vertices[i].BoneIndices.Count > j)
                        index = mesh.Vertices[i].BoneIndices[j];

                    list.Add(Convert.ToSingle(index));
                }
                for (int j = 0; j < 4; j++)
                { 
                    if (mesh.Vertices[i].BoneWeights.Count > j)
                        list.Add(mesh.Vertices[i].BoneWeights[j]);
                    else
                        list.Add(0);
                }
                if (Renderer.SupportsTangents)
                {
                    list.Add(mesh.Vertices[i].Tangent.X);
                    list.Add(mesh.Vertices[i].Tangent.Y);
                    list.Add(mesh.Vertices[i].Tangent.Z);
                }
                if (Renderer.SupportsBinormals)
                {
                    list.Add(mesh.Vertices[i].Bitangent.X);
                    list.Add(mesh.Vertices[i].Bitangent.Y);
                    list.Add(mesh.Vertices[i].Bitangent.Z);
                }

                for (int t = 0; t < mesh.Vertices[i].TexCoords.Length - 1; t++)
                {
                    list.Add(mesh.Vertices[i].TexCoords[t + 1].X);
                    list.Add(mesh.Vertices[i].TexCoords[t + 1].Y);
                }
            }
            return list.ToArray();
        }
    }
}
