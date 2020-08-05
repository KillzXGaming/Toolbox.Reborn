using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core.GX;
using Toolbox.Core.OpenGL;
using Toolbox.Core;
using Collada141;
using System.Linq;
using OpenTK;

namespace GCNLibrary.WW
{
    public class Model
    {
        public string FileName { get; set; }

        public uint ResourceType;

        public SkeletalAnim SkeletalAnim { get; set; }

        public TPL TextureContainer { get; set; }
        public List<ModelGroup> Models = new List<ModelGroup>();

        /// <summary>
        /// Updates the skeleton from a baked animation if one is already loaded.
        /// </summary>
        public void UpdateSkeleton() {
            if (SkeletalAnim == null) return;

            for (int i = 0; i < Models.Count; i++)
                Models[i].UpdateSkeleton(SkeletalAnim, i);
        }

        private bool updated = false;

        /// <summary>
        /// Updates the mesh indices from a baked skeleton if one is already loaded
        /// </summary>
        public void UpdateBoneIndices()
        {
            if (SkeletalAnim == null || updated) return;
            updated = true;

            int[] indexTable = new int[10];
            for (int i = 0; i < 10; i++)
                indexTable[i] = -1;

            for (int m = 0; m < Models.Count; m++)
            {
                var meshIndices = SkeletalAnim.GetMeshIndexGroups(m);
                for (int i = 0; i < meshIndices.Count; i++)
                {
                    if (Models[m].Meshes.Count <= i)
                        break;

                    foreach (var indexGrp in meshIndices[i]){
                        foreach (var boneIndices in indexGrp.BoneIndices) {
                            int index = boneIndices.RiggingIndex / 3;
                            indexTable[index] = indexGrp.Index - boneIndices.BoneIndexShift;
                        }
                    }
                    foreach (var meshGroup in Models[m].Meshes[i].Groups)
                        ApplyBoneTransforms(Models[m], meshGroup, indexTable);
                }
            }
        }

        private void ApplyBoneTransforms(ModelGroup model, MeshGroup mesh, int[] indexTable)
        {
            var skeleton = model.ToGeneric().Skeleton;

            List<STVertex> appliedVertices = new List<STVertex>();

            for (int v = 0; v < mesh.Vertices.Count; v++) {
                if (mesh.Vertices[v].BoneIndices.Count > 0 && !appliedVertices.Contains(mesh.Vertices[v]))
                {
                    int boneIndex = mesh.Vertices[v].BoneIndices[0];
                    if (indexTable[boneIndex] != -1) {
                        var realIndex = indexTable[boneIndex];

                        var transform = skeleton.Bones[realIndex].Transform;
                        mesh.Vertices[v].Position = Vector3.TransformPosition(mesh.Vertices[v].Position, transform);

                        mesh.Vertices[v].BoneIndices[0] = realIndex; 
                    }

                    appliedVertices.Add(mesh.Vertices[v]);
                }
            }
            appliedVertices.Clear();
        }

        public Model(Stream stream, string fileName, uint resourceType) {
            FileName = fileName;
            Read(new FileReader(stream), resourceType);
        }

        private uint positionsOffset;
        private uint normalsoffset;
        private uint colorsOffset;
        private uint texCoordsOffset;
        private uint materialColorsOffset;

        private void Read(FileReader reader, uint resourceType)
        {
            reader.SetByteOrder(true);

            ResourceType = resourceType;

            //Offsets are multiplied by 4
            positionsOffset = reader.ReadUInt32() * 4;
            normalsoffset = reader.ReadUInt32() * 4;
            colorsOffset = reader.ReadUInt32() * 4;
            texCoordsOffset = reader.ReadUInt32() * 4; 
            materialColorsOffset = reader.ReadUInt32() * 4;
            uint textureContainerOffset = reader.ReadUInt32() * 4;
            uint drawElementCount = reader.ReadUInt32();
            uint drawElementsOffset = reader.ReadUInt32() * 4;
            uint shapeCount = reader.ReadUInt32();
            uint shapesOffset = reader.ReadUInt32() * 4;
            uint packetCount = reader.ReadUInt32();
            uint packetsOffset = reader.ReadUInt32() * 4;

            TextureContainer = new TPL();
            TextureContainer.FileInfo = new File_Info();
            if (textureContainerOffset != 0)
            {
                TextureContainer.Load(new SubStream(
                    reader.BaseStream, textureContainerOffset));
            }

            List<uint> readShapes = new List<uint>();
            for (int i = 0; i < drawElementCount; i++)
            {
                reader.SeekBegin(drawElementsOffset + (i * 8));
                uint numShapes = reader.ReadUInt32();
                uint shapeOffset = reader.ReadUInt32() * 4;

                //Model lists often have dupes so skip those
                //Usually they are fixed to have 5 of them.
                //Then they get lower in detail.
                if (readShapes.Contains(shapeOffset))
                    break;

                ModelGroup model = new ModelGroup();
                Models.Add(model);

                readShapes.Add(shapeOffset);

                for (int j = 0; j < numShapes; j++) {
                    reader.SeekBegin(shapeOffset + (j * 8));
                    model.Meshes.Add(ReadShape(reader));
                }
            }
        }

