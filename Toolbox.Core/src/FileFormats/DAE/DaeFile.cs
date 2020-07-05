using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.ModelView;
using Toolbox.Core.Collada;

namespace Factor5Library
{
    public class DaeFile : ObjectTreeNode, IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "DAE" };
        public string[] Extension { get; set; } = new string[] { "*.dae" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".dae";
        }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric());

        public STGenericScene Header;

        public void Load(Stream stream)
        {
            this.Label = FileInfo.FileName;
            Tag = this;
            Header = DAE.Read(stream, FileInfo.FilePath);

            var model = ToGeneric();
            Header.Models[0].Skeleton.Reset();
            foreach (var child in model.CreateTreeHiearchy().Children)
                AddChild(child);
        }

        public STGenericModel ToGeneric()
        {
            Header.Models[0].Name = FileInfo.FileName;
            return Header.Models[0];
        }

        public void Save(Stream stream)
        {
        }
    }
}
