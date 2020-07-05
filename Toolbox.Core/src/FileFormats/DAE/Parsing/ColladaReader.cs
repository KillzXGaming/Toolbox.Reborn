using System;
using System.Collections.Generic;
using System.Text;
using Collada141;
using OpenTK;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Toolbox.Core.IO;

namespace Toolbox.Core.Collada
{
    public class ColladaReader
    {
        public static STGenericScene Read(string fileName, DAE.ImportSettings settings = null) {
            return Read(COLLADA.Load(fileName), settings);
        }

        public static STGenericScene Read(Stream stream, DAE.ImportSettings settings = null) {
            return Read(COLLADA.Load(stream), settings);
        }

        static STGenericScene Read(COLLADA collada, DAE.ImportSettings settings)
        {
            if (settings == null) settings = new DAE.ImportSettings();

            string folder = settings.FolderPath;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            STGenericScene Scene = new STGenericScene();
            ColladaScene colladaScene = new ColladaScene(collada, settings);

            //Usually there is only one scene, but it can be possible some tools use multiple per model
            //Each one contains node hiearchies for bones and meshes
            foreach (var scene in colladaScene.scenes.visual_scene)
            {
                var model = new STGenericModel(scene.name);
                model.Textures = LoadTextures(folder, colladaScene).OrderBy(x => x.Name).ToList();
                Node Root = LoadScene(scene, model, colladaScene);
                Scene.Models.Add(model);

                if (colladaScene.materials != null)
                {
                    foreach (var mat in colladaScene.materials.material)
                        model.Materials.Add(LoadMaterial(colladaScene, mat));
                }
                else
                    model.Materials.Add(new STGenericMaterial() { Name = "Dummy" });

                if (model.Skeleton.Bones.Count == 0)
                    model.Skeleton.Bones.Add(new STBone(model.Skeleton, "root"));

                //Setup bone indices
                foreach (var mesh in model.Meshes)
                {
                    for (int v = 0; v < mesh.Vertices.Count; v++)
                    {
                        foreach (var name in mesh.Vertices[v].BoneNames)
                        {
                            int index = model.Skeleton.Bones.FindIndex(x => x.Name == name);
                            if (index != -1)
                                mesh.Vertices[v].BoneIndices.Add(index);
                        }
                    }

                    foreach (var group in mesh.PolygonGroups)
                    {
                        if (group.MaterialIndex < model.Materials.Count && group.MaterialIndex != -1)
                            group.Material = model.Materials[group.MaterialIndex];
                    }
                }

                if (settings.FixDuplicateNames)
                {
                    //Adjust duplicate names
                    /*    foreach (var mesh in model.Meshes)
                        {
                            var names = model.Meshes.Select(x => x.Name).ToList();
                            mesh.Name = Utility.RenameDuplicateString(names, mesh.Name, 0, 2);
                        }

                        foreach (var mat in model.Materials)
                        {
                            var names = model.Materials.Select(x => x.Name).ToList();
                            mat.Name = Utility.RenameDuplicateString(names, mat.Name, 0, 2);
                        }

                        foreach (var bone in model.Skeleton.Bones)
                        {
                            var names = model.Skeleton.Bones.Select(x => x.Name).ToList();
                            bone.Name = Utility.RenameDuplicateString(names, bone.Name, 0, 2);
                        }*/
                }
            }

            sw.Stop();
            Console.WriteLine("DAE Elapsed={0}", sw.Elapsed);

            return Scene;
        }


        public class ColladaScene
        {
            public DAE.ImportSettings Settings;

            public library_geometries geometries;
            public library_images images;
            public library_visual_scenes scenes;
            public library_effects effects;
            public library_controllers controllers;
            public library_materials materials;

            public Dictionary<string, effect> effectLookup = new Dictionary<string, effect>();

            public UpAxisType UpAxisType = UpAxisType.Y_UP;
            public assetUnit UintSize;

