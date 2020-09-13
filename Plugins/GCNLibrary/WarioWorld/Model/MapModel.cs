using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core.GX;
using Toolbox.Core.OpenGL;
using Toolbox.Core;
using System.Linq;

namespace GCNLibrary.WW
{
    public class MapModel : IModelFormat
    {
        public string FileName { get; set; }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric()) { DisplayVertexColorAlpha = false };
        public TPL TextureContainer { get; set; }
        public List<Mesh> Meshes = new List<Mesh>();

        public uint ResourceType;

        private STGenericModel CachedModel;
        public STGenericModel ToGeneric()
        {
            if (CachedModel != null) return CachedModel;

            STGenericModel model = new STGenericModel(FileName);
            foreach (var shape in Meshes)
            {
                foreach (var meshGroup in shape.Groups)
                {
                    var genericMesh = new STGenericMesh();
                    genericMesh.Name = $"Mesh{model.Meshes.Count}";
                    genericMesh.Vertices.AddRange(meshGroup.Vertices);

                    var group = new STPolygonGroup();
                    genericMesh.PolygonGroups.Add(group);

                    genericMesh.Optmize(group);
                    model.Meshes.Add(genericMesh);

                    var mat = new STGenericMaterial();
                    group.Material = mat;
               //     group.IsTransparentPass = true;
                    if (TextureContainer.Textures.Count > 0)
                    {
                        var texMap = new STGenericTextureMap()
                        {
                            Name = $"Texture{meshGroup.TextureIndex}",
                            Type = STTextureType.Diffuse,
                        };
                        mat.TextureMaps.Add(texMap);
                    }
                }
            }
            foreach (var tex in TextureContainer.Textures)
                model.Textures.Add(tex);

            CachedModel = model;
            return model;
        }

        public MapModel(Stream stream, string fileName, uint resourceType) {
            FileName = fileName;
            Read(new FileReader(stream), resourceType);
        }

        private void Read(FileReader reader, uint resourceType)
        {
            reader.SetByteOrder(true);
            ResourceType = resourceType;

            uint positionsOffset = reader.ReadUInt32() * 4;
            uint normalsOffset = reader.ReadUInt32() * 4;
            uint colorsOffset = reader.ReadUInt32() * 4;
            uint texCoordsOffset = reader.ReadUInt32() * 4;
            uint textureContainerOffset = reader.ReadUInt32() * 4;
            uint unkOffset = reader.ReadUInt32() * 4; //Material animations???
            uint unk2Offset = reader.ReadUInt32() * 4; //Material animation data???
            uint shapeCount = reader.ReadUInt32();
            uint shapesOffset = reader.ReadUInt32() * 4;
            uint packetCount = reader.ReadUInt32();
            uint packetsOffset = reader.ReadUInt32() * 4;

            TextureContainer = new TPL();
            TextureContainer.FileInfo = new File_Info();
            TextureContainer.Load(new SubStream(
                reader.BaseStream, textureContainerOffset));

            Dictionary<int, MeshGroup> meshGroups = new Dictionary<int, MeshGroup>();

            reader.SeekBegin(shapesOffset);
            for (int i = 0; i < shapeCount; i++)
            {
                uint unk = reader.ReadUInt16();
                uint numPackets = reader.ReadUInt16();
                uint packetOffset = reader.ReadUInt32() * 4;

                using (reader.TemporarySeek(packetOffset, SeekOrigin.Begin))
                {
                    for (int j = 0; j < numPackets; j++)
                    {
                        reader.SeekBegin(packetOffset + (j * 12));
                        Packet packet = new Packet()
                        {
                            Offset = reader.ReadUInt32() * 4,
                            Size = reader.ReadUInt32(),
                            Flags1 = reader.ReadByte(),
                            Flags2 = reader.ReadByte(),
                            MaterialColorIndex = reader.ReadSByte(),
                            TextureIndex = reader.ReadSByte(),
                        };

                        if (!meshGroups.ContainsKey(packet.TextureIndex))
                        {
                            meshGroups.Add(packet.TextureIndex, new MeshGroup() {
                                TextureIndex = packet.TextureIndex,
                            });
                        }

                        var meshGroup = meshGroups[packet.TextureIndex];

                        List<GXVertexLayout> layouts = new List<GXVertexLayout>();
                        layouts.Add(new GXVertexLayout(
                            GXAttributes.Position,
                            GXComponentType.F32, GXAttributeType.INDEX16, positionsOffset));
                        if (normalsOffset != 0)
                        {
                            layouts.Add(new GXVertexLayout(
                                GXAttributes.Normal,
                                GXComponentType.S16, GXAttributeType.INDEX16, normalsOffset));
                        }
                        if (colorsOffset != 0)
                        {
                            layouts.Add(new GXVertexLayout(
                                 GXAttributes.Color0,
                                 GXComponentType.RGBA4, GXAttributeType.INDEX16, colorsOffset));
                        }
                        if (texCoordsOffset != 0)
                        {
                            layouts.Add(new GXVertexLayout(
                                GXAttributes.TexCoord0,
                                GXComponentType.S16, GXAttributeType.INDEX16, texCoordsOffset) {
                                Divisor = 1024.0f,
                            });
                        }

                        var displayListData = new SubStream(reader.BaseStream, packet.Offset, packet.Size);
                        meshGroup.Vertices.AddRange(DisplayListHelper.ReadDisplayLists(
                            displayListData, reader.BaseStream, layouts.ToArray(), new DisplayListHelper.Config()
                            {
                                OpCodeShift = -1, //Op codes seem to be +1 higher than they should be. Unsure why.
                            }));
                    }
                }
            }

            //Maps have 100s of shapes. To make rendering more efficent, combine meshes by materials using the mesh groups.
            foreach (var group in meshGroups.Values) {
                Meshes.Add(new Mesh() {
                    Groups = new List<MeshGroup>() { group }
                });
            }
            meshGroups.Clear();
        }
    }
}
