using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Toolbox.Core;
using Toolbox.Core.IO;
using Collada141;
using System.IO;

namespace Toolbox.Core.Collada
{
    public class DAE
    {
        public STGenericScene Scene = new STGenericScene();

        public class ExportSettings
        {
            public bool SuppressConfirmDialog = false;
            public bool OptmizeZeroWeights = true;
            public bool UseVertexColors = true;
            public bool FlipTexCoordsVertical = true;
            public bool ExportTextures = true;
            public bool OnlyExportRiggedBones = false;
            public bool TransformColorUVs = false;
            public bool RemoveDuplicateVertices = true;
            public bool OptmizeIndices = true;

            public bool UseTextureChannelComponents = true;

            public Version FileVersion = new Version();

            public ProgramPreset Preset = ProgramPreset.NONE;

            public string ImageExtension = ".png";
            public string ImageFolder = "";
        }

        public class ImportSettings
        {
            public bool FlipUVsVertical { get; set; } = true;

            public uint MaxSkinningCount = 4;
            public bool RemoveDuplicateVerts { get; set; } = true;
            public bool FixDuplicateNames { get; set; } = true;

            public string FolderPath { get; set; } = "";
        }

        public class Version
        {
            public int Major = 1;
            public int Minor = 4;
            public int Micro = 1;
        }

        public static STGenericScene Read(System.IO.Stream stream, string fileName, DAE.ImportSettings settings = null) {
            if (settings == null) settings = new ImportSettings();
            settings.FolderPath = Path.GetDirectoryName(fileName);

            return ColladaReader.Read(stream, settings);
        }

        public static STGenericScene Read(string fileName, DAE.ImportSettings settings = null) {
            if (settings == null) settings = new ImportSettings();
            settings.FolderPath = Path.GetDirectoryName(fileName);

            return ColladaReader.Read(fileName, settings);
        }

        public static void Export(string FileName, ExportSettings settings, STGenericMesh mesh)
        {
            Export(FileName, settings, new List<STGenericMesh>() { mesh },
                new List<STGenericMaterial>(), new List<STGenericTexture>());
        }

        public static void Export(string FileName, ExportSettings settings, IModelFormat model)
        {
            var genericModel = model.ToGeneric();

            Export(FileName, settings, genericModel.Meshes, genericModel.GetMaterials(),
                genericModel.Textures, genericModel.Skeleton);
        }

        public static void Export(string FileName, ExportSettings settings, STGenericModel model, List<STGenericTexture> Textures, STSkeleton skeleton = null, List<int> NodeArray = null)
        {
            Export(FileName, settings, model.Meshes, model.GetMaterials(), Textures, skeleton, NodeArray);
        }