            public ColladaScene(COLLADA collada, DAE.ImportSettings settings)
            {
                Settings    = settings;
                geometries  = FindLibraryItem<library_geometries>(collada.Items);
                images      = FindLibraryItem<library_images>(collada.Items);
                scenes      = FindLibraryItem<library_visual_scenes>(collada.Items);
                effects     = FindLibraryItem<library_effects>(collada.Items);
                controllers = FindLibraryItem<library_controllers>(collada.Items);
                materials   = FindLibraryItem<library_materials>(collada.Items);

                if (effects != null)
                {
                    for (int i = 0; i < effects.effect.Length; i++)
                        effectLookup.Add(effects.effect[i].id, effects.effect[i]);
                }

                if (collada.asset != null) {
                    UpAxisType = collada.asset.up_axis;
                    UintSize = collada.asset.unit;
                }
            }

            private static T FindLibraryItem<T>(object[] items) {
                var item = Array.Find(items, x => x.GetType() == typeof(T));
                return (T)item;
            }
        }

        static List<STGenericTexture> LoadTextures(string folder, ColladaScene scene)
        {
            if (scene.images == null) return new List<STGenericTexture>();

            List<STGenericTexture> textures = new List<STGenericTexture>();
            foreach (var image in scene.images.image)
            {
                if (image.Item is string) {
                    string path = (string)image.Item;
                    if (path.StartsWith("file://"))
                        path = path.Substring(7, path.Length - 7);

                    if (File.Exists($"{folder}/{path}"))
                    {
                        var textureFile = LoadImage($"{folder}/{path}");
                        textureFile.Name = image.id;
                        textures.Add(textureFile);
                    }
                    if (File.Exists(path))
                    {
                        var textureFile = LoadImage(path);
                        textureFile.Name = image.id;
                        textures.Add(textureFile);
                    }
                }
                if (image.Item is byte[]) //Embedded. Todo
                {

                }
            }


            return textures;
        }

        static STGenericTexture LoadImage(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".dds")
                return new DDS(path);
            return new GenericBitmapTexture(path);
        }

        public static STGenericMaterial LoadMaterial(ColladaScene scene, material daeMat)
        {
            STGenericMaterial mat = new STGenericMaterial();
            mat.Name = daeMat.id;

            if (daeMat.instance_effect != null) {
                var effectid = daeMat.instance_effect.url.Remove(0, 1);
                if (scene.effectLookup.ContainsKey(effectid))
                {
                    var effect = scene.effectLookup[effectid];
                    if (effect.Items == null)
                        return mat;

                    foreach (var item in effect.Items)
                    {
                        if (item.Items == null)
                            continue;

                        foreach (var param in item.Items)
                        {
                            if (param is common_newparam_type) {
                                var newparam = (common_newparam_type)param;
                                if (newparam.ItemElementName == ItemChoiceType.surface)
                                {
                                    var surface = newparam.Item as fx_surface_common;
                                    var name = surface.init_from[0].Value;
                                    mat.TextureMaps.Add(new STGenericTextureMap()
                                    {
                                        Name = name,
                                        Type = STTextureType.Diffuse,
                                    });
                                }
                            }
                            if (param is technique){
                                var technique = (technique)param;
                            }
                        }
                    }
                }
                else
                    Console.WriteLine($"cannot find id! {effectid}");
            }
            return mat;
        }

        static Dictionary<int, string> NodeInstanceTransform = new Dictionary<int, string>();

        public static Node LoadScene(visual_scene visualScene,
            STGenericModel model, ColladaScene colladaScene)
        {
            Node node = new Node(null);
            node.Name = visualScene.name;

            foreach (node child in visualScene.node)
                node.Children.Add(LoadHiearchy(node, child, model, colladaScene));

            //Transform all meshes by node transform
            foreach (var child in node.Children)
                TransformMeshInstanced(model, child);

            NodeInstanceTransform.Clear();

            return node;
        }

        static void TransformMeshInstanced(STGenericModel model, Node parent)
        {
            if (NodeInstanceTransform.Any(x => x.Value == $"#{parent.ID}")) {
                foreach (var node in NodeInstanceTransform.Where(x => x.Value == $"#{parent.ID}"))
                {
                    var index = node.Key;
                    for (int v = 0; v < model.Meshes[index].Vertices.Count; v++)
                    {
                        model.Meshes[index].Vertices[v].Position = Vector3.TransformPosition(
                            model.Meshes[index].Vertices[v].Position, parent.Transform);

                        model.Meshes[index].Vertices[v].Normal = Vector3.TransformNormal(
                       model.Meshes[index].Vertices[v].Normal, parent.Transform);
                    }
                }
            }

            foreach (var child in parent.Children)
                TransformMeshInstanced(model, child);
        }

