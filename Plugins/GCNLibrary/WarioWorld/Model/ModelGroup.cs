using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.OpenGL;

namespace GCNLibrary.WW
{
    public class ModelGroup : IModelFormat
    {
        public ModelRenderer Renderer => new ModelRenderer(ToGeneric());

        public List<Mesh> Meshes = new List<Mesh>();

        private STGenericModel CachedModel;
        public STGenericModel ToGeneric()
        {
            if (CachedModel != null) return CachedModel;

            STGenericModel model = new STGenericModel("Model");
            foreach (var shape in Meshes)
            {
                foreach (var meshGroup in shape.Groups)
                {
                    var genericMesh = new STGenericMesh();
                    genericMesh.Name = $"Mesh{model.Meshes.Count}";
                    genericMesh.Vertices.AddRange(meshGroup.Vertices);

                    var group = new STPolygonGroup();
                    genericMesh.PolygonGroups.Add(group);

                    var mat = new STGenericMaterial();
                    mat.DiffuseColor = meshGroup.Color;
                    group.Material = mat;
                    if (meshGroup.TextureIndex != -1)
                    {
                        var texMap = new STGenericTextureMap()
                        {
                            Name = $"Texture{meshGroup.TextureIndex}",
                            Type = STTextureType.Diffuse,
                        };
                        mat.TextureMaps.Add(texMap);
                    }

                    genericMesh.Optmize(group);
                    model.Meshes.Add(genericMesh);
                }
            }

            CachedModel = model;
            return model;
        }

        /// <summary>
        /// Updates the skeleton from a baked animation if one is already loaded.
        /// </summary>
        public void UpdateSkeleton(SkeletalAnim SkeletalAnim, int index)
        {
            if (SkeletalAnim == null) return;

            var skeleton = SkeletalAnim.CreateBakedSkeleton(index);
            CachedModel.Skeleton = skeleton;
        }
    }

    public class Mesh
    {
        public List<MeshGroup> Groups = new List<MeshGroup>();
    }

    public class MeshGroup
    {
        public List<STVertex> Vertices = new List<STVertex>();
        public int TextureIndex { get; set; }
        public STColor8 Color { get; set; } = STColor8.White;
        public Packet Packet { get; set; }
    }
}
