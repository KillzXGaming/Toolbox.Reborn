using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ModelView;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a model which stores multiple meshes <see cref="STGenericMesh"/>
    /// and multiple materials <see cref="STGenericMaterial"/> and
    /// a <see cref="STSkeleton"/>.
    /// </summary>
    public class STGenericModel
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A list of <see cref="STGenericMesh"/> used for rendering and exporting mesh data.
        /// editing, and exporting meshes.
        /// </summary>
        public List<STGenericMesh> Meshes = new List<STGenericMesh>();

        /// <summary>
        /// A list of <see cref="STGenericMaterial"/> used for rendering and exporting material data. 
        /// editing, and exporting materials.
        /// </summary>
        public List<STGenericMaterial> Materials = new List<STGenericMaterial>();

        /// <summary>
        /// A list of <see cref="STGenericTexture"/> used for rendering and exporting textures.
        /// </summary>
        public List<STGenericTexture> Textures = new List<STGenericTexture>();

        /// <summary>
        /// The skeleton of the model used to store a list of <see cref="STBone"/>.
        /// Used for rendering and exporting bone data.
        /// </summary>
        public STSkeleton Skeleton = new STSkeleton();

        public STGenericModel(string name) {
            Name = name;
        }

        public List<STGenericMaterial> GetMaterials()
        {
            List<STGenericMaterial> materials = new List<STGenericMaterial>();
            foreach (var mesh in Meshes)
            {
                foreach (var group in mesh.PolygonGroups)
                {
                    if (group.Material != null && !materials.Contains(group.Material))
                        materials.Add(group.Material);
                }
            }
            return materials;
        }

        public ObjectTreeNode CreateTreeHiearchy()
        {
            ObjectTreeNode root = new ObjectTreeNode(Name) { Tag = this };
            ObjectTreeNode meshFolder = new ObjectTreeNode("Meshes");
            ObjectTreeNode textureFolder = new ObjectTreeNode("Textures");
            ObjectTreeNode skeletonFolder = new ObjectTreeNode("Skeleton");

            root.ImageKey = "Model";

            foreach (var mesh in Meshes)
                meshFolder.AddChild(LoadMesh(mesh));

            foreach (var tex in Textures)
                textureFolder.AddChild(LoadTextureFormat(tex));

            if (Skeleton != null)
                skeletonFolder.Children.AddRange(Skeleton.CreateBoneTree());

            if (meshFolder.ChildCount > 0) root.AddChild(meshFolder);
            if (textureFolder.ChildCount > 0) root.AddChild(textureFolder);
            if (skeletonFolder.ChildCount > 0) root.AddChild(skeletonFolder);

            return root;
        }

        private ObjectTreeNode LoadMesh(STGenericMesh mesh)
        {
            ObjectTreeNode node = new ObjectTreeNode(mesh.Name);
            node.ImageKey = "Mesh";
            node.Tag = mesh;
            return node;
        }

        private ObjectTreeNode LoadTextureFormat(STGenericTexture texture)
        {
            ObjectTreeNode node = new ObjectTreeNode(texture.Name);
            node.ImageKey = "Texture";
            node.Tag = texture;
            return node;
        }
    }
}
