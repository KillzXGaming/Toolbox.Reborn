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
    public class TMB : IFileFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "Timing Bank" };
        public string[] Extension { get; set; } = new string[] { "*.tmb" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".tmb";
        }

        public TMB_Parser Header;

        public void Load(Stream stream) {
            Header = new TMB_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }
    }
}