        public static void Export(string FileName, ExportSettings settings,
            List<STGenericMesh> Meshes, List<STGenericMaterial> Materials,
            List<STGenericTexture> Textures, STSkeleton skeleton = null, List<int> NodeArray = null)
        {
            if (Materials == null)
                Materials = new List<STGenericMaterial>();

            if (settings.RemoveDuplicateVertices)
            {
                foreach (var mesh in Meshes)
                    mesh.RemoveDuplicateVertices();
            }

            Console.WriteLine($"DAE Materials {Materials.Count}");

            List<string> failedTextureExport = new List<string>();
            Dictionary<string, STGenericMaterial> MaterialRemapper = new Dictionary<string, STGenericMaterial>();

            using (ColladaWriter writer = new ColladaWriter(FileName, settings))
            {
                writer.WriteAsset();

                if (Materials.Count > 0)
                {
                    List<string> textureNames = new List<string>();
                    for (int i = 0; i < Textures?.Count; i++)
                    {
                        if (!textureNames.Contains(Textures[i].Name))
                            textureNames.Add(Textures[i].Name);

                        if (settings.ExportTextures)
                        {
                            try
                            {
                                var bitmap = Textures[i].GetBitmap();
                                if (bitmap != null)
                                {
                                    string textureName = Textures[i].Name;
                                    if (textureName.RemoveIllegaleFileNameCharacters() != textureName)
                                    {
                                        string properName = textureName.RemoveIllegaleFileNameCharacters();
                                        for (int m = 0; m < Materials?.Count; m++)
                                        {
                                            foreach (var tex in Materials[m].TextureMaps)
                                            {
                                                if (tex.Name == textureName)
                                                    tex.Name = properName;
                                            }
                                        }

                                        textureName = properName;
                                    }

                                    if (settings.ImageFolder != "")
                                        bitmap.Save($"{settings.ImageFolder}/{textureName}.png");
                                    else
                                        bitmap.Save($"{textureName}.png");
                                    bitmap.Dispose();

                                    GC.Collect();
                                }
                            }
                            catch (Exception ex)
                            {
                                failedTextureExport.Add(Textures[i].Name);
                            }
                        }
                    }

                    for (int i = 0; i < Materials.Count; i++)
                    {
                        if (Materials[i].Name == null)
                            Materials[i].Name = $"Material{i}";
                    }

                    List<Material> materials = new List<Material>();
                    foreach (var mat in Materials)
                    {
                        Material material = new Material();
                        material.Name = mat.Name;

                        if (!MaterialRemapper.ContainsKey(mat.Name)) {
                            MaterialRemapper.Add(mat.Name, mat);
                        }
                        else
                        {
                            string name = Utils.RenameDuplicateString(mat.Name, MaterialRemapper.Keys.ToList());
                            MaterialRemapper.Add(name, mat);
                            material.Name = name;
                        }

                        if (mat.DiffuseColor != null)
                            material.DiffuseColor = new float[4] {
                                mat.DiffuseColor.R / 255.0F, 
                                mat.DiffuseColor.G / 255.0F,
                                mat.DiffuseColor.B / 255.0F,
                                mat.DiffuseColor.A / 255.0F };

                        materials.Add(material);

                        foreach (var tex in mat.TextureMaps)
                        {
                            TextureMap texMap = new TextureMap();
                            texMap.Name = tex.Name;
                            if (tex.Type == STTextureType.Diffuse)
                                texMap.Type = PhongTextureType.diffuse;
                            else if (tex.Type == STTextureType.Normal)
                                texMap.Type = PhongTextureType.bump;
                            else if (tex.Type == STTextureType.Specular)
                                texMap.Type = PhongTextureType.specular;
                            else if (tex.Type == STTextureType.Emission)
                                texMap.Type = PhongTextureType.emission;
                            else
                                continue; //Skip adding unknown types

                            if (tex.WrapU == STTextureWrapMode.Repeat)
                                texMap.WrapModeS = SamplerWrapMode.WRAP;
                            else if (tex.WrapU == STTextureWrapMode.Mirror)
                                texMap.WrapModeS = SamplerWrapMode.MIRROR;
                            else if (tex.WrapU == STTextureWrapMode.Clamp)
                                texMap.WrapModeS = SamplerWrapMode.CLAMP;


                            if (tex.WrapV == STTextureWrapMode.Repeat)
                                texMap.WrapModeT = SamplerWrapMode.WRAP;
                            else if (tex.WrapV == STTextureWrapMode.Mirror)
                                texMap.WrapModeT = SamplerWrapMode.MIRROR;
                            else if (tex.WrapV == STTextureWrapMode.Clamp)
                                texMap.WrapModeT = SamplerWrapMode.CLAMP;


                            //If no textures are saved, still keep images references
                            //So the user can still dump textures after
                            if (Textures?.Count == 0 && !textureNames.Contains(texMap.Name))
                                textureNames.Add($"{texMap.Name}");

                            material.Textures.Add(texMap);
                        }
                    }

                    writer.WriteLibraryImages(textureNames.ToArray());

                    writer.WriteLibraryMaterials(materials);
                    writer.WriteLibraryEffects(materials);
                }
                else
                    writer.WriteLibraryImages();

                if (skeleton != null)
                {
                    for (int i = 0; i < skeleton.Bones.Count; i++)
                    {
                        if (skeleton.Bones[i].Name == null)
                            skeleton.Bones[i].Name = $"Bones{i}";
                    }

                    //Search for bones with rigging first
                    List<string> riggedBones = new List<string>();
                    if (settings.OnlyExportRiggedBones)
                    {
                        for (int i = 0; i < Meshes.Count; i++)
                        {
                            for (int v = 0; v < Meshes[i].Vertices.Count; v++)
                            {
                                var vertex = Meshes[i].Vertices[v];
                                for (int j = 0; j < vertex.BoneIndices.Count; j++)
                                {
                                    int id = -1;
                                    if (NodeArray != null && NodeArray.Count > vertex.BoneIndices[j])
                                    {
                                        id = NodeArray[vertex.BoneIndices[j]];
                                    }
                                    else
                                        id = vertex.BoneIndices[j];

                                    if (id < skeleton.Bones.Count && id != -1)
                                        riggedBones.Add(skeleton.Bones[id].Name);
                                }
                            }
                        }
                    }

                    foreach (var bone in skeleton.Bones)
                    {
                        if (settings.OnlyExportRiggedBones && !riggedBones.Contains(bone.Name))
                        {
                            Console.WriteLine("Skipping " + bone.Name);
                            continue;
                        }

                        //Set the inverse matrix
                        var inverse = skeleton.GetBoneTransform(bone).Inverted();
                        var transform = bone.GetTransform();

                        float[] Transform = new float[] {
                       transform.M11, transform.M21, transform.M31, transform.M41,
                       transform.M12, transform.M22, transform.M32, transform.M42,
                       transform.M13, transform.M23, transform.M33, transform.M43,
                       transform.M14, transform.M24, transform.M34, transform.M44
                        };

                        float[] InvTransform = new float[] {
                      inverse.M11, inverse.M21, inverse.M31, inverse.M41,
                      inverse.M12, inverse.M22, inverse.M32, inverse.M42,
                      inverse.M13, inverse.M23, inverse.M33, inverse.M43,
                      inverse.M14, inverse.M24, inverse.M34, inverse.M44
                        };

                        writer.AddJoint(bone.Name, bone.ParentIndex == -1 ? "" :
                            skeleton.Bones[bone.ParentIndex].Name, Transform, InvTransform,
                            new float[3] { bone.Position.X, bone.Position.Y, bone.Position.Z },
                            new float[3] { bone.EulerRotation.X, bone.EulerRotation.Y, bone.EulerRotation.Z },
                            new float[3] { bone.Scale.X, bone.Scale.Y, bone.Scale.Z });
                    }
                }

                for (int i = 0; i < Meshes.Count; i++)
                {
                    if (Meshes[i].Name == null)
                        Meshes[i].Name = $"Mesh{i}";
                }

                int meshIndex = 0;

                writer.StartLibraryGeometries();
                foreach (var mesh in Meshes)
                {
                    int[] IndexTable = null;
                    if (NodeArray != null)
                        IndexTable = NodeArray.ToArray();

                    writer.StartGeometry(mesh.Name);

                   /* if (mesh.MaterialIndex != -1 && Materials.Count > mesh.MaterialIndex)
                    {
                        writer.CurrentMaterial = Materials[mesh.MaterialIndex].Text;
                        Console.WriteLine($"MaterialIndex {mesh.MaterialIndex } {Materials[mesh.MaterialIndex].Text}");
                    }*/

                    if (settings.TransformColorUVs)
                    {
                        List<STVertex> transformedVertices = new List<STVertex>();
                        foreach (var poly in mesh.PolygonGroups)
                        {
                            var mat = poly.Material;
                            if (mat == null) continue;

                            var faces = poly.Faces;
                            for (int v = 0; v < poly.Faces.Count; v += 3)
                            {
                                if (faces.Count < v + 2)
                                    break;

                                var diffuse = mat.TextureMaps.FirstOrDefault(x => x.Type == STTextureType.Diffuse);
                                STTextureTransform transform = new STTextureTransform();
                                if (diffuse != null)
                                    transform = diffuse.Transform;

                                var vertexA = mesh.Vertices[(int)faces[v]];
                                var vertexB = mesh.Vertices[(int)faces[v + 1]];
                                var vertexC = mesh.Vertices[(int)faces[v + 2]];

                                if (!transformedVertices.Contains(vertexA))
                                {
                                    vertexA.TexCoords[0] = (vertexA.TexCoords[0] * transform.Scale) + transform.Translate;
                                    transformedVertices.Add(vertexA);
                                }
                                if (!transformedVertices.Contains(vertexB))
                                {
                                    vertexB.TexCoords[0] = (vertexB.TexCoords[0] * transform.Scale) + transform.Translate;
                                    transformedVertices.Add(vertexB);
                                }
                                if (!transformedVertices.Contains(vertexC))
                                {
                                    vertexC.TexCoords[0] = (vertexC.TexCoords[0] * transform.Scale) + transform.Translate;
                                    transformedVertices.Add(vertexC);
                                }
                            }
                        }
                    }

                    // collect sources
                    List<float> Position = new List<float>();
                    List<float> Normal = new List<float>();
                    List<float> UV1 = new List<float>();
                    List<float> UV2 = new List<float>();
                    List<float> UV3 = new List<float>();
                    List<float> Color = new List<float>();
                    List<float> Color2 = new List<float>();
                    List<int[]> BoneIndices = new List<int[]>();
                    List<float[]> BoneWeights = new List<float[]>();

                    bool HasNormals = false;
                    bool HasColors = false;
                    bool HasColors2 = false;
                    bool HasUV0 = false;
                    bool HasUV1 = false;
                    bool HasUV2 = false;
                    bool HasBoneIds = false;

                    mesh.OptimizeVertices();

                    foreach (var vertex in mesh.Vertices)
                    {
                        //Remove zero weights
                        if (settings.OptmizeZeroWeights)
                        {
                            float MaxWeight = 1;
                            for (int i = 0; i < 4; i++)
                            {
                                if (vertex.BoneWeights.Count <= i)
                                    continue;

                                if (vertex.BoneIndices.Count < i + 1)
                                {
                                    vertex.BoneWeights[i] = 0;
                                    MaxWeight = 0;
                                }
                                else
                                {
                                    float weight = vertex.BoneWeights[i];
                                    if (vertex.BoneWeights.Count == i + 1)
                                        weight = MaxWeight;

                                    if (weight >= MaxWeight)
                                    {
                                        weight = MaxWeight;
                                        MaxWeight = 0;
                                    }
                                    else
                                        MaxWeight -= weight;

                                    vertex.BoneWeights[i] = weight;
                                }
                            }
                        }


                        if (vertex.Normal != Vector3.Zero) HasNormals = true;
                        if (vertex.Colors.Length > 0 && settings.UseVertexColors) HasColors = true;
                        if (vertex.Colors.Length > 1 && settings.UseVertexColors) HasColors2 = true;
                        if (vertex.TexCoords.Length > 0) HasUV0 = true;
                        if (vertex.TexCoords.Length > 1) HasUV1 = true;
                        if (vertex.TexCoords.Length > 2) HasUV2 = true;
                        if (vertex.BoneIndices.Count > 0) HasBoneIds = true;

                        Position.Add(vertex.Position.X); Position.Add(vertex.Position.Y); Position.Add(vertex.Position.Z);
                        Normal.Add(vertex.Normal.X); Normal.Add(vertex.Normal.Y); Normal.Add(vertex.Normal.Z);

                        for (int i = 0; i < vertex.TexCoords.Length; i++)
                        {
                            var texCoord = vertex.TexCoords[i];
                            if (settings.FlipTexCoordsVertical)
                                texCoord = new Vector2(texCoord.X, 1 - texCoord.Y);

                            if (i == 0) {
                                UV1.Add(texCoord.X); UV1.Add(texCoord.Y);
                            }
                            if (i == 1) {
                                UV2.Add(texCoord.X); UV2.Add(texCoord.Y);
                            }
                            if (i == 2) {
                                UV3.Add(texCoord.X); UV3.Add(texCoord.Y);
                            }
                        }


                        if (vertex.Colors.Length > 0)
                            Color.AddRange(new float[] { vertex.Colors[0].X, vertex.Colors[0].Y, vertex.Colors[0].Z, vertex.Colors[0].W });
                        if (vertex.Colors.Length > 1)
                            Color2.AddRange(new float[] { vertex.Colors[1].X, vertex.Colors[1].Y, vertex.Colors[1].Z, vertex.Colors[1].W });

                        List<int> bIndices = new List<int>();
                        List<float> bWeights = new List<float>();
                        for (int b = 0; b < vertex.BoneIndices.Count; b++)
                        {
                            if (b > mesh.VertexSkinCount - 1)
                                continue;

                            if (vertex.BoneWeights.Count > b)
                            {
                                if (vertex.BoneWeights[b] == 0)
                                    continue;
                            }

                            int index = -1;
                            if (IndexTable != null)
                                index = (int)IndexTable[vertex.BoneIndices[b]];
                            else
                                index = (int)vertex.BoneIndices[b];

                            if (index != -1 && index < skeleton?.Bones.Count)
                                bIndices.Add(index);

                            //Some models may only use indices (single bind, rigid skin)
                            if (vertex.BoneWeights.Count > b)
                                bWeights.Add(vertex.BoneWeights[b]);
                            else
                                bWeights.Add(1);
                        }

                        if (bIndices.Count == 0 && mesh.BoneIndex != -1)
                        {
                            HasBoneIds = true;
                            bIndices.Add(mesh.BoneIndex);
                            bWeights.Add(1);
                        }

                        BoneIndices.Add(bIndices.ToArray());
                        BoneWeights.Add(bWeights.ToArray());
                    }

                    List<TriangleList> triangleLists = new List<TriangleList>();
                    if (mesh.PolygonGroups.Count > 0)
                    {
                        foreach (var group in mesh.PolygonGroups)
                        {
                            TriangleList triangleList = new TriangleList();

                            triangleLists.Add(triangleList);

                            STGenericMaterial material = new STGenericMaterial();

                            if (group.MaterialIndex != -1 && Materials.Count > group.MaterialIndex)
                                material = Materials[group.MaterialIndex];

                            if (group.Material != null)
                                material = group.Material;

                           if (MaterialRemapper.Values.Any(x => x == material))
                            {
                                var key = MaterialRemapper.FirstOrDefault(x => x.Value == material).Key;
                                triangleList.Material = key;
                            }
                           else if (material.Name != string.Empty)
                                triangleList.Material = material.Name;

                            List<uint> faces = new List<uint>();
                            if (group.PrimitiveType == STPrimitiveType.TriangleStrips)
                                faces = TriangleConverter.ConvertTriangleStripsToTriangles(group.Faces);
                            else
                                faces = group.Faces;

                            for (int i = 0; i < faces.Count; i++)
                                triangleList.Indices.Add(faces[i]);
                        }
                    }

                    // write sources
                    writer.WriteGeometrySource(mesh.Name, SemanticType.POSITION, Position.ToArray(), triangleLists.ToArray());

                    if (HasNormals)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.NORMAL, Normal.ToArray(), triangleLists.ToArray());

                    if (HasColors)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.COLOR, Color.ToArray(), triangleLists.ToArray(), 0);

                    if (HasColors2)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.COLOR, Color2.ToArray(), triangleLists.ToArray(), 1);

                    Console.WriteLine($"HasUV0 {HasUV0} {UV1.Count}");

                    if (HasUV0)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.TEXCOORD, UV1.ToArray(), triangleLists.ToArray(), 0);

                    if (HasUV1)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.TEXCOORD, UV2.ToArray(), triangleLists.ToArray(), 1);

                    if (HasUV2)
                        writer.WriteGeometrySource(mesh.Name, SemanticType.TEXCOORD, UV3.ToArray(), triangleLists.ToArray(), 2);

                    if (HasBoneIds)
                        writer.AttachGeometryController(BoneIndices, BoneWeights);

                    writer.EndGeometryMesh();
                }
                writer.EndGeometrySection();
            }
        }
    }
}