        public static Node LoadHiearchy(Node parent, node daeNode,
            STGenericModel model, ColladaScene colladaScene)
        {
            Node node = new Node(parent);
            node.Name = daeNode.name;
            node.ID = daeNode.id;
            node.Type = daeNode.type;
            node.Transform = DaeUtility.GetMatrix(daeNode.Items) * parent.Transform;

            if (daeNode.instance_geometry != null)
            {
                geometry geom = DaeUtility.FindGeoemertyFromNode(daeNode, colladaScene.geometries);
                model.Meshes.Add(LoadMeshData(colladaScene, node, geom, colladaScene.materials));
            }
            if (daeNode.instance_controller != null)
            {
                foreach (var insance_controller in daeNode.instance_controller)
                {
                    var skeletonNode = insance_controller.skeleton[0];

                    NodeInstanceTransform.Add(model.Meshes.Count, skeletonNode);

                    controller controller = DaeUtility.FindControllerFromNode(insance_controller, colladaScene.controllers);
                    geometry geom = DaeUtility.FindGeoemertyFromController(controller, colladaScene.geometries);
                    model.Meshes.Add(LoadMeshData(colladaScene, node, geom, colladaScene.materials, controller));
                }
            }
            try
            {
            
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert mesh {daeNode.name} \n {ex.ToString()}");
            }

            //Find the root bone
            if (node.Type == NodeType.JOINT) {
                //Apply axis rotation
                Matrix4 boneTransform = Matrix4.Identity;
                if (colladaScene.UpAxisType == UpAxisType.Y_UP) {
                  //  boneTransform = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
                }
                if (colladaScene.UintSize != null && colladaScene.UintSize.meter != 1)
                {
                    //var scale = ApplyUintScaling(colladaScene, new Vector3(1));
                   // boneTransform *= Matrix4.CreateScale(scale);
                }

                LoadBoneHiearchy(daeNode, model, null, ref boneTransform);

                if (daeNode.node1 != null)
                {
                    foreach (node child in daeNode.node1)
                        node.Children.Add(LoadBoneNodeHiearchy(node, child, ref boneTransform));
                }
            }
            else if (daeNode.node1 != null) {
                foreach (node child in daeNode.node1)
                    node.Children.Add(LoadHiearchy(node, child, model, colladaScene));
            }

            return node;
        }

        private static Node LoadBoneNodeHiearchy(Node parent, node daeNode, ref Matrix4 parentTransform)
        {
            Node node = new Node(parent);
            node.Name = daeNode.name;
            node.Type = daeNode.type;
            node.ID = daeNode.id;
            node.Transform = DaeUtility.GetMatrix(daeNode.Items) * parentTransform;

            if (daeNode.node1 != null)
            {
                foreach (node child in daeNode.node1)
                    node.Children.Add(LoadBoneNodeHiearchy(node, child, ref parentTransform));
            }

            return node;
        }

        private static STBone LoadBoneHiearchy(node daeNode, STGenericModel model,
            STBone boneParent, ref Matrix4 parentTransform)
        {
            STBone bone = new STBone(model.Skeleton, daeNode.name);
            model.Skeleton.Bones.Add(bone);

            var transform = DaeUtility.GetMatrix(daeNode.Items) * parentTransform;
            bone.Position = transform.ExtractTranslation();
            bone.Scale = transform.ExtractScale();
            bone.Rotation = transform.ExtractRotation();
            bone.Parent = boneParent;

            //Reset the parent transform for children. We only need to apply the parent root transform 
            parentTransform = Matrix4.Identity;

            if (daeNode.node1 != null)
            {
                foreach (node child in daeNode.node1)
                    bone.Children.Add(LoadBoneHiearchy(child, model, bone,ref parentTransform));
            }
            return bone;
        }

