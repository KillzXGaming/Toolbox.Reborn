using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.ModelView;
using Toolbox.Core.Imaging;
using OpenTK;

namespace CTRLibrary.Grezzo
{
    public class CMD : IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "CTR Model Binary" };
        public string[] Extension { get; set; } = new string[] { "*.cmb" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "cmb ");
            }
        }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric());
        public CMB_Parser Header;

        public void Load(Stream stream) {
            Header = new CMB_Parser(stream);

            Runtime.BonePointSize = 3;
        }

        public void Save(Stream stream)
        {

        }

        private STGenericModel Model;
        public STGenericModel ToGeneric()
        {
            if (Model != null) return Model;

            var model = new STGenericModel(FileInfo.FileName);
            var sectionData = Header.FileHeader.SectionData;

            foreach (var bone in sectionData.SkeletonChunk.Bones)
            {
                model.Skeleton.Bones.Add(new STBone(model.Skeleton)
                {
                    Name = $"Bone{bone.ID}",
                    Position = new Vector3(
                        bone.Translation.X,
                        bone.Translation.Y,
                        bone.Translation.Z),
                    Scale = new Vector3(
                        bone.Scale.X,
                        bone.Scale.Y,
                        bone.Scale.Z),
                    EulerRotation = new Vector3(
                        bone.Rotation.X,
                        bone.Rotation.Y,
                        bone.Rotation.Z),
                    ParentIndex = bone.ParentIndex,
                });
            }

            model.Skeleton.Reset();
            model.Skeleton.Update();

            foreach (var tex in sectionData.TextureChunk.Textures) {
                model.Textures.Add(new CTXB.TextureWrapper(tex) { Name = $"Texture{model.Textures.Count}" });
            }

            foreach (var mat in sectionData.MaterialChunk.Materials)
            {
                STGenericMaterial genericMat = new STGenericMaterial();
                genericMat.Name = $"Material{model.Materials.Count}";
                model.Materials.Add(genericMat);

                bool HasDiffuse = false;
                foreach (var tex in mat.TextureMaps)
                {
                    if (tex.TextureIndex != -1)
                    {
                        STGenericTextureMap matTexture = new STGenericTextureMap();
                        genericMat.TextureMaps.Add(matTexture);

                        if (tex.TextureIndex < model.Textures.Count)
                            matTexture.Name = model.Textures[tex.TextureIndex].Name;

                        if (!HasDiffuse && matTexture.Name != "bg_syadowmap") //Quick hack till i do texture env stuff
                        {
                            matTexture.Type = STTextureType.Diffuse;
                            HasDiffuse = true;
                        }
                    }
                }
            }

            var shapeData = sectionData.SkeletalMeshChunk.ShapeChunk;
            var meshData = sectionData.SkeletalMeshChunk.MeshChunk;
            foreach (var mesh in meshData.Meshes)
            {
                STGenericMesh genericMesh = new STGenericMesh();
                genericMesh.Name = $"Mesh_{model.Meshes.Count}";
                model.Meshes.Add(genericMesh);

                var shape = shapeData.SeperateShapes[(int)mesh.SepdIndex];

                List<ushort> SkinnedBoneTable = new List<ushort>();
                foreach (var prim in shape.Primatives)
                {
                    if (prim.BoneIndexTable != null)
                        SkinnedBoneTable.AddRange(prim.BoneIndexTable);
                }

                //Now load the vertex and face data
                if (shape.Position.VertexData != null)
                {
                    int VertexCount = shape.Position.VertexData.Length;
                    for (int v = 0; v < VertexCount; v++)
                    {
                        STVertex vert = new STVertex();
                        vert.TexCoords = new Vector2[1];

                        vert.Position = new OpenTK.Vector3(
                            shape.Position.VertexData[v].X,
                            shape.Position.VertexData[v].Y,
                            shape.Position.VertexData[v].Z);

                        if (shape.Normal.VertexData != null && shape.Normal.VertexData.Length > v)
                        {
                            vert.Normal = new OpenTK.Vector3(
                            shape.Normal.VertexData[v].X,
                            shape.Normal.VertexData[v].Y,
                            shape.Normal.VertexData[v].Z).Normalized();
                        }

                        if (shape.Color.VertexData != null && shape.Color.VertexData.Length > v)
                        {
                            vert.Colors = new Vector4[1]
                            {
                                new OpenTK.Vector4(
                            shape.Color.VertexData[v].X,
                            shape.Color.VertexData[v].Y,
                            shape.Color.VertexData[v].Z,
                            shape.Color.VertexData[v].W).Normalized() 
                            };
                        }

                        if (shape.TexCoord0.VertexData != null && shape.TexCoord0.VertexData.Length > v)
                        {
                            vert.TexCoords[0] = new OpenTK.Vector2(
                            shape.TexCoord0.VertexData[v].X,
                            1 - shape.TexCoord0.VertexData[v].Y);
                        }

                        if (shape.TexCoord1.VertexData != null)
                        {

                        }

                        if (shape.TexCoord2.VertexData != null)
                        {

                        }

                        for (int i = 0; i < 16; i++)
                        {
                            if (i < shape.Primatives[0].BoneIndexTable.Length)
                            {
                                int boneId = shape.Primatives[0].BoneIndexTable[i];

                                if (shape.Primatives[0].SkinningMode == SkinningMode.RIGID_SKINNING)
                                {
                                    vert.Position = Vector3.TransformPosition(vert.Position, model.Skeleton.Bones[boneId].Transform);
                                    vert.Normal = Vector3.TransformNormal(vert.Position, model.Skeleton.Bones[boneId].Transform);
                                }
                            }
                        }

                        bool HasSkinning = shape.Primatives[0].SkinningMode != SkinningMode.SINGLE_BONE
                        && shape.BoneIndices.Type == CmbDataType.UByte; //Noclip checks the type for ubyte so do the same

                        bool HasWeights = shape.Primatives[0].SkinningMode == SkinningMode.SMOOTH_SKINNING;

                        if (shape.BoneIndices.VertexData != null && HasSkinning && shape.BoneIndices.VertexData.Length > v)
                        {
                            var BoneIndices = shape.BoneIndices.VertexData[v];
                            for (int j = 0; j < shape.boneDimension; j++)
                            {
                                if (BoneIndices[j] < SkinnedBoneTable.Count)
                                    vert.BoneIndices.Add((int)SkinnedBoneTable[(int)BoneIndices[j]]);
                                //   Console.WriteLine("boneIds " + BoneIndices[j]);

                                //    ushort index = shape.Primatives[0].BoneIndexTable[(uint)BoneIndices[j]];
                            }
                        }
                        if (shape.BoneWeights.VertexData != null && HasWeights && shape.BoneWeights.VertexData.Length > v)
                        {
                            var BoneWeights = shape.BoneWeights.VertexData[v];
                            for (int j = 0; j < shape.boneDimension; j++)
                            {
                                vert.BoneWeights.Add(BoneWeights[j]);
                            }
                        }

                        genericMesh.Vertices.Add(vert);
                    }
                }

                foreach (var prim in shape.Primatives)
                {
                    STPolygonGroup group = new STPolygonGroup();
                    genericMesh.PolygonGroups.Add(group);
                    group.MaterialIndex = mesh.MaterialIndex;

                   // for (int i = 0; i < prim.Primatives[0].Indices.Length; i++)
                  //      group.Faces.Add(prim.Primatives[0].Indices[i]);
                }
            }

            Model = model;
            return model;
        }
    }
}
