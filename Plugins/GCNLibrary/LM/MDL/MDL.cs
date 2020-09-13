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
using Toolbox.Core.GX;
using System.Threading;
using BrawlLib.Modeling.Triangle_Converter;

namespace GCNLibrary.LM.MDL
{
    public class MDL  : IFileFormat, IModelFormat, IReplaceableModel
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "LM Actor Model" };
        public string[] Extension { get; set; } = new string[] { "*.mdl" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                reader.SetByteOrder(true);
                return reader.ReadUInt32() == 0x04B40000;
            }
        }

        public ModelRenderer Renderer => new MDL_Render(ToGeneric());
        public List<STGenericTexture> Textures = new List<STGenericTexture>();

        public MDL_Parser Header;

        public void Load(Stream stream)
        {
            Header = new MDL_Parser(stream);
            foreach (var tex in Header.Textures)
                Textures.Add(new Texture(tex) { Name = $"Texture{Textures.Count}" });

            ToGeneric().Skeleton.PreviewScale = 3;
        }

        private STGenericModel Model;

        public STGenericModel ToGeneric()
        {
            if (Model != null) return Model;

            var model = new STGenericModel(FileInfo.FileName);
            model.Textures = Textures;

            Matrix4 matrix = Matrix4.Identity;

            List<STGenericMaterial> materials = new List<STGenericMaterial>();
            for (int i = 0; i < Header.Materials.Length; i++)
                materials.Add(CreateMaterial(Header.Materials[i], i));

            model.Skeleton = new STSkeleton();
            Matrix4[] transforms = new Matrix4[Header.FileHeader.JointCount];
            for (int i = 0; i < Header.FileHeader.JointCount; i++)
            {
                var transfrom = Header.Matrix4Table[i];
                transfrom.Invert();
                transfrom.Transpose();
                transforms[i] = transfrom;

                model.Skeleton.Bones.Add(new STBone(model.Skeleton)
                {
                    Name = Header.Nodes[i].ShapeCount > 0 ? $"Mesh{i}" : $"Bone{i}",
                    Position = transfrom.ExtractTranslation(),
                    Rotation = transfrom.ExtractRotation(),
                    Scale = transfrom.ExtractScale(),
                });
            }

            TraverseNodeGraph(model.Skeleton, 0);

            model.Skeleton.ConvertWorldToLocalSpace();
            model.Skeleton.Reset();
            model.Skeleton.Update();

            for (int i = 0; i < Header.Meshes.Count; i++) {
                var mesh = new STGenericMesh() { Name = $"Mesh{i}" };
                model.Meshes.Add(mesh);

                var matIndex = Header.Meshes[i].DrawElement.MaterialIndex;

                STPolygonGroup group = new STPolygonGroup();
                group.Material = materials[matIndex];
                group.PrimitiveType = STPrimitiveType.Triangles;
                mesh.PolygonGroups.Add(group);

                foreach (var packet in Header.Meshes[i].Packets) {
                    foreach (var drawList in  packet.DrawLists)
                    {
                        var verts = new List<STVertex>();
                        for (int v = 0; v < drawList.Vertices.Count; v++)
                        {
                            if (drawList.Vertices[v].MatrixIndex != -1 && drawList.Vertices[v].MatrixDataIndex < Header.FileHeader.JointCount)
                                matrix = transforms[drawList.Vertices[v].MatrixDataIndex];
                            else
                                matrix = Matrix4.Identity;

                            verts.Add(ToVertex(drawList.Vertices[v], ref matrix));
                        }

                        switch (drawList.OpCode)
                        {
                            case 0xA0:
                                verts = ConvertTriFans(verts);
                                mesh.Vertices.AddRange(verts);
                                break;
                            case 0x90:
                                mesh.Vertices.AddRange(verts);
                                break;
                            case 0x98:
                                verts = ConvertTriStrips(verts);
                                mesh.Vertices.AddRange(verts);
                                break;
                            default:
                                throw new Exception("Unknown opcode " + drawList.OpCode);
                        }
                    }
                }
                mesh.Optmize(group);
            }

            Model = model;
            return model;
        }

        static int GetMaterialIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Material", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        static int GetTextureIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Texture", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        static int GetBoneIndex(string name)
        {
            int index = 0;
            string value = name.Replace("Bone", string.Empty).Replace("Mesh", string.Empty);
            int.TryParse(value, out index);
            return index;
        }

        public void FromGeneric(STGenericScene scene) {
            var model = scene.Models[0];

            bool useTriangeStrips = false;

            MDL_Parser mdl = new MDL_Parser();
            mdl.FileHeader = new MDL_Parser.Header();
            mdl.Meshes = new List<MDL_Parser.Mesh>();

            List<Node> nodes = new List<Node>();
            List<Matrix4> matrices = new List<Matrix4>();
            List<Material> materials = new List<Material>();
            List<TextureHeader> textures = new List<TextureHeader>();
            List<Sampler> samplers = new List<Sampler>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Vector4> colors = new List<Vector4>();
            List<Shape> shapes = new List<Shape>();
            List<ShapePacket> packets = new List<ShapePacket>();

            List<DrawElement> elements = new List<DrawElement>();
            List<MDL_Parser.Weight> weights = new List<MDL_Parser.Weight>();

            model.Textures = model.Textures.OrderBy(x => GetTextureIndex(x.Name)).ToList();
            model.OrderBones(model.Skeleton.Bones.OrderBy(x => GetBoneIndex(x.Name)).ToList());

            foreach (var texture in model.Textures) {
                textures.Add(new TextureHeader()
                {
                    Width = (ushort)texture.Width,
                    Height = (ushort)texture.Height,
                    Format = Decode_Gamecube.TextureFormats.CMPR,
                    ImageData = Decode_Gamecube.EncodeFromBitmap(texture.GetBitmap(),
                    Decode_Gamecube.TextureFormats.CMPR).Item1,
                });
            }

            //If sampler list isn't replaced in json, force a new one
            if (Header.Samplers.Length == 0) {
                //Create samplers to map textures to materials
                foreach (var texture in model.Textures)
                {
                    samplers.Add(new Sampler()
                    {
                        WrapModeU = 2,
                        WrapModeV = 2,
                        MagFilter = 0,
                        MinFilter = 0,
                        TextureIndex = (ushort)model.Textures.IndexOf(texture),
                    });
                }
            }
            else
                samplers = Header.Samplers.ToList();

            //Create a root node if no bones are present
            if (model.Skeleton.Bones.Count == 0) {
                nodes.Add(new Node()
                {
                    ChildIndex = 1,
                    SiblingIndex = 0,
                    ShapeCount = (ushort)model.Meshes.Count,
                    ShapeIndex = 0,
                });
            }

            //Create a node graph from a skeleton
            foreach (var bone in model.Skeleton.Bones.Where(x => x.ParentIndex == -1))
                CreateNodeGraph(nodes, bone);

            //Set node transforms
            model.Skeleton.Reset();
            foreach (var bone in model.Skeleton.Bones) {
                Matrix4 transform = bone.Transform;
                transform.Transpose();
                transform.Invert();
                matrices.Add(transform);
            }

            ushort shapeIndex = 0;

            //Adjust the node indices
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeIndex = (ushort)i;
                if (i == 0)
                {
                    //Store the total amount of drawn elements in nodes
                    nodes[i].ShapeCount = (ushort)model.Meshes.Count;
                    shapeIndex += (ushort)model.Meshes.Count;
                }
                
                foreach (var mesh in model.Meshes) {
                    for (int v = 0; v < mesh.Vertices.Count; v++) {
                        if (mesh.Vertices[v].BoneIndices.Contains(i))
                            nodes[i].ShapeIndex = shapeIndex;
                    }
                }
            }

            List<STGenericMaterial> genericMats = model.GetMaterials();
            genericMats = genericMats.OrderBy(x => GetMaterialIndex(x.Name)).ToList();

            //Create a new material and assign the first tev stage mapped textures
            foreach (var material in genericMats)
            {
                var mat = new Material();
                if (material.TextureMaps.Count > 0)
                {
                    string name = material.TextureMaps[0].Name;
                    int index = model.Textures.FindIndex(x => x.Name == name);
                    int samplerIndex = samplers.FindIndex(x => x.TextureIndex == index);
                    if (samplerIndex != -1)
                    {
                        mat.TevStages[0].Unknown = (ushort)0;
                        mat.TevStages[0].SamplerIndex = (ushort)samplerIndex;
                    }
                }
                materials.Add(mat);
            }

            if (genericMats.Count == 0) {
                var mat = new Material();
                materials.Add(mat);
            }

            //Create shapes and draw elements
            int packetIndex = 0;
            mdl.FileHeader.FaceCount = 0;
            foreach (var mesh in model.Meshes)
            {
                int materialIndex = 0;
                if (mesh.PolygonGroups[0].Material != null)
                    materialIndex = genericMats.IndexOf(mesh.PolygonGroups[0].Material);

                List<ushort> boneIndices = new List<ushort>();

                //Create packets to store our vertex data
                //Packets will split based on joint indices
                //A single packet can hold up to 10 indices
                //There will be either rigid indices (directly rigs to joints)
                //Or smooth indices (rigs to weight table. Indexes higher than joint count)
                List<ShapePacket> shapePackets = new List<ShapePacket>();
                ShapePacket packet = new ShapePacket();
                shapePackets.Add(packet);

                int vindex = 0;

                mdl.FileHeader.FaceCount += (ushort)(mesh.PolygonGroups.Sum(x => x.Faces.Count) / 3);

                var group = mesh.PolygonGroups[0];
                for (int v = 0; v < group.Faces.Count; v += 3)
                {
                    //Check the current triangle and find the max amount of indices currently in use.
                    int maxBoneIndices = boneIndices.Count;
                    for (int i = 0; i < 3; i++)
                    {
                        var vertex = mesh.Vertices[(int)group.Faces[v + (2 - i)]];
                        for (int j = 0; j < vertex.BoneIndices.Count; j++)
                        {
                            int index = vertex.BoneIndices[j];

                            if (!boneIndices.Contains((ushort)index) || vertex.BoneIndices.Count > 1)
                                maxBoneIndices++;
                        }
                    }

                    //Reset the bone indices depending on how many are used currently
                    if (maxBoneIndices > 9)
                    {
                        //Create a new packet to store additional indices
                        boneIndices = new List<ushort>();
                        packet = new ShapePacket();
                        shapePackets.Add(packet);
                    }

                    ShapePacket.DrawList drawList = new ShapePacket.DrawList();
                    drawList.OpCode = (byte)GXOpCodes.DRAW_TRIANGLES;
                    packet.DrawLists.Add(drawList);



                    for (int i = 0; i < 3; i++)
                    {
                        ShapePacket.VertexGroup vertexGroup = new ShapePacket.VertexGroup();
                        drawList.Vertices.Add(vertexGroup);

                        var vertex = mesh.Vertices[(int)group.Faces[v + (2 - i)]];

                        //Round the values
                        Vector3 pos = vertex.Position;
                        Vector3 nrm = vertex.Normal;

                        if (vertex.TexCoords.Length > 0)
                        {

                            if (!texCoords.Contains(vertex.TexCoords[0]))
                                texCoords.Add(vertex.TexCoords[0]);

                            vertexGroup.TexCoordIndex = (short)texCoords.IndexOf(vertex.TexCoords[0]);
                        }

                        //Note the bone indices list will store the real index
                        //The vertex data will just index the bone indices list instead
                        if (vertex.BoneIndices.Count == 1)
                        {
                            ushort index = (ushort)vertex.BoneIndices[0];
                            if (!boneIndices.Contains(index))
                                boneIndices.Add(index);

                            //Index our rigid skinning index from the index list of the current packet
                            vertexGroup.MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);
                            vertexGroup.Tex0MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);
                            vertexGroup.Tex1MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);

                            //Rigid indices require vertices to be inversed by the matrix
                            var matrix = matrices[index];
                            matrix.Transpose();
                            pos = Vector3.TransformPosition(pos, matrix);
                            nrm = Vector3.TransformNormal(nrm, matrix);
                        }
                        else if (vertex.BoneIndices.Count > 1)
                        {
                            vertex.SortBoneIndices();

                            //Create a weight entry used to index our weight table
                            var weightEntry = new MDL_Parser.Weight();
                            for (int j = 0; j < vertex.BoneWeights.Count; j++)
                                weightEntry.Weights.Add(vertex.BoneWeights[j]);
                            for (int j = 0; j < vertex.BoneIndices.Count; j++)
                                weightEntry.JointIndices.Add(vertex.BoneIndices[j]);

                            MDL_Parser.Weight existingWeight = null;
                            for (int w = 0; w < weights.Count; w++)
                            {
                                int matchedWeights = 0;
                                for (int j = 0; j < vertex.BoneIndices.Count; j++)
                                {
                                    int jointIndex = weights[w].JointIndices.IndexOf(vertex.BoneIndices[j]);
                                    if (jointIndex == -1)
                                        continue;

                                    if (weights[w].Weights[jointIndex] == vertex.BoneWeights[j])
                                    {
                                        matchedWeights++;
                                    }
                                }
                                if (matchedWeights == vertex.BoneIndices.Count)
                                    existingWeight = weights[w];
                            }
                            //Find an existing weight table entry that matches
                            if (existingWeight == null)
                            {
                                existingWeight = weightEntry;
                                weights.Add(existingWeight);
                            }

                            //Index our weight table entry
                            //These indices shift by rigid index
                            ushort rigidIndex = (ushort)nodes.Count;
                            ushort index = (ushort)(weights.IndexOf(existingWeight) + rigidIndex);

                            if (!boneIndices.Contains(index))
                                boneIndices.Add(index);

                            //Index our smooth skinning index from the index list of the current packet
                            vertexGroup.MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);
                            vertexGroup.Tex0MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);
                            vertexGroup.Tex1MatrixIndex = (sbyte)(boneIndices.IndexOf(index) * 3);
                        }
                        else if (vertex.BoneIndices.Count == 0)
                        {
                            if (!boneIndices.Contains(0))
                                boneIndices.Add(0);

                            vertexGroup.MatrixIndex = 0;
                            vertexGroup.Tex0MatrixIndex = 0;
                            vertexGroup.Tex1MatrixIndex = 0;
                        }

                        if (!positions.Contains(pos))
                            positions.Add(pos);
                        if (!normals.Contains(nrm))
                            normals.Add(nrm);

                        vertexGroup.PositionIndex = (short)positions.IndexOf(pos);
                        vertexGroup.NormalIndex = (short)normals.IndexOf(nrm);

                        //Update the matrix indices
                        for (int j = 0; j < boneIndices.Count; j++)
                            packet.MatrixIndices[j] = boneIndices[j];

                        packet.MatrixIndicesCount = (ushort)boneIndices.Count;
                        vindex++;
                    }
                }

                elements.Add(new DrawElement()
                {
                    ShapeIndex = (ushort)shapes.Count,
                    MaterialIndex = (ushort)materialIndex,
                });
                shapes.Add(new Shape()
                {
                    PacketBeginIndex = (ushort)packetIndex,
                    PacketCount = (ushort)shapePackets.Count,
                });
                packetIndex += shapePackets.Count;
                packets.AddRange(shapePackets);
            }

            foreach (var packet in packets) {
                packet.Data = packet.CreateDrawList(packet.DrawLists, false,
                    normals.Count > 0, texCoords.Count > 0, colors.Count > 0);
                packet.DataSize = (uint)packet.Data.Length;
            }

            var lodPackets = new List<ShapePacket>();
            foreach (var packet in packets) {
                var lodPacket = new ShapePacket();
                lodPacket.MatrixIndices = packet.MatrixIndices;
                lodPacket.MatrixIndicesCount = packet.MatrixIndicesCount;
                lodPacket.Data = packet.CreateDrawList(packet.DrawLists, true,
                    normals.Count > 0, texCoords.Count > 0, colors.Count > 0);
                lodPacket.DataSize = (uint)lodPacket.Data.Length;
                lodPackets.Add(lodPacket);
            }

            packets.AddRange(lodPackets);
            mdl.Matrix4Table = new Matrix4[matrices.Count];
            for (int i = 0; i < matrices.Count; i++) {
                mdl.Matrix4Table[i] = matrices[i];
            }

            mdl.Materials = materials.ToArray();
            mdl.Nodes = nodes.ToArray();
            mdl.Samplers = samplers.ToArray();
            mdl.Colors = colors.ToArray();
            mdl.Positions = positions.ToArray();
            mdl.TexCoords = texCoords.ToArray();
            mdl.Normals = normals.ToArray();
            mdl.DrawElements = elements.ToArray();
            mdl.Shapes = shapes.ToArray();
            mdl.ShapePackets = packets.ToArray();
            mdl.Weights = weights.ToArray();
            mdl.Textures = textures.ToArray();
            mdl.LODPositions = new Vector3[0];
            mdl.LODNormals = new Vector3[0];

            Header = mdl;
        }

        public string ExportSamplers()
        {
            SamplerOrder samplerList = new SamplerOrder();
            foreach (var sampler in Header.Samplers) {
                samplerList.Samplers.Add(new SamplerConvert()
                {
                    TextureIndex = sampler.TextureIndex,
                    MagFilter = sampler.MagFilter,
                    MinFilter = sampler.MinFilter,
                    WrapModeU = sampler.WrapModeU,
                    WrapModeV = sampler.WrapModeV,
                });
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(samplerList, Newtonsoft.Json.Formatting.Indented);
        }

        public void ReplaceSamplers(string text)
        {
            var convertedSamplers = Newtonsoft.Json.JsonConvert.DeserializeObject
                        <SamplerOrder>(text);

            Sampler[] samplers = new Sampler[convertedSamplers.Samplers.Count];
            for (int i = 0; i < convertedSamplers.Samplers.Count; i++)
            {
                samplers[i] = new Sampler()
                {
                    MagFilter = convertedSamplers.Samplers[i].MagFilter,
                    MinFilter = convertedSamplers.Samplers[i].MinFilter,
                    WrapModeU = convertedSamplers.Samplers[i].WrapModeU,
                    WrapModeV = convertedSamplers.Samplers[i].WrapModeV,
                    TextureIndex = convertedSamplers.Samplers[i].TextureIndex,
                };
            }
            Header.Samplers = samplers.ToArray();
        }

        public string ExportMaterial(DrawElement drawElement)
        {
            ushort nodeIdex = 0;
            foreach (var node in Header.Nodes)
            {
                for (int i = 0; i < node.ShapeCount; i++)
                {
                    if (node.ShapeIndex + i == drawElement.ShapeIndex)
                        nodeIdex = node.NodeIndex;
                }
            }

            var shape = Header.Shapes[drawElement.ShapeIndex];
            var material = Header.Materials[drawElement.MaterialIndex];
            var convertedMat = new MaterialConvert();
            convertedMat.AlphaFlags = material.AlphaFlags;
            convertedMat.DiffuseR = material.Color.R;
            convertedMat.DiffuseG = material.Color.G;
            convertedMat.DiffuseB = material.Color.B;
            convertedMat.DiffuseA = material.Color.A;
            convertedMat.Unknown1 = material.Unknown1;
            convertedMat.Unknown2 = material.Unknown3;
            convertedMat.MeshSettings = new ShapeFlags()
            {
                NormalsFlags = shape.NormalFlags,
                Unknown1 = shape.Unknown1,
                Unknown2 = shape.Unknown2,
                Unknown3 = shape.Unknown3,
                NodeIndex = nodeIdex,
            };

            TevStageConvert[] stages = new TevStageConvert[material.NumTevStages];
            convertedMat.TevStages = stages;
            for (int i = 0; i < material.NumTevStages; i++) {
                stages[i] = new TevStageConvert();
                stages[i].SamplerIndex = material.TevStages[i].SamplerIndex;
                stages[i].Unknown = material.TevStages[i].Unknown;
                stages[i].Values = material.TevStages[i].Unknowns2;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(convertedMat, Newtonsoft.Json.Formatting.Indented);
        }       

        public ushort ReplaceMaterial(string text, DrawElement drawElement)
        {
            var convertedMat = Newtonsoft.Json.JsonConvert.DeserializeObject
                <MaterialConvert>(text);

            var currentMaterial = Header.Materials[drawElement.MaterialIndex];
            currentMaterial.Color = new STColor8(
                convertedMat.DiffuseR, 
                convertedMat.DiffuseG, 
                convertedMat.DiffuseB, 
                convertedMat.DiffuseA);
            currentMaterial.AlphaFlags = convertedMat.AlphaFlags;
            currentMaterial.Unknown1 = convertedMat.Unknown1;
            currentMaterial.Unknown3 = convertedMat.Unknown2;

            List<Sampler> samplers = Header.Samplers.ToList();

            if (convertedMat.TevStages != null) {
                currentMaterial.NumTevStages = (byte)convertedMat.TevStages.Length;
                for (int i = 0; i < currentMaterial.NumTevStages; i++) {
                    if (currentMaterial.TevStages.Length <= i)
                        break;

                    var stage = convertedMat.TevStages[i];
                    var currentStage  = currentMaterial.TevStages[i];

                    currentStage.Unknown = stage.Unknown;
                    currentStage.Unknowns2 = stage.Values;
                }
            }

            var meshSettings = convertedMat.MeshSettings;

            Header.Samplers = samplers.ToArray();
            Header.Materials[drawElement.MaterialIndex] = currentMaterial;

            Header.Shapes[drawElement.ShapeIndex].NormalFlags = meshSettings.NormalsFlags;
            Header.Shapes[drawElement.ShapeIndex].Unknown1 = meshSettings.Unknown1;
            Header.Shapes[drawElement.ShapeIndex].Unknown2 = meshSettings.Unknown2;
            Header.Shapes[drawElement.ShapeIndex].Unknown3 = meshSettings.Unknown3;

            return meshSettings.NodeIndex;
        }

        public class SamplerOrder
        {
            public List<SamplerConvert> Samplers = new List<SamplerConvert>();
        }

        public class SamplerConvert
        {
            public ushort TextureIndex;
            public byte WrapModeU;
            public byte WrapModeV;
            public byte MinFilter;
            public byte MagFilter;
        }

        class ShapeFlags
        {
            public byte NormalsFlags { get; set; }
            public byte Unknown1 { get; set; }
            public byte Unknown2 { get; set; }
            public byte Unknown3 { get; set; }

            public ushort NodeIndex { get; set; } = 0;
        }

        class MaterialConvert
        {
            public ShapeFlags MeshSettings { get; set; }

            public byte DiffuseR { get; set; }
            public byte DiffuseG { get; set; }
            public byte DiffuseB { get; set; }
            public byte DiffuseA { get; set; }

            public byte AlphaFlags { get; set; }

            public ushort Unknown1 { get; set; }
            public byte Unknown2 { get; set; }

            public TevStageConvert[] TevStages { get; set; }
        }

        class TevStageConvert
        {
            public ushort SamplerIndex { get; set; }
            public ushort Unknown { get; set; }
            public float[] Values { get; set; }
        }

        private void CreateNodeGraph(List<Node> nodes, STBone bone, bool isSibling = false)
        {
            int currentIndex = nodes.Count;

            //Add a node to represent our node hiearchy
            //Bones have no shape/draw references. Indices are always the last count used.
            nodes.Add(new Node() {
                NodeIndex = (ushort)currentIndex,
                ChildIndex = bone.Children.Count > 0 ? (ushort)1 : (ushort)0,
            });

            for (int i = 0; i < bone.Children.Count; i++) {
                //Check the child if there is a sibling next to it
                //Sibling indices are used for parenting. 
                //These will chain together multiple nodes that have the same parenting

                //Check if there is children that go after the current child
                bool sibling = i < bone.Children.Count - 1;
                CreateNodeGraph(nodes, bone.Children[i], sibling);
            }
           
            int siblingStart = 0;
            if (isSibling)  //Set a sibling index relative to our current index
                siblingStart = nodes.Count - currentIndex;

            nodes[currentIndex].SiblingIndex = (ushort)siblingStart;
        }

        private void TraverseNodeGraph(STSkeleton skeleton, int index, int parentIndex = -1)
        {
            Node node = Header.Nodes[index];
            skeleton.Bones[index].ParentIndex = parentIndex;

            if (node.ChildIndex > 0) //Advanced to next node
                TraverseNodeGraph(skeleton, index + node.ChildIndex, index);
            if (node.SiblingIndex > 0) //Connect a sibling from the current index
                TraverseNodeGraph(skeleton, index + node.SiblingIndex, parentIndex);
        }

        private STGenericMaterial CreateMaterial(Material material, int index)
        {
            MDL_Material mat = new MDL_Material();
            mat.Name = $"Material{index}";
            mat.TintColor = material.Color;

            if (material.TevStages[0].SamplerIndex != ushort.MaxValue)
            {
                var texturObj = Header.Samplers[material.TevStages[0].SamplerIndex];
                var tex = Textures[texturObj.TextureIndex];

                STTextureWrapMode wrapModeU = ConvertWrapMode(texturObj.WrapModeU);
                STTextureWrapMode wrapModeV = ConvertWrapMode(texturObj.WrapModeV);

                mat.TextureMaps.Add(new STGenericTextureMap()
                {
                    Name = tex.Name,
                    WrapU = wrapModeU,
                    WrapV = wrapModeV,
                    Type = STTextureType.Diffuse,
                });
            }

            return mat;
        }

        private static STTextureWrapMode ConvertWrapMode(byte value)
        {
            switch ((Decode_Gamecube.WrapModes)value)
            {
                case Decode_Gamecube.WrapModes.Repeat:
                    return STTextureWrapMode.Repeat;
                case Decode_Gamecube.WrapModes.MirroredRepeat:
                    return STTextureWrapMode.Mirror;
                case Decode_Gamecube.WrapModes.ClampToEdge:
                    return STTextureWrapMode.Clamp;
                default:
                    return STTextureWrapMode.Repeat;
            }
        }

        public class MDL_Material : STGenericMaterial
        {
            public STColor8 TintColor { get; set; }
        }

        private List<STVertex> ConvertTriFans(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            int vertexId = 0;
            int firstVertex = vertexId;

            for (int index = 0; index < 3; index++)
                outVertices.Add(vertices[index]);

            for (int index = 2; index < vertices.Count; index++)
            {
                var vert1 = vertices[firstVertex];
                var vert2 = vertices[index - 1];
                var vert3 = vertices[index];

                if (!vert1.Position.Equals(vert2.Position) &&
                    !vert2.Position.Equals(vert3.Position) &&
                    !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private List<STVertex> ConvertTriStrips(List<STVertex> vertices)
        {
            List<STVertex> outVertices = new List<STVertex>();
            for (int index = 2; index < vertices.Count; index++)
            {
                bool isEven = (index % 2 != 1);

                var vert1 = vertices[index - 2];
                var vert2 = isEven ? vertices[index] : vertices[index - 1];
                var vert3 = isEven ? vertices[index - 1] : vertices[index];

                if (!vert1.Position.Equals(vert2.Position) &&
                    !vert2.Position.Equals(vert3.Position) &&
                    !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert2);
                    outVertices.Add(vert3);
                    outVertices.Add(vert1);
                }
            }
            return outVertices;
        }

        private STVertex ToVertex(ShapePacket.VertexGroup drawList, ref Matrix4 transform)
        {
            Vector3 position = Header.Positions[drawList.PositionIndex];
            Vector3 normal = new Vector3();
            Vector2 texCoord = new Vector2();
            Vector4 color = Vector4.One;

            List<int> boneIndices = new List<int>();
            List<float> boneWeights = new List<float>();

            if (drawList.NormalIndex != -1)
                normal = Header.Normals[drawList.NormalIndex];
            if (drawList.TexCoordIndex != -1)
                texCoord = Header.TexCoords[drawList.TexCoordIndex];
            if (drawList.ColorIndex != -1)
                color = Header.Colors[drawList.ColorIndex];

            //Rigid matrices start first, 0 - joint count
            //Skinned ones are after and will index the weight table instead of directly rigging to a matrix
            if (drawList.MatrixDataIndex >= Header.FileHeader.JointCount)
            {
                var weightIndex = drawList.MatrixDataIndex - Header.FileHeader.JointCount;

                var weights = Header.Weights[weightIndex];
                for (int i = 0; i < weights.JointIndices.Count; i++)
                {
                    boneIndices.Add(weights.JointIndices[i]);
                    boneWeights.Add(weights.Weights[i]);
                }
            }  //If the matrix is rigid and used then bind it directly to the joint with a weight of 1
            else if (drawList.MatrixIndex != -1)
            {
                boneIndices.Add(drawList.MatrixDataIndex);
                boneWeights.Add(1);
            } 

            position = Vector3.TransformPosition(position, transform);
            normal = Vector3.TransformNormal(normal, transform);

            return new STVertex()
            {
                Position = position,
                Normal = normal,
                BoneIndices = boneIndices,
                BoneWeights = boneWeights,
                TexCoords = new Vector2[1] { texCoord },
                Colors = new Vector4[1] { color },
            };
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public class Texture : STGenericTexture
        {
            public byte[] ImageData;

            public Texture(TextureHeader header)
            {
                Width = header.Width;
                Height = header.Height;
                MipCount = 1;
                Platform = new GamecubeSwizzle(header.Format);
                ImageData = header.ImageData;
            }

            public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0)
            {
                return ImageData;
            }

            public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
