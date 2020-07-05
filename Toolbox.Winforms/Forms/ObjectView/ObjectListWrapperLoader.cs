using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;
using Toolbox.Core.ModelView;

namespace Toolbox.Winforms
{
    public class ObjectListWrapperLoader
    {
        public static ObjectTreeNode OpenFormat(ImageList imageList, IFileFormat fileFormat)
        {
            ObjectTreeNode hiearchyNode = null;

            if (fileFormat is IArchiveFile)
            {
                hiearchyNode = LoadArchiveFormat((IArchiveFile)fileFormat);
                if (fileFormat is ObjectTreeNode) {
                    hiearchyNode.OnBeforeExpand();
                    foreach (var child in ((ObjectTreeNode)fileFormat).Children)
                        hiearchyNode.AddChild(child);
                }
            }
            else if (fileFormat is ObjectTreeNode)
                hiearchyNode = (ObjectTreeNode)fileFormat;
            else if (fileFormat is ITextureContainer)
                hiearchyNode = LoadTextureContainerFormat(imageList, (ITextureContainer)fileFormat);
            else if (fileFormat is STGenericTexture)
                hiearchyNode = LoadTextureFormat((STGenericTexture)fileFormat);
            else if (fileFormat is IModelFormat)
                hiearchyNode = LoadModelFormat((IModelFormat)fileFormat);
            else if (fileFormat is IModelSceneFormat)
                hiearchyNode = LoadModelFormat((IModelSceneFormat)fileFormat);
            else
                hiearchyNode = new ObjectTreeNode(fileFormat.FileInfo.FileName) { Tag = fileFormat, };

            hiearchyNode.OnBeforeExpand();

            if (fileFormat is IArchiveFile && fileFormat is ITextureContainer)
                hiearchyNode.Children.AddRange(LoadTextureContainerFormat(imageList, (ITextureContainer)fileFormat).Children);

            return hiearchyNode;
        }

        static ObjectTreeNode LoadModelFormat(IModelSceneFormat modelFormat)
        {
            IFileFormat fileFormat = (IFileFormat)modelFormat;
            var scene = modelFormat.ToGeneric();
            ObjectTreeNode root = new ObjectTreeNode(fileFormat.FileInfo.FileName) { Tag = modelFormat };
            return root;
        }

        static ObjectTreeNode LoadModelFormat(IModelFormat modelFormat)
        {
            IFileFormat fileFormat = (IFileFormat)modelFormat;
            var model = modelFormat.ToGeneric();

            ObjectTreeNode root = new ObjectTreeNode(fileFormat.FileInfo.FileName) { Tag = modelFormat };
            ObjectTreeNode meshFolder = new ObjectTreeNode("Meshes");
            ObjectTreeNode textureFolder = new ObjectTreeNode("Textures");
            ObjectTreeNode skeletonFolder = new ObjectTreeNode("Skeleton");

            foreach (var mesh in model.Meshes)
                meshFolder.AddChild(LoadMesh(mesh));

            foreach (var tex in model.Textures)
                textureFolder.AddChild(LoadTextureFormat(tex));

            if (model.Skeleton != null)
                skeletonFolder.Children.AddRange(model.Skeleton.CreateBoneTree());

            if (meshFolder.ChildCount > 0) root.AddChild(meshFolder);
            if (textureFolder.ChildCount > 0) root.AddChild(textureFolder);
            if (skeletonFolder.ChildCount > 0) root.AddChild(skeletonFolder);

            return root;
        }

        static ObjectTreeNode LoadTextureContainerFormat(ImageList imageList, ITextureContainer textureContainer)
        {
            IFileFormat fileFormat = (IFileFormat)textureContainer;
            ObjectTreeNode root = new ObjectTreeNode(fileFormat.FileInfo.FileName);
            root.Tag = fileFormat;
            root.ImageKey = "TextureContainer";

            foreach (var tex in textureContainer.TextureList)
                root.AddChild(LoadTextureFormat(tex));

            return root;
        }

        static ObjectTreeNode LoadMesh(STGenericMesh mesh)
        {
            ObjectTreeNode node = new ObjectTreeNode(mesh.Name);
            node.ImageKey = "Mesh";
            node.Tag = mesh;
            return node;
        }

        static ObjectTreeNode LoadTextureFormat(STGenericTexture texture)
        {
            ObjectTreeNode node = new ObjectTreeNode(texture.Name);
            node.ImageKey = "Texture";
            node.Tag = texture;
            return node;
        }

        static ObjectTreeNode LoadArchiveFormat(IArchiveFile archiveFile)
        {
            IFileFormat fileFormat = (IFileFormat)archiveFile;

            ObjectTreeNode root = new ObjectTreeNode(fileFormat.FileInfo.FileName);

            root.ImageKey = "Folder";
            root.Tag = fileFormat;

            var hiearchyNode = CreateObjectHiearchy(root, archiveFile);
            if (hiearchyNode.ChildCount == 1 && hiearchyNode.Children[0].ChildCount > 0)
            {
                hiearchyNode = hiearchyNode.Children[0];
                hiearchyNode.Tag = fileFormat;
            }

            return new ArchiveFileWrapper(hiearchyNode);
        }

        static ObjectTreeNode CreateObjectHiearchy(ObjectTreeNode parent, IArchiveFile archiveFile)
        {
            // build a TreeNode collection from the file list
            foreach (var file in archiveFile.Files)
            {
                string[] paths = file.FileName.Split('/');
                ProcessTree(parent, file, paths, 0);
            }
            return parent;
        }

        static void ProcessTree(ObjectTreeNode parent, ArchiveFileInfo file, string[] paths, int index)
        {
            string currentPath = paths[index];
            if (paths.Length - 1 == index)
            {
                var fileNode = new ObjectTreeNode(currentPath);
                string ext = Utils.GetExtension(currentPath);
                if (FileImageKeys.Lookup.ContainsKey(ext))
                    fileNode.ImageKey = FileImageKeys.Lookup[ext];
                else
                    fileNode.ImageKey = "File";
                fileNode.Type = ext;
                fileNode.Size = STMath.GetFileSize(file.GetFileSize());
                fileNode.Tag = file;

                parent.AddChild(fileNode);
                return;
            }

            var node = FindFolderNode(parent, currentPath);
            if (node == null)
            {
                node = new ObjectTreeNode(currentPath);
                node.ImageKey = "Folder";
                parent.AddChild(node);
            }

            ProcessTree(node, file, paths, index + 1);
        }

        private static ObjectTreeNode FindFolderNode(ObjectTreeNode parent, string path)
        {
            ObjectTreeNode node = null;
            foreach (var child in parent.Children.ToArray())
            {
                if (child.Label.Equals(path))
                {
                    node = (ObjectTreeNode)child;
                    break;
                }
            }

            return node;
        }
    }
}