        private static STGenericMesh LoadMeshData(ColladaScene scene, Node node,
            geometry geom, library_materials materials, controller controller = null)
        {
            mesh daeMesh = geom.Item as mesh;

            STGenericMesh mesh = new STGenericMesh();
            mesh.Vertices = new List<STVertex>();
            mesh.Name = geom.name;

            var boneWeights = ParseWeightController(controller, scene);
            foreach (var item in daeMesh.Items) {
                //Poly lists can control specific amounts of indices for primitive types like quads
                if (item is polylist)
                {
                    var poly = item as polylist;
                    ConvertPolygon(scene, mesh, daeMesh, poly.input,
                        boneWeights, materials, poly.material, poly.p, (int)poly.count, poly.vcount);
                }
                else if (item is triangles)
                {
                    var triangle = item as triangles;
                    ConvertPolygon(scene, mesh, daeMesh, triangle.input,
                       boneWeights, materials, triangle.material, triangle.p, (int)triangle.count);
                }
            }

            foreach (var vertex in mesh.Vertices)
            {
                if (scene.Settings.FlipUVsVertical)
                {
                    for (int i = 0; i < vertex.TexCoords.Length; i++)
                        vertex.TexCoords[i] = new Vector2(vertex.TexCoords[i].X, 1 - vertex.TexCoords[i].Y);
                }

                 vertex.Position = Vector3.TransformPosition(vertex.Position, node.Transform);
                 vertex.Normal = Vector3.TransformNormal(vertex.Normal, node.Transform);
            }

            if (controller != null)
            {
                skin skin = controller.Item as skin;
                var bindMatrix = CreateBindMatrix(skin.bind_shape_matrix);
                for(int v = 0; v < mesh.Vertices.Count; v++)
                {
                    mesh.Vertices[v].Position = Vector3.TransformPosition(mesh.Vertices[v].Position, bindMatrix);
                    mesh.Vertices[v].Normal = Vector3.TransformNormal(mesh.Vertices[v].Normal, bindMatrix);
                }
            }

            return mesh;
        }

