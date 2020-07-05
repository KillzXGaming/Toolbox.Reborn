using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.Animations;
using Toolbox.Core.ModelView;
using OpenTK;

namespace GCNLibrary.LM
{
    public class SLK : IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "Shape Animation" };
        public string[] Extension { get; set; } = new string[] { "*.slk" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".slk" && fileInfo.ParentArchive != null;
        }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric());

        public SLK_Parser Header;
        public SLS_Parser MorphData;

        public void Load(Stream stream) {
            MorphData = TryGetMorphData();
            Header = new SLK_Parser(stream);
        }

        private SLS_Parser TryGetMorphData()
        {
            var morphFile = FileInfo.FilePath.Replace("slk", "sls");
            foreach (var file in FileInfo.ParentArchive.Files) {
                if (file.FileName == morphFile)
                    return new SLS_Parser(file.FileData);
            }

            throw new Exception($"Failed to find morph model target! {FileInfo.FileName.Replace("slk", "sls")}");
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        private STGenericModel Model;

        public STGenericModel ToGeneric()
        {
            if (Model != null) return Model;

            var model = new STGenericModel(FileInfo.FileName);
       /*     var msh = new STGenericMesh() { Name = $"Mesh0" };
            model.Meshes.Add(msh);

            List<STVertex> verts = new List<STVertex>();
            for (int v = 0; v < MorphData.Positions.Count; v++)
            {
                STVertex vertex = new STVertex();
                vertex.Position = MorphData.Positions[v];
                verts.Add(vertex);
            }

            STPolygonGroup group = new STPolygonGroup();
            group.PrimitiveType = STPrimitiveType.Points;
            msh.PolygonGroups.Add(group);

            msh.Vertices.AddRange(verts);
            msh.Optmize(group);
            */
            
            for (int i = 0; i < MorphData.MorphGroups.Count; i++)
            {
                var mesh = new STGenericMesh() { Name = $"Mesh{i}" };
                model.Meshes.Add(mesh);

                var morphData = MorphData.MorphGroups[i];

                STPolygonGroup group = new STPolygonGroup();
                group.PrimitiveType = STPrimitiveType.Triangles;
                mesh.PolygonGroups.Add(group);

                List<STVertex> verts = new List<STVertex>();
                for (int v = 0; v < morphData.Positions.Count; v++)
                {
                    STVertex vertex = new STVertex();
                    vertex.Position = morphData.Positions[v];
                    verts.Add(vertex);
                }

                verts = ConvertTriStrips(verts);
                mesh.Vertices.AddRange(verts);

                mesh.Optmize(group);
            }

            Model = model;
            return model;
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
    }
}
