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
using GCNLibrary.J3D;

namespace GCNLibrary.Pikmin1.Model
{
    public class MOD  : IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "Pikmin 1 Model" };
        public string[] Extension { get; set; } = new string[] { "*.mod" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".mod";
        }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric())
        { };

        public MOD_Parser Header;

        public void Load(Stream stream)
        {
        //    this.Label = FileInfo.FileName;
        //    Tag = this;
            Header = new MOD_Parser(stream);
        }

        public void Save(Stream stream)
        {

        }

        private STGenericModel CachedModel;
        public STGenericModel ToGeneric()
        {
            if (CachedModel != null) return CachedModel;

            var model = new STGenericModel(FileInfo.FileName);

            for (int i = 0; i < Header.Textures?.Length; i++)
                model.Textures.Add(Header.Textures[i]);

            var sorted = Header.Joints.ToArray();
            for (int i = 0; i < sorted.Length; i++) {
                var joint = sorted[i];

                var bone = new STBone(model.Skeleton);
                if (Header.JointNames.Length > i)
                    bone.Name = Header.JointNames[i];
                else if (joint.PolygonGroups.Count > 0)
                    bone.Name = $"Batch{i}";
                else
                    bone.Name = $"Bone{i}";

                bone.Position = joint.Position;
                bone.EulerRotation = joint.Rotation;
                bone.Scale = joint.Scale;
                bone.ParentIndex = joint.ParentIndex;

                model.Skeleton.Bones.Add(bone);
            }


            model.Skeleton.Reset();
            model.Skeleton.Update();

            for (int i = 0; i < Header.Joints.Length; i++) {
                foreach (var poly in Header.Joints[i].PolygonGroups) {
                    var mesh = Header.Shapes[poly.ShapeIndex];
                    var mat = Header.MaterialData.Materials[poly.MaterialIndex];

                    var genericMesh = CreateGenericMesh(model, mesh, mat);
                    genericMesh.Name = $"Mesh{model.Meshes.Count}";
                    model.Meshes.Add(genericMesh);
                }
            }

            CachedModel = model;
            return CachedModel;
        }

        private STGenericMesh CreateGenericMesh(STGenericModel model, MOD_Parser.Shape mesh, Material material)
        {
            List<STVertex> transformedVertices2 = new List<STVertex>();
            for (int v = 0; v < mesh.Vertices.Count; v++)
            {
                if (mesh.Vertices[v].BoneIndices.Count > 1 || transformedVertices2.Contains(mesh.Vertices[v]))
                    continue;

                transformedVertices2.Add(mesh.Vertices[v]);
                for (int j = 0; j < mesh.Vertices[v].BoneIndices.Count; j++)
                {
                    int index = mesh.Vertices[v].BoneIndices[j];
                    if (mesh.Vertices[v].BoneIndices.Count == 1)
                        mesh.Vertices[v].BoneIndices[j] = Header.RigidSkinningIndices[index];
                }
            }

            var genericMesh = new STGenericMesh();

            List<STVertex> transformedVertices = new List<STVertex>();
            for (int v = 0; v < mesh.Vertices.Count; v++)
            {
                if (mesh.Vertices[v].BoneIndices.Count == 1 && !transformedVertices.Contains(mesh.Vertices[v]))
                {
                    transformedVertices.Add(mesh.Vertices[v]);

                    var boneIndex = mesh.Vertices[v].BoneIndices[0];
                    var transform = model.Skeleton.Bones[boneIndex].Transform;

                    mesh.Vertices[v].Position = Vector3.TransformPosition(mesh.Vertices[v].Position,
                       transform);
                    mesh.Vertices[v].Normal = Vector3.TransformNormal(mesh.Vertices[v].Normal,
                        transform);

                    Console.WriteLine($"TRANSFORM {boneIndex}");
                }
            }

            genericMesh.Vertices.AddRange(mesh.Vertices);

            var poly = new STPolygonGroup();
            genericMesh.PolygonGroups.Add(poly);
            genericMesh.Optmize(poly);

            var genericMat = new MODMaterial(Header, material);
            poly.Material = genericMat;

            if (material.TextureFlags == 2)
                poly.IsTransparentPass = true;

            return genericMesh;
        }
    }
}