        private static void ConvertPolygon(ColladaScene scene,STGenericMesh mesh, mesh daeMesh,
            InputLocalOffset[] inputs, List<BoneWeight[]> boneWeights, 
            library_materials materials, string material, string polys, int polyCount, string vcount = "")
        {
            List<uint> faces = new List<uint>();

            STPolygonGroup group = new STPolygonGroup();
            mesh.PolygonGroups.Add(group);
            group.MaterialIndex = DaeUtility.FindMaterialIndex(materials, material);

            string[] indices = polys.Trim(' ').Split(' ');
            string[] vertexCount = new string[0];
            if (vcount != string.Empty)
                vertexCount = vcount.Trim(' ').Split(' ');

            int stride = 0;
            for (int i = 0; i < inputs.Length; i++)
                stride = Math.Max(0, (int)inputs[i].offset + 1);

            //Create a current list of all the vertices
            //Use a list to expand duplicate indices
            List<Vertex> vertices = new List<Vertex>();
            var vertexSource = DaeUtility.FindSourceFromInput(daeMesh.vertices.input[0], daeMesh.source);
            var floatArr = vertexSource.Item as float_array;
            for (int v = 0; v < (int)floatArr.count / 3; v++) {
                vertices.Add(new Vertex(vertices.Count, new List<int>()));
            }

            var indexStride = (indices.Length / 3) / polyCount;
            for (int i = 0; i < polyCount ; i++)
            {
                int count = 3;
                if (vertexCount.Length > i)
                    count = Convert.ToInt32(vertexCount[i]);

                for (int v = 0; v < count; v++)
                {
                    List<int> semanticIndices = new List<int>();
                    for (int j = 0; j < inputs.Length; j++)
                    {
                        int faceOffset = (indexStride * 3) * i;
                        int index = Convert.ToInt32(indices[faceOffset + (v * indexStride) + (int)inputs[j].offset]);
                        semanticIndices.Add(index);
                    }

                    BoneWeight[] boneWeightData = new BoneWeight[0];
                    if (boneWeights?.Count > semanticIndices[0])
                        boneWeightData = boneWeights[semanticIndices[0]];

                    VertexLoader.LoadVertex(ref faces, ref vertices, semanticIndices, boneWeightData);
                }
            }

            int numTexCoordChannels = 0;
            int numColorChannels = 0;

            //Find them in both types of inputs
            for (int i = 0; i < inputs.Length; i++) {
                if (inputs[i].semantic == "TEXCOORD") numTexCoordChannels++;
                if (inputs[i].semantic == "COLOR") numColorChannels++;
            }

            for (int i = 0; i < daeMesh.vertices.input.Length; i++) {
                if (daeMesh.vertices.input[i].semantic == "TEXCOORD") numTexCoordChannels++;
                if (daeMesh.vertices.input[i].semantic == "COLOR") numColorChannels++;
            }


            for (int i = 0; i < vertices.Count; i++)
                if (!vertices[i].IsSet)
                    vertices.Remove(vertices[i]);

            bool hasNormals = false;
            foreach (var daeVertex in vertices)
            {
                if (daeVertex.semanticIndices.Count == 0)
                    continue;

                STVertex vertex = new STVertex();
                vertex.TexCoords = new Vector2[numTexCoordChannels];
                vertex.Colors = new Vector4[numColorChannels];
                foreach (var boneWeight in daeVertex.BoneWeights)
                {
                    vertex.BoneWeights.Add(boneWeight.Weight);
                    vertex.BoneNames.Add(boneWeight.Bone);
                }

                mesh.Vertices.Add(vertex);

                //DAE has 2 inputs. Vertex and triangle inputs
                //Triangle inputs use indices over vertex inputs which only use one
                //Triangle inputs allow multiple color/uv sets unlike vertex inputs
                //Certain programs ie Noesis DAEs use vertex inputs. Most programs use triangle inputs
                for (int j = 0; j < daeMesh.vertices.input.Length; j++)
                {
                    //Vertex inputs only use the first index
                    var vertexInput = daeMesh.vertices.input[j];
                    source source = DaeUtility.FindSourceFromInput(vertexInput, daeMesh.source);
                    if (source == null) continue;
                    if (vertexInput.semantic == "NORMAL") 
                        hasNormals = true;

                    int dataStride = (int)source.technique_common.accessor.stride;
                    int index = daeVertex.semanticIndices[0] * dataStride;

                    ParseVertexSource(ref vertex, scene, source, numTexCoordChannels, numColorChannels,
                        dataStride, index, 0, vertexInput.semantic);
                }

                for (int i = 0; i < inputs.Length; i++)
                {
                    var input = inputs[i];
                    source source = DaeUtility.FindSourceFromInput(input, daeMesh.source);
                    if (source == null) continue;
                    if (input.semantic == "NORMAL")
                        hasNormals = true;

                    int dataStride = (int)source.technique_common.accessor.stride;
                    int index = daeVertex.semanticIndices[i] * dataStride;

                    ParseVertexSource(ref vertex, scene, source, numTexCoordChannels, numColorChannels,
                        dataStride, index, (int)input.set, input.semantic);
                }
            }

            group.Faces = faces;

            if (!hasNormals)
                CalculateNormals(mesh.Vertices, faces);
            if (scene.Settings.RemoveDuplicateVerts) {
                mesh.RemoveDuplicateVertices();
            }
        }

        private static void RemoveDuplicateVerts(List<STVertex> vertices, List<uint> faces)
        {
            List<VertexCheck> vertcies = new List<VertexCheck>();
            for (int v = 0; v < vertices.Count; v++)
            {

            }
        }

        private struct VertexCheck
        {
            public Vector3 Position;
            public Vector3 Normal;
        }

        private static List<uint> TriangulateQuad(List<STVertex> vertices, List<uint> faces)
        {
            var newfaces = new List<uint>();
            int facecount = faces.Count;
            for (int i = 0; i < facecount / 4; i += 4)
            {
                var A = faces[i+0];
                var B = faces[i+1];
                var C = faces[i+2];
                var D = faces[i+3];

                double dist1 = vertices[(int)A].DistanceTo(vertices[(int)C]);
                double dist2 = vertices[(int)B].DistanceTo(vertices[(int)D]);
                if (dist1 > dist2)
                {
                    newfaces.AddRange(new uint[] { A, B, D });
                    newfaces.AddRange(new uint[] { B, C, D });
                }
                else
                {
                    newfaces.AddRange(new uint[] { A, B, C });
                    newfaces.AddRange(new uint[] { A, C, D });
                }
            }

            return newfaces;
        }

