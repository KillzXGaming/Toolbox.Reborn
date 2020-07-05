using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Linq;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a generic mesh used for rendering, exporting and editing geoemetry.
    /// These can optionally be attatched to a <see cref="STGenericModel"/>.
    /// </summary>
    public class STGenericMesh
    {
        /// <summary>
        /// Gets or sets the name of the mesh.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the total amount of faces in all polygon groups
        /// </summary>
        public List<uint> Faces
        {
            get
            {
                List<uint> faces = new List<uint>();
                for (int i = 0; i < PolygonGroups.Count; i++)
                    faces.AddRange(PolygonGroups[i].Faces);
                return faces;
            }
        }

        /// <summary>
        /// A list of polygon groups which store the faces and material indices of a mesh.
        /// </summary>
        public List<STPolygonGroup> PolygonGroups = new List<STPolygonGroup>();

        /// <summary>
        /// A list of <see cref="STVertex"/> which determine the 
        /// points of a mesh is rendered and displayed
        /// </summary>
        public List<STVertex> Vertices = new List<STVertex>();

        /// <summary>
        /// The bone index used for binding a bone to a mesh.
        /// </summary>
        public int BoneIndex { get; set; }

        /// <summary>
        /// The total amount of skinning a single vertex can use.
        /// </summary>
        public uint VertexSkinCount { get; set; }

        /// <summary>
        /// The transformation of the model when moved in the 3D view.
        /// </summary>
        public ModelTransform Transform { get; set; } = new ModelTransform();

        public EventHandler MeshSelected;
        public EventHandler MeshDeselected;
        public EventHandler SelectMesh;

        public virtual void AferSelect() {
            MeshSelected?.Invoke(this, EventArgs.Empty);
        }

        public virtual void AferDeselect() {
            MeshDeselected?.Invoke(this, EventArgs.Empty);
        }

        #region methods

        /// <summary>
        /// Gets a list of materials used by all the polygon groups.
        /// </summary>
        /// <returns></returns>
        public List<STGenericMaterial> GetMaterials()
        {
            List<STGenericMaterial> materials = new List<STGenericMaterial>();
            foreach (var group in PolygonGroups)
                if (group.Material != null)
                    materials.Add(group.Material);
            return materials;
        }

        /// <summary>
        /// Optmizes the indices by regenerating them from the given polygon group.
        /// </summary>
        /// <param name="group"></param>
        public void Optmize(STPolygonGroup group)
        {
            Dictionary<STVertex, int> verticesNew = new Dictionary<STVertex, int>();

            group.Faces.Clear();

            foreach (var v in Vertices)
            {
                if (!verticesNew.ContainsKey(v))
                    verticesNew.Add(v, verticesNew.Count);

                if (verticesNew.ContainsKey(v))
                    group.Faces.Add((uint)verticesNew[v]);
            }

            Vertices.Clear();
            Vertices.AddRange(verticesNew.Keys);
        }

        public void OptimizeVertices(STPolygonGroup group)
        {
            Dictionary<VertexStruct, uint> vertexBank = new Dictionary<VertexStruct, uint>();
            List<STVertex> vertices = new List<STVertex>();
            List<uint> faces = new List<uint>();

            Console.WriteLine($"Vertices {Vertices.Count}");
            foreach (var ind in group.Faces)
            {
                var v = Vertices[(int)ind];

                VertexStruct vert = v.ToStruct();
                if (!vertexBank.ContainsKey(vert))
                {
                    uint index = (uint)vertexBank.Count;
                    faces.Add(index);
                    vertexBank.Add(vert, index);
                    vertices.Add(v);
                }
                else
                {
                    faces.Add(vertexBank[vert]);
                }
            }

            Console.WriteLine($"newvertices {vertices.Count}");

            Vertices = vertices.ToList();
            group.Faces = faces.ToList();
        }

        struct Vertex
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector2 TexCoord { get; set; }
        }

        public void RemoveDuplicateVertices()
        {
            int MAX_BANK = 30000; //  for speeding this up a little with some loss...

            foreach (STPolygonGroup p in PolygonGroups)
            {
                List<STVertex> newVertices = new List<STVertex>();
                List<uint> newFaces = new List<uint>();

                List<STVertex> vbank = new List<STVertex>(); // only check last 50 verts - may miss far apart ones but is faster
                foreach (int f in p.Faces)
                {
                    int newFaceIndex = -1;
                    int i = 0;

                    // Has to loop through all the new vertices each time, which is very slow.
                    foreach (STVertex v in vbank)
                    {
                        if (v.Equals(Vertices[f]))
                        {
                            newFaceIndex = i;
                            break;
                        }
                        else
                            i++;
                    }

                    bool verticesAreEqual = newFaceIndex != -1;
                    if (verticesAreEqual)
                    {
                        newFaces.Add((uint)(newVertices.Count + newFaceIndex));
                    }
                    else
                    {
                        vbank.Add(Vertices[f]);
                        newFaces.Add((uint)(newVertices.Count + vbank.Count - 1));
                    }
                    if (vbank.Count > MAX_BANK)
                    {
                        newVertices.AddRange(vbank);
                        vbank.Clear();
                    }
                }
                newVertices.AddRange(vbank);

                Vertices = newVertices;
                p.Faces = newFaces;
            }
        }

        /// <summary>
        /// Flips the texture coordinates vertically.
        /// </summary>
        public void FlipUvsVertical()
        {
            for (int i = 0; i < Vertices.Count; i++) {
                for (int u = 0; u < Vertices[i].TexCoords.Length; u++) {
                    Vertices[i].TexCoords[u] = new Vector2(
                        Vertices[i].TexCoords[u].X,
                    1 - Vertices[i].TexCoords[u].Y);
                }
            }
        }

        /// <summary>
        /// Flips the texture coordinates horizontally.
        /// </summary>
        public void FlipUvsHorizontal()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                for (int u = 0; u < Vertices[i].TexCoords.Length; u++)
                {
                    Vertices[i].TexCoords[u] = new Vector2(
                       1 - Vertices[i].TexCoords[u].X,
                           Vertices[i].TexCoords[u].Y);
                }
            }
        }

        /// <summary>
        /// Sets the vertex color for all vertices of the current mesh given the color channel.
        /// </summary>
        /// <param name="intColor"></param>
        public void SetVertexColor(Vector4 intColor, int channel = 0)
        {
            foreach (STVertex v in Vertices)
            {
                if (v.Colors.Length > channel)
                    v.Colors[channel] = intColor;
            }
        }

        /// <summary>
        /// Recalculates the normals based on vertex positions.
        /// </summary>
        public void CalculateNormals()
        {
            if (Vertices.Count < 3)
                return;

            Vector3[] normals = new Vector3[Vertices.Count];
            for (int i = 0; i < normals.Length; i++)
                normals[i] = new Vector3(0, 0, 0);

            List<uint> f = Faces;
            for (int i = 0; i < f.Count; i += 3)
            {
                STVertex v1 = Vertices[(int)f[i]];
                STVertex v2 = Vertices[(int)f[i + 1]];
                STVertex v3 = Vertices[(int)f[i + 2]];
                Vector3 nrm = CalculateNormal(v1, v2, v3);

                normals[f[i + 0]] += nrm * (nrm.Length / 2);
                normals[f[i + 1]] += nrm * (nrm.Length / 2);
                normals[f[i + 2]] += nrm * (nrm.Length / 2);
            }

            for (int i = 0; i < normals.Length; i++)
                Vertices[i].Normal = normals[i].Normalized();
        }

        private Vector3 CalculateNormal(STVertex v1, STVertex v2, STVertex v3)
        {
            Vector3 U = v2.Position - v1.Position;
            Vector3 V = v3.Position - v1.Position;

            // Don't normalize here, so surface area can be calculated. 
            return Vector3.Cross(U, V);
        }

        /// <summary>
        /// Calculates the tangents and bitangents 
        /// for <see cref="Vertices"/> from texture coordinates.
        /// </summary>
        /// <param name="uvSet"></param>
        public void CalculateTangentBitangent(int uvSet)
        {
            if (Vertices.Count < 3)
                return;

            if (Vertices.Any(x => x.TexCoords?.Length <= uvSet)) {
                throw new Exception($"Invalid UV set {uvSet} given for calculating tangents.");
            }

            Vector3[] tanArray = new Vector3[Vertices.Count];
            Vector3[] bitanArray = new Vector3[Vertices.Count];

            CalculateTanBitanArrays(Faces, tanArray, bitanArray, uvSet);
            ApplyTanBitanArray(tanArray, bitanArray);
        }

        private void ApplyTanBitanArray(Vector3[] tanArray, Vector3[] bitanArray)
        {
            if (Vertices.Count < 3)
                return;

            for (int i = 0; i < Vertices.Count; i++)
            {
                STVertex v = Vertices[i];
                Vector3 newTan = tanArray[i];
                Vector3 newBitan = bitanArray[i];

                // The tangent and bitangent should be orthogonal to the normal. 
                // Bitangents are not calculated with a cross product to prevent flipped shading  with mirrored normal maps.
                v.Tangent = new Vector4(Vector3.Normalize(newTan - v.Normal * Vector3.Dot(v.Normal, newTan)), 1);
                v.Bitangent = new Vector4(Vector3.Normalize(newBitan - v.Normal * Vector3.Dot(v.Normal, newBitan)), 1);
                v.Bitangent *= -1;
            }
        }

        private void CalculateTanBitanArrays(List<uint> faces, Vector3[] tanArray, Vector3[] bitanArray, int uvSet = 0)
        {
            if (Vertices.Count < 3 || faces.Count <= 0)
                return;

            for (int i = 0; i < faces.Count; i += 3)
            {
                STVertex v1 = Vertices[(int)faces[i]];
                STVertex v2 = Vertices[(int)faces[i + 1]];
                STVertex v3 = Vertices[(int)faces[i + 2]];

                float x1 = v2.Position.X - v1.Position.X;
                float x2 = v3.Position.X - v1.Position.X;
                float y1 = v2.Position.Y - v1.Position.Y;
                float y2 = v3.Position.Y - v1.Position.Y;
                float z1 = v2.Position.Z - v1.Position.Z;
                float z2 = v3.Position.Z - v1.Position.Z;

                float s1, s2, t1, t2;

                s1 = v2.TexCoords[uvSet].X - v1.TexCoords[uvSet].X;
                s2 = v3.TexCoords[uvSet].X - v1.TexCoords[uvSet].X;
                t1 = v2.TexCoords[uvSet].Y - v1.TexCoords[uvSet].Y;
                t2 = v3.TexCoords[uvSet].Y - v1.TexCoords[uvSet].Y;

                float div = (s1 * t2 - s2 * t1);
                float r = 1.0f / div;

                // Fix +/- infinity from division by 0.
                if (r == float.PositiveInfinity || r == float.NegativeInfinity)
                    r = 1.0f;

                float sX = t2 * x1 - t1 * x2;
                float sY = t2 * y1 - t1 * y2;
                float sZ = t2 * z1 - t1 * z2;
                Vector3 s = new Vector3(sX, sY, sZ) * r;

                float tX = s1 * x2 - s2 * x1;
                float tY = s1 * y2 - s2 * y1;
                float tZ = s1 * z2 - s2 * z1;
                Vector3 t = new Vector3(tX, tY, tZ) * r;

                // Prevents black tangents or bitangents due to having vertices with the same UV coordinates. 
                float delta = 0.00075f;
                bool sameU, sameV;

                sameU = (Math.Abs(v1.TexCoords[uvSet].X - v2.TexCoords[uvSet].X) < delta) &&
                        (Math.Abs(v2.TexCoords[uvSet].X - v3.TexCoords[uvSet].X) < delta);

                sameV = (Math.Abs(v1.TexCoords[uvSet].Y - v2.TexCoords[uvSet].Y) < delta) &&
                        (Math.Abs(v2.TexCoords[uvSet].Y - v3.TexCoords[uvSet].Y) < delta);

                if (sameU || sameV)
                {
                    // Let's pick some arbitrary tangent vectors.
                    s = new Vector3(1, 0, 0);
                    t = new Vector3(0, 1, 0);
                }

                // Average tangents and bitangents.
                tanArray[faces[i]] += s;
                tanArray[faces[i + 1]] += s;
                tanArray[faces[i + 2]] += s;

                bitanArray[faces[i]] += t;
                bitanArray[faces[i + 1]] += t;
                bitanArray[faces[i + 2]] += t;
            }
        }

        #endregion
    }
}
