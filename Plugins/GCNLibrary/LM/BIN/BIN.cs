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

namespace GCNLibrary.LM.BIN
{
    public class BIN  : IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "LM Binary Model" };
        public string[] Extension { get; set; } = new string[] { "*.bin" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true))
            {
                reader.SetByteOrder(true);
                return reader.ReadByte() == 2 && fileInfo.Extension == ".bin";
            }
        }

        public ModelRenderer Renderer => new BIN_Render(ToGeneric());
        public BIN_Parser Header;

        public void Load(Stream stream) {
            Header = new BIN_Parser(stream);
        }

        private STGenericModel Model;
        public STGenericModel ToGeneric()
        {
            if (Model != null) return Model;

            var model = new STGenericModel(FileInfo.FileName);

            List<Texture> textures = new List<Texture>();
            Header.GetTextures(textures);
            for (int i = 0; i < textures.Count; i++)
                model.Textures.Add(new BINTexture(textures[i]) {Name = $"Texture{i}" });

            int index = 0;
            foreach (var node in Header.GetAllSceneNodes())
            {
                model.Skeleton.Bones.Add(new STBone(model.Skeleton)
                {
                    Name = $"Node{index++}",
                    Position = node.Translate,
                    EulerRotation = node.Rotate * STMath.Deg2Rad,
                    Scale = node.Scale,
                    ParentIndex = node.ParentIndex,
                });
            }
            model.Skeleton.Reset();
            model.Skeleton.Update();

            int nodeIndex = 0;
            foreach (var node in Header.GetAllSceneNodes())
            {
                for (int i = 0; i < node.DrawnParts.Count; i++)
                {
                    STGenericMesh mesh = new STGenericMesh();
                    mesh.Name = $"Mesh{model.Meshes.Count}";

                    STPolygonGroup group = new STPolygonGroup();
                    group.PrimitiveType = STPrimitiveType.Triangles;
                    mesh.PolygonGroups.Add(group);
                    group.IsTransparentPass = true;

                    BIN_Material mat = new BIN_Material();

                    var element = node.DrawnParts[i];
                    mat.TintColor = element.Material.AmbientColor;
                    foreach (var sampler in element.Material.Samplers)
                    {
                        mat.TextureMaps.Add(new STGenericTextureMap()
                        {
                            WrapU = ConvertWrapModes(sampler.WrapS),
                            WrapV = ConvertWrapModes(sampler.WrapT),
                            Name = $"Texture{sampler.TextureIndex}",
                            Type = mat.TextureMaps.Count == 0 ? STTextureType.Diffuse : STTextureType.None,
                        });
                    }

                    group.Material = mat;

                    foreach (var packet in element.Batch.Packets)
                    {
                        List<STVertex> verts = new List<STVertex>();
                        for (int v = 0; v < packet.Vertices.Length; v++)
                            verts.Add(ToVertex(model.Skeleton.Bones[nodeIndex], packet.Vertices[v]));

                        switch (packet.OpCode)
                        {
                            case 0xA0:
                                ConvertTriFans(verts);
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
                                throw new Exception("Unknown opcode " + packet.OpCode);
                        }
                    }

                    mesh.Optmize(group);
                    model.Meshes.Add(mesh);
                }
                nodeIndex++;
            }

            Model = model;
            return model;
        }

        private STTextureWrapMode ConvertWrapModes(byte value)
        {
            if (value == 0) return STTextureWrapMode.Clamp;
            else if (value == 1) return STTextureWrapMode.Repeat;
            else if (value == 2) return STTextureWrapMode.Mirror;
            return STTextureWrapMode.Repeat;
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

        private static List<STVertex> ConvertTriFans(List<STVertex> vertices)
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

        private STVertex ToVertex(STBone node, ShapeBatch.VertexGroup group)
        {
            Vector3 positon = group.Position;
            Vector3 normal = group.Normal;

            positon = Vector3.TransformPosition(positon, node.Transform);
            // normal = Vector3.TransformNormal(normal, node.Transform);

            return new STVertex()
            {
                Position = positon,
                Normal = normal,
                TexCoords = new Vector2[1] { group.Texcoord },
            };
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public class BIN_Material : STGenericMaterial
        {
            public STColor8 TintColor { get; set; }
        }

        public class BINTexture : STGenericTexture
        {
            public byte[] ImageData;

            public BINTexture(Texture header)
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