        private static void ParseVertexSource(ref STVertex vertex, ColladaScene scene, source source, 
            int numTexCoordChannels, int numColorChannels, int stride, int index, int set, string semantic)
        {
            float_array array = source.Item as float_array;
            if (array.Values == null) return;
            switch (semantic)
            {
                case "VERTEX":
                case "POSITION":
                    vertex.Position = new Vector3(
                        (float)array.Values[index + 0],
                        (float)array.Values[index + 1],
                        (float)array.Values[index + 2]);
                    vertex.Position = ApplyUintScaling(scene, vertex.Position);
                    if (scene.UpAxisType == UpAxisType.Z_UP) {
                        vertex.Position = new Vector3(
                             vertex.Position.X,
                             vertex.Position.Z,
                             -vertex.Position.Y);
                    }
                    break;
                case "NORMAL":
                    vertex.Normal = new Vector3(
                        (float)array.Values[index + 0],
                        (float)array.Values[index + 1],
                        (float)array.Values[index + 2]);
                    if (scene.UpAxisType == UpAxisType.Z_UP) {
                        vertex.Normal = new Vector3(
                             vertex.Normal.X,
                             vertex.Normal.Z,
                             -vertex.Normal.Y);
                    }
                    break;
                case "TEXCOORD":
                    vertex.TexCoords[set] = new Vector2(
                        (float)array.Values[index + 0],
                        (float)array.Values[index + 1]);
                    break;
                case "COLOR":
                    float R = 1, G = 1, B = 1, A = 1;
                    if (stride >= 1) R = (float)array.Values[index + 0];
                    if (stride >= 2) G = (float)array.Values[index + 1];
                    if (stride >= 3) B = (float)array.Values[index + 2];
                    if (stride >= 4) A = (float)array.Values[index + 3];
                    vertex.Colors[set] = new Vector4(R, G, B, A);
                    break;
            }

            //We need to make sure the axis is converted to Z up
            /*  switch (scene.UpAxisType)
              {
                  case UpAxisType.X_UP:
                      break;
                  case UpAxisType.Y_UP:
                      Vector3 pos = vertex.Position;
                      Vector3 nrm = vertex.Normal;
                      //   vertex.Position = new Vector3(pos.X, pos.Z, pos.Y);
                      //    vertex.Normal = new Vector3(nrm.X, nrm.Z, nrm.Y);
                      break;
              }*/
        }

        private static Vector3 ApplyUintScaling(ColladaScene scene, Vector3 value)
        {
            return value;

            if (scene.UintSize == null) return value;

            Vector3 val = value;
            float scale = (float)scene.UintSize.meter;

            //Convert scale to centimeters then multiple the vertex
            switch (scene.UintSize.name)
            {
                case "meter":
                    scale *= 100;
                    break;
            }

            val *= scale;
            return val;
        }

        private static void CalculateNormals(List<STVertex> vertices, List<uint> f)
        {
            if (vertices.Count < 3)
                return;

            Vector3[] normals = new Vector3[vertices.Count];

            for (int i = 0; i < normals.Length; i++)
                normals[i] = new Vector3(0, 0, 0);

            for (int i = 0; i < f.Count; i += 3)
            {
                STVertex v1 = vertices[(int)f[i]];
                STVertex v2 = vertices[(int)f[i + 1]];
                STVertex v3 = vertices[(int)f[i + 2]];
                Vector3 nrm = CalculateNormal(v1, v2, v3);

                normals[f[i + 0]] += nrm * (nrm.Length / 2);
                normals[f[i + 1]] += nrm * (nrm.Length / 2);
                normals[f[i + 2]] += nrm * (nrm.Length / 2);
            }

            for (int i = 0; i < normals.Length; i++)
                vertices[i].Normal = normals[i].Normalized();
        }

        private static Vector3 CalculateNormal(STVertex v1, STVertex v2, STVertex v3)
        {
            Vector3 U = v2.Position - v1.Position;
            Vector3 V = v3.Position - v1.Position;

            // Don't normalize here, so surface area can be calculated. 
            return Vector3.Cross(U, V);
        }

