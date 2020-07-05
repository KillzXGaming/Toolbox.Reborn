using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.Collada
{
    public class VertexLoader
    {
        public static void LoadVertex(ref List<uint> faces, ref List<ColladaReader.Vertex> vertices,
            List<int> semanticIndices, BoneWeight[] boneWeights)
        {
            int positionIndex = semanticIndices[0];
            var current = vertices[positionIndex];
            if (!current.IsSet)
            {
                current.BoneWeights = boneWeights;
                current.semanticIndices = semanticIndices;
                faces.Add((uint)positionIndex);
            }
            else
                LoadDupedPositionVert(ref faces, ref vertices, current, semanticIndices, boneWeights);
        }

        private static void LoadDupedPositionVert(ref List<uint> faces, ref List<ColladaReader.Vertex> vertices,
         ColladaReader.Vertex vert, List<int> semanticIndices, BoneWeight[] boneWeights)
        {
            if (vert.IsMatch(semanticIndices))
            {
                faces.Add((uint)vert.Index);
                return;
            }

            if (vert.DuplicateVertex != null)
            {
                LoadDupedPositionVert(ref faces, ref vertices, vert.DuplicateVertex, semanticIndices, boneWeights);
                return;
            }

            var duplicateVertex = new ColladaReader.Vertex(vertices.Count, semanticIndices);

            duplicateVertex.BoneWeights = boneWeights;
            duplicateVertex.semanticIndices = semanticIndices;
            vert.DuplicateVertex = duplicateVertex;

            vertices.Add(duplicateVertex);
            faces.Add((uint)duplicateVertex.Index);
        }
    }
}
