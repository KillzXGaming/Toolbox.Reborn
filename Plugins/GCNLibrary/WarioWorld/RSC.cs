using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core;
using Toolbox.Core.ModelView;

namespace GCNLibrary.WW
{
    public class RSC : ObjectTreeNode, IFileFormat, IArchiveFile
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "Wario World Resource" };
        public string[] Extension { get; set; } = new string[] { "*.rsc" };

        public File_Info FileInfo { get; set; }

        public bool CanAddFiles { get; set; } = false;
        public bool CanRenameFiles { get; set; } = false;
        public bool CanReplaceFiles { get; set; } = true;
        public bool CanDeleteFiles { get; set; } = false;

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".rsc";
        }

        public RSC_Parser Header;

        public IEnumerable<ArchiveFileInfo> Files => Header.Files;
        public void ClearFiles() { Header.Files.Clear(); }

        //Resource Data
        public List<SceneData> Scenes = new List<SceneData>();
        public List<MapModel> MapModels = new List<MapModel>();

        public List<LightingData> Lights = new List<LightingData>();

        public void Load(Stream stream) {
            Header = new RSC_Parser(FileInfo.FileName, stream);

            //Load all the resources
            foreach (var file in Header.Files)
            {
                if (file.Type == RSC_Parser.ResourceType.MapObjectParams)
                    Scenes.Add(new SceneData(file.FileData));
                if (file.Type == RSC_Parser.ResourceType.TextureContainer)
                    file.OpenFileFormatOnLoad = true;
            }

            ObjectTreeNode mapFolder = new ObjectTreeNode("Maps");
            ObjectTreeNode modelFolder = new ObjectTreeNode("Models");
            ObjectTreeNode sceneFolder = new ObjectTreeNode("Scene");

            ObjectTreeNode currentModel = null;
            foreach (var file in Header.Files)
            {
                Console.WriteLine($"Reading FILE {file.FileName} {file.Type}");
                if (file.Type == RSC_Parser.ResourceType.StaticModel || file.Type == RSC_Parser.ResourceType.RiggedModel)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                    var modelContainer = new Model(file.FileData, file.FileName, (uint)file.Type);
                    ObjectTreeNode modelContainerNode = new ObjectTreeNode($"{fileName}");
                    modelContainerNode.ImageKey = "Model";
                    modelFolder.AddChild(modelContainerNode);
                    modelContainerNode.Tag = modelContainer;
                    currentModel = modelContainerNode;

                    if (file.Type == RSC_Parser.ResourceType.StaticModel)
                        currentModel.Tag = modelContainer.Models[0];

                    var firstNode = CreateModelNode(modelContainer, fileName, 0,
                          file.Type == RSC_Parser.ResourceType.RiggedModel);

                    foreach (var child in firstNode.Children)
                        modelContainerNode.AddChild(child);

                    for (int i = 0; i < modelContainer.Models.Count; i++)
                    {
                        if (i == 0)
                            continue;

                        var modelNode = CreateModelNode(modelContainer, fileName, i, 
                            file.Type == RSC_Parser.ResourceType.RiggedModel);

                        modelContainerNode.AddChild(modelNode);
                    }


                    if (file.Type == RSC_Parser.ResourceType.RiggedModel)
                    {
                        ObjectTreeNode modelAnimationsFolder = new ObjectTreeNode("Animations");
                        modelContainerNode.AddChild(modelAnimationsFolder);
                    }
                }
                if (file.Type == RSC_Parser.ResourceType.MapModel)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                    var model = new MapModel(file.FileData, file.FileName, (uint)file.Type);
                    var genericModel = model.ToGeneric();
                    genericModel.Name = $"{fileName}";
                    ObjectTreeNode modelNode = genericModel.CreateTreeHiearchy();
                    modelNode.Tag = model;
                    modelFolder.AddChild(modelNode);
                    currentModel = modelNode;

                    MapModels.Add(model);
                }
                if (file.Type == RSC_Parser.ResourceType.SkeletonAnimation)
                {
                    var anim = new SkeletalAnim(file.FileData, file.FileName);
                    if (currentModel != null)
                    {
                        var model = ((Model)currentModel.Tag);
                        if (model.SkeletalAnim != null)
                            continue;

                        model.SkeletalAnim = anim;
                        model.UpdateSkeleton();
                        model.UpdateBoneIndices();
                        Runtime.BonePointSize = 6;

                       /* foreach (var modelNode in currentModel.Children) {
                            var modelgroup = (ModelGroup)modelNode.Tag;
                            var nodes = modelgroup.ToGeneric().Skeleton.CreateBoneTree();
                            foreach (var node in nodes)
                                modelNode.Children[1].AddChild(node);
                        }*/

                        var animations = anim.ToGeneric();
                        foreach (var animation in animations)
                        {
                            var animNode = new ObjectTreeNode(animation.Name);
                            animNode.Tag = animation;
                            currentModel.Children[model.Models.Count + 2].AddChild(animNode);
                        }

                        currentModel.Tag = model.Models[0];
                        currentModel = null;
                    }
                }
                if (file.Type == RSC_Parser.ResourceType.LightingData)
                {
                    string fileName = Path.GetFileName(file.FileName);

                    var lightingData = new LightingData(file.FileData);
                    var lightingNode = new ObjectTreeNode(fileName);
                    lightingNode.Tag = lightingData;
                    for (int i = 0; i < lightingData.Lights.Length; i++)
                    {
                        var light = lightingData.Lights[i];
                        lightingNode.AddChild(new ObjectTreeNode()
                        {
                            Label = $"Light{i}",
                            Tag = light,
                        });
                    }
                    this.AddChild(lightingNode);
                }
            }

            if (modelFolder.Children.Count > 0)
                this.AddChild(modelFolder);
        }

        private ObjectTreeNode CreateModelNode(Model modelContainer, string fileName, int index, bool isRigged)
        {
            var model = modelContainer.Models[index];
            var genericModel = model.ToGeneric();
            genericModel.Name = $"{fileName}_SubModel{index}";
            foreach (var texture in modelContainer.TextureContainer.Textures)
                genericModel.Textures.Add(texture);

            ObjectTreeNode modelNode = genericModel.CreateTreeHiearchy();
            modelNode.Tag = model;

            if (isRigged)
            {
                ObjectTreeNode skeletonFolder = new ObjectTreeNode("Skeleton");
                skeletonFolder.Tag = genericModel.Skeleton;
                modelNode.AddChild(skeletonFolder);
            }
            return modelNode;
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Add(new RSC_Parser.FileEntry()
            {
                FileData = archiveFileInfo.FileData,
                FileName = archiveFileInfo.FileName,
            });
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            Header.Files.Remove((RSC_Parser.FileEntry)archiveFileInfo);
            return false;
        }

        public class ArcFile : ArchiveFileInfo
        {

        }
    }
}
