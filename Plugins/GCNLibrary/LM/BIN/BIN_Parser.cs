using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Toolbox.Core.IO;
using Toolbox.Core;
using OpenTK;

namespace GCNLibrary.LM.BIN
{
    public class BIN_Parser
    {
        //Constant sizes of the section headers
        public const uint hSamplerSize = 0x14;
        public const uint hTextureSize = 0xC;
        public const uint hShapeBatchSize = 0x18;
        public const uint hMaterialSize = 0x28;
        public const uint hSceneGraphSize = 0x8C;

        public byte Version { get; set; } = 0x02;
        public string InternalName { get; set; }

        SortedDictionary<int, SceneGraphNode> SceneGraphs = new SortedDictionary<int, SceneGraphNode>();
        SortedDictionary<int, ShapeBatch> Batches = new SortedDictionary<int, ShapeBatch>();
        SortedDictionary<int, Sampler> Samplers = new SortedDictionary<int, Sampler>();
        SortedDictionary<int, Material> Materials = new SortedDictionary<int, Material>();
        SortedDictionary<int, Texture> Textures = new SortedDictionary<int, Texture>();
        
        public SceneGraphNode SceneRoot { get; set; }

        internal uint TextureOffset;
        internal uint SamplerOffset;

        internal uint PositionOffset;
        internal uint Attribute1Offset;
        internal uint Attribute2Offset;
        internal uint NormalOffset;
        internal uint TexCoordOffset;
        internal uint Attribute3Offset;
        internal uint Attribute4Offset;
        internal uint Attribute5Offset;

        internal uint MaterialOffset;
        internal uint ShapeBatchOffset;
        internal uint SceneGraphOffset;

        public BIN_Parser(Stream stream)
        {
            using (var reader = new FileReader(stream)) {
                Read(reader);
            }
        }

        public void Save(Stream stream) {
            Write(new FileWriter(stream));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            Version = reader.ReadByte();
            InternalName = reader.ReadString(0x0B);
            TextureOffset = reader.ReadUInt32();
            SamplerOffset = reader.ReadUInt32();
            PositionOffset = reader.ReadUInt32();
            NormalOffset = reader.ReadUInt32();
            Attribute1Offset = reader.ReadUInt32();
            Attribute2Offset = reader.ReadUInt32();
            TexCoordOffset = reader.ReadUInt32();
            Attribute3Offset = reader.ReadUInt32();
            Attribute4Offset = reader.ReadUInt32();
            Attribute5Offset = reader.ReadUInt32();

            MaterialOffset = reader.ReadUInt32();
            ShapeBatchOffset = reader.ReadUInt32();
            SceneGraphOffset = reader.ReadUInt32();

            SceneRoot = ReadSection<SceneGraphNode>(reader, 0);
            OrderLists();
        }

