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

namespace GCNLibrary.Pikmin1
{
    public class MOD  : ObjectTreeNode, IFileFormat, IModelFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "Pikmin 1 Model" };
        public string[] Extension { get; set; } = new string[] { "*.mod" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".mod";
        }

        public ModelRenderer Renderer => new ModelRenderer(ToGeneric());
        public MOD_Parser Header;

        public void Load(Stream stream)
        {
            this.Label = FileInfo.FileName;
            Tag = this;
            Header = new MOD_Parser(stream);
        }

        public void Save(Stream stream)
        {

        }

        public STGenericModel ToGeneric()
        {
            var model = new STGenericModel(FileInfo.FileName);
            return model;
        }
    }
}