        private Mesh ReadShape(FileReader reader)
        {
            Mesh mesh = new Mesh();

            ushort unk = reader.ReadUInt16();
            ushort numPackets = reader.ReadUInt16();
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

                    MeshGroup meshGroup = new MeshGroup()
                    {
                        Packet = packet,
                        TextureIndex = packet.TextureIndex,
                    };
                    mesh.Groups.Add(meshGroup);



                  //  Console.WriteLine($"Material {packet.MaterialColorIndex} TextureIndex {packet.TextureIndex}");
                  //  Console.WriteLine($"Flags1 {packet.Flags1 >> 4} {packet.Flags1 & 0xF} Flags2 {packet.Flags2}");

                    if (packet.MaterialColorIndex != -1) {
                        using (reader.TemporarySeek(materialColorsOffset + (packet.MaterialColorIndex * 4), SeekOrigin.Begin)) {
                            meshGroup.Color = reader.ReadColor8RGBA();
                        }
                    }

                    DisplayListHelper.Config config = new DisplayListHelper.Config();

                    List<GXVertexLayout> layouts = new List<GXVertexLayout>();
                    if (packet.Flags2 != 3 && packet.Flags2 != 15 && packet.Flags2 != 16
                        && packet.Flags2 != 14 && packet.Flags2 != 0)
                    {
                        layouts.Add(new GXVertexLayout(
                            GXAttributes.PosNormMatrix,
                            GXComponentType.U8, GXAttributeType.INDEX8, 48));
                    }
                    if (packet.Flags2 == 18)
                    {
                        layouts.Add(new GXVertexLayout(
                           GXAttributes.Tex0Matrix,
                           GXComponentType.U8, GXAttributeType.INDEX8, 48));
                    }
                    if (packet.Flags2 == 19)
                    {
                        layouts.Add(new GXVertexLayout(
                           GXAttributes.Tex0Matrix,
                           GXComponentType.U8, GXAttributeType.INDEX8, 48));
                        layouts.Add(new GXVertexLayout(
                         GXAttributes.Tex1Matrix,
                         GXComponentType.U8, GXAttributeType.INDEX8, 48));
                        layouts.Add(new GXVertexLayout(
                         GXAttributes.Tex2Matrix,
                         GXComponentType.U8, GXAttributeType.INDEX8, 48));
                    }

                    layouts.Add(new GXVertexLayout(
                            GXAttributes.Position,
                            GXComponentType.S16, GXAttributeType.INDEX16, positionsOffset));

                    if (normalsoffset != 0)
                    {
                        layouts.Add(new GXVertexLayout(
                                      GXAttributes.Normal,
                                      GXComponentType.S16, GXAttributeType.INDEX16, normalsoffset));
                    }

                    if (packet.Flags2 >= 16)
                    {
                        layouts.Add(new GXVertexLayout(
                                     GXAttributes.NormalBinormalTangent,
                                     GXComponentType.S16, GXAttributeType.INDEX16, normalsoffset));
                        layouts.Add(new GXVertexLayout(
                                     GXAttributes.NormalBinormalTangent,
                                     GXComponentType.S16, GXAttributeType.INDEX16, normalsoffset));
                    }
                    if (packet.Flags2 == 14 || packet.Flags2 == 15)
                    {
                        layouts.Add(new GXVertexLayout(
                                     GXAttributes.NormalBinormalTangent,
                                     GXComponentType.S16, GXAttributeType.INDEX16, normalsoffset));
                    }

                    if (colorsOffset != 0 && packet.Flags2 != 0)
                    {
                        layouts.Add(new GXVertexLayout(
                                          GXAttributes.Color0,
                                          GXComponentType.RGBA4, GXAttributeType.INDEX16, colorsOffset));
                    }
                    if (texCoordsOffset != 0 && packet.Flags2 != 0)
                    {
                        layouts.Add(new GXVertexLayout(
                                GXAttributes.TexCoord0,
                                GXComponentType.S16, GXAttributeType.INDEX16, texCoordsOffset)
                        {
                            Divisor = 1024.0f,
                        });
                    }

                    if (packet.Flags2 == 16 || packet.Flags2 == 17 || packet.Flags2 == 19) {
                        config.OpCodeShift = -2;
                    }

                    var displayListData = new SubStream(reader.BaseStream, packet.Offset, packet.Size);
                    meshGroup.Vertices.AddRange(DisplayListHelper.ReadDisplayLists(
                        displayListData, reader.BaseStream, layouts.ToArray(), config));
                }
            }
            return mesh;
        }
    }
}