        private void Write(FileWriter writer)
        {
            OrderLists();

            writer.SetByteOrder(true);
            writer.Write(Version);
            writer.WriteString(InternalName, 0xB);
            writer.Write(new uint[21]); //reserve space for offsets

            List<Texture> textures = Textures.Values.ToList();
            List<Sampler> samplers = Samplers.Values.ToList();
            List<Material> materials = Materials.Values.ToList();
            List<SceneGraphNode> nodes = SceneGraphs.Values.ToList();
            List<ShapeBatch> batches = Batches.Values.ToList();

            //Save texture header
            writer.WriteUint32Offset(12);
            long texturePos = writer.Position;
            for (int i = 0; i < textures.Count; i++)
                textures[i].Write(writer, this);
            writer.Align(32);

            //Save texture data
            for (int i = 0; i < textures.Count; i++) {
                writer.WriteUint32Offset(texturePos + 8 + (i * hTextureSize), texturePos);
                writer.Write(textures[i].ImageData);
                writer.Align(32);
            }

            //Save samplers
            writer.WriteUint32Offset(16);
            for (int i = 0; i < samplers.Count; i++)
            {
                samplers[i].TextureIndex = (short)textures.IndexOf(samplers[i].Texture);
                samplers[i].Write(writer, this);
            }

            //Save vertices
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> colors = new List<Vector4>();
            List<Vector2> texcoords = new List<Vector2>();
            for (int i = 0; i < batches.Count; i++)
            {
                foreach (var packet in batches[i].Packets)
                {
                    for (int v = 0; v < packet.Vertices.Length; v++)
                    {
                        var vert = packet.Vertices[v];
                        Vector3 shortPos = new Vector3(
                            (short)vert.Position.X,
                            (short)vert.Position.Y,
                            (short)vert.Position.Z);

                        if (!positions.Contains(shortPos))
                            positions.Add(shortPos);
                        if (!normals.Contains(vert.Normal) && vert.Normal != null)
                            normals.Add(vert.Normal);
                        if (!texcoords.Contains(vert.Texcoord) && vert.Texcoord != null)
                            texcoords.Add(vert.Texcoord);
                        if (!colors.Contains(vert.Color0) && vert.Color0 != null)
                            colors.Add(vert.Color0);

                        vert.PositionIndex = (short)positions.IndexOf(vert.Position);
                        vert.NormalIndex = (short)normals.IndexOf(vert.Normal);
                        vert.TexCoordIndex[0] = (short)texcoords.IndexOf(vert.Texcoord);
                        vert.Color0Index = (short)colors.IndexOf(vert.Color0);
                    }
                }
            }

            writer.WriteUint32Offset(20);
            for (int i = 0; i < positions.Count; i++) {
                writer.Write((short)positions[i].X);
                writer.Write((short)positions[i].Y);
                writer.Write((short)positions[i].Z);
            }
            writer.Align(32);

            if (normals.Count > 0)
            {
                writer.WriteUint32Offset(24);
                for (int i = 0; i < normals.Count; i++)
                {
                    writer.Write(normals[i].X);
                    writer.Write(normals[i].Y);
                    writer.Write(normals[i].Z);
                }
                writer.Align(32);
            }

            if (colors.Count > 0)
            {
                writer.WriteUint32Offset(28);
                for (int i = 0; i < colors.Count; i++)
                {
                    writer.Write((byte)(colors[i].X * 255));
                    writer.Write((byte)(colors[i].Y * 255));
                    writer.Write((byte)(colors[i].Z * 255));
                    writer.Write((byte)(colors[i].W * 255));
                }
                writer.Align(32);
            }

            if (texcoords.Count > 0)
            {
                writer.WriteUint32Offset(36);
                for (int i = 0; i < texcoords.Count; i++)
                {
                    writer.Write(texcoords[i].X);
                    writer.Write(texcoords[i].Y);
                }
                writer.Align(32);
            }

            writer.WriteUint32Offset(52);
            for (int i = 0; i < materials.Count; i++)
            {
                for (int j = 0; j < materials[i].Samplers.Count; j++)
                    materials[i].SamplerIndices[j] = (short)samplers.IndexOf(materials[i].Samplers[j]);

                materials[i].Write(writer, this);
            }

            long batchPos = writer.Position;
            writer.WriteUint32Offset(56);
            for (int i = 0; i < Batches.Count; i++)
                Batches[i].Write(writer, this);

            for (int i = 0; i < Batches.Count; i++) {
                writer.WriteUint32Offset(batchPos + 12 + (i * hShapeBatchSize), batchPos);

                long pos = writer.Position;
                for (int d = 0; d < Batches[i].Packets.Count; d++)
                    Batches[i].Packets[d].Write(Batches[i], writer, this);

                writer.Align(32);

                long endpos = writer.Position;
                using (writer.TemporarySeek(batchPos + 2 + (i * hShapeBatchSize), SeekOrigin.Begin)) {
                    writer.Write((ushort)((endpos - pos) / 0x20));
                }
            }

            long sceneGraphPos = writer.Position;
            writer.WriteUint32Offset(60);
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].Write(writer, this);
            }
            for (int i = 0; i < nodes.Count; i++) {

                writer.WriteUint32Offset(sceneGraphPos + 80 + (hSceneGraphSize * i), sceneGraphPos);
                for (int d = 0; d < nodes[i].DrawnParts.Count; d++) {
                    var part = nodes[i].DrawnParts[d];
                    part.MaterialIndex = (short)materials.IndexOf(part.Material);
                    part.ShapeBatchIndex = (short)batches.IndexOf(part.Batch);
                    part.Write(writer, this);
                }
            }
            writer.AlignBytes(16);
        }

        internal void OrderLists()
        {
            SceneGraphs.OrderBy(x => x.Key);
            Batches.OrderBy(x => x.Key);
            Samplers.OrderBy(x => x.Key);
            Materials.OrderBy(x => x.Key);
            Textures.OrderBy(x => x.Key);
        }

        public List<SceneGraphNode> GetAllSceneNodes() {
            return SceneGraphs.Values.ToList();
        }

        public List<DrawElement> GetDrawElements() {
            List<DrawElement> elements = new List<DrawElement>();
            GetDrawElements(elements, SceneRoot);
            return elements;
        }

        private void GetDrawElements(List<DrawElement> elements, SceneGraphNode parent) {
            if (parent.DrawnParts.Count > 0)
                elements.AddRange(parent.DrawnParts);

            foreach (var node in parent.Children)
                GetDrawElements(elements, node);
        }

        public void GetTextures(List<Texture> textures)
        {
            foreach (var tex in Textures.OrderBy(x => x.Key))
                textures.Add(tex.Value);
        }

        public T ReadSection<T>(FileReader reader, int index)
        {
            SeekSection<T>(reader, index);
            object section = null;

            if (typeof(T) == typeof(SceneGraphNode))
            {
                if (!SceneGraphs.ContainsKey(index))
                    SceneGraphs.Add(index, new SceneGraphNode(reader, index, this));
                section = SceneGraphs[index];
            }
            else if (typeof(T) == typeof(Sampler))
            {
                if (!Samplers.ContainsKey(index))
                    Samplers.Add(index, new Sampler(reader, this));
                section = Samplers[index];
            }
            else if (typeof(T) == typeof(Material))
            {
                if (!Materials.ContainsKey(index))
                    Materials.Add(index, new Material(reader, this));
                section = Materials[index];
            }
            else if (typeof(T) == typeof(ShapeBatch))
            {
                if (!Batches.ContainsKey(index))
                    Batches.Add(index, new ShapeBatch(reader, this));
                section = Batches[index];
            }
            else if (typeof(T) == typeof(Texture))
            {
                if (!Textures.ContainsKey(index))
                    Textures.Add(index, new Texture(reader, this));
                section = Textures[index];
            }

            return (T)section;
        }
        
        /// <summary>
        /// Seeks to the section by the given index and type provided. 
        /// The section must be referenced in the file header to be used and not a vertex attribute.
        /// </summary>
        public void SeekSection<T>(FileReader reader, int index) {
            reader.SeekBegin(GetOffset<T>((uint)index));
        }

        /// <summary>
        /// Seeks to the section by the given index and type provided. 
        /// The section must be referenced in the file header to be used and not a vertex attribute.
        /// </summary>
        public void SeekSection<T>(FileWriter writer, int index) {
            writer.SeekBegin(GetOffset<T>((uint)index));
        }

        private uint GetOffset<T>(uint index)
        {
            if (typeof(T) == typeof(SceneGraphNode))
                return SceneGraphOffset + (index * BIN_Parser.hSceneGraphSize);
            else if (typeof(T) == typeof(Texture))
                return TextureOffset + (index * BIN_Parser.hTextureSize);
            else if (typeof(T) == typeof(Sampler))
                return SamplerOffset + (index * BIN_Parser.hSamplerSize);
            else if (typeof(T) == typeof(Material))
                return MaterialOffset + (index * BIN_Parser.hMaterialSize);
            else if (typeof(T) == typeof(ShapeBatch))
                return ShapeBatchOffset + (index * BIN_Parser.hShapeBatchSize);
            return 0;
        }
    }
}