        private static List<BoneWeight[]> ParseWeightController(controller controller, ColladaScene scene)
        {
            if (controller == null) return new List<BoneWeight[]>();

            List<BoneWeight[]> boneWeights = new List<BoneWeight[]>();
            skin skin = controller.Item as skin;
            string[] skinningCounts = skin.vertex_weights.vcount.Trim(' ').Split(' ');
            string[] indices = skin.vertex_weights.v.Trim(' ').Split(' ');

            int maxSkinning = (int)scene.Settings.MaxSkinningCount;
            int stride = skin.vertex_weights.input.Length;

            int indexOffset = 0;
            for (int v = 0; v < skinningCounts.Length; v++)
            {
                int numSkinning = Convert.ToInt32(skinningCounts[v]);

                BoneWeight[] boneWeightsArr = new BoneWeight[Math.Min(maxSkinning, numSkinning)];
                for (int j = 0; j < numSkinning; j++) {
                    if (j < scene.Settings.MaxSkinningCount)
                    {
                        boneWeightsArr[j] = new BoneWeight();
                        foreach (var input in skin.vertex_weights.input)
                        {
                            int offset = (int)input.offset;
                            var source = DaeUtility.FindSourceFromInput(input, skin.source);
                            int index = Convert.ToInt32(indices[indexOffset + offset]);
                            if (input.semantic == "WEIGHT")
                            {
                                var weights = source.Item as float_array;
                                boneWeightsArr[j].Weight = (float)weights.Values[index];
                            }
                            if (input.semantic == "JOINT")
                            {
                                var bones = source.Item as Name_array;
                                boneWeightsArr[j].Bone = bones.Values[index];
                            }
                        }
                    }
                    indexOffset += stride;
                }

                boneWeightsArr = RemoveZeroWeights(boneWeightsArr);
                boneWeights.Add(boneWeightsArr);
            }

            return boneWeights;
        }

        static OpenTK.Matrix4 CreateBindMatrix(string text)
        {
            if (text == null || text == "")
                return OpenTK.Matrix4.Identity;

            var mat = OpenTK.Matrix4.Identity;
            string[] data = text.Trim().Replace("\n", " ").Split(' ');
            if (data.Length != 48)
                return mat;

            mat.M11 = float.Parse(data[0]); mat.M12 = float.Parse(data[1]); mat.M13 = float.Parse(data[2]); mat.M14 = float.Parse(data[3]);
            mat.M21 = float.Parse(data[4]); mat.M22 = float.Parse(data[5]); mat.M23 = float.Parse(data[6]); mat.M24 = float.Parse(data[7]);
            mat.M31 = float.Parse(data[8]); mat.M32 = float.Parse(data[9]); mat.M33 = float.Parse(data[10]); mat.M34 = float.Parse(data[11]);
            mat.M41 = float.Parse(data[12]); mat.M42 = float.Parse(data[13]); mat.M43 = float.Parse(data[14]); mat.M44 = float.Parse(data[15]);
            return mat;
        }

        private static BoneWeight[] RemoveZeroWeights(BoneWeight[] boneWeights)
        {
            float[] weights = new float[8];

            int MaxWeight = 255;
            for (int j = 0; j < 8; j++)
            {
                if (boneWeights.Length > j)
                {
                    int weight = (int)(boneWeights[j].Weight * 255);
                    if (boneWeights.Length == j + 1)
                        weight = MaxWeight;

                    if (weight >= MaxWeight)
                    {
                        weight = MaxWeight;
                        MaxWeight = 0;
                    }
                    else
                        MaxWeight -= weight;

                    weights[j] = weight / 255f;
                }
                else
                {
                    weights[j] = 0;
                    MaxWeight = 0;
                }
            }

            for (int i = 0; i < boneWeights.Length; i++)
                boneWeights[i].Weight = weights[i];

            return boneWeights;
        }

        public class Node
        {
            public Node Parent;

            public string Name { get; set; }
            public Matrix4 Transform { get; set; }

            public string ID { get; set; }
            
            public NodeType Type = NodeType.NODE;

            public List<Node> Children = new List<Node>();

            public Node(Node parent) {
                Parent = parent;
                Transform = Matrix4.Identity;
            }
        }

        public class Vertex
        {
            public BoneWeight[] BoneWeights;

            public List<int> semanticIndices = new List<int>();

            public Vertex DuplicateVertex;
            public bool IsSet => semanticIndices.Count > 0;
            public int Index { get; private set; }

            public Vertex(int index, List<int> indices)
            {
                Index = index;
                semanticIndices = indices;
                BoneWeights = new BoneWeight[0];
            }

            public bool IsMatch(List<int> indices)
            {
                for (int i = 0; i < indices.Count; i++)
                    if (indices[i] != semanticIndices[i])
                        return false;
                return true;
            }
        }
    }
}
