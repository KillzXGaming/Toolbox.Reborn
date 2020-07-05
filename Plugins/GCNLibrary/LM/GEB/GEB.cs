using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.OpenGL;
using Toolbox.Core.Animations;
using Toolbox.Core.ModelView;
using SharpYaml.Serialization;

namespace GCNLibrary.LM
{
    public class GEB : IFileFormat, IConvertableTextFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "Ghost Entity Becon" };
        public string[] Extension { get; set; } = new string[] { "*.geb" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.Extension == ".geb";
        }

        public TextFileType TextFileType => TextFileType.Yaml;
        public bool CanConvertBack => true;

        public string ConvertToString() {
            return ToText(Header);
        }

        public void ConvertFromString(string text) {
            Header = FromText(text);
        }

        public GEB_Parser Header;

        public void Load(Stream stream) {
            Header = new GEB_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public string ToText(GEB_Parser header)
        {
            var serializer = new Serializer();
            return serializer.Serialize(header);
        }

        public GEB_Parser FromText(string text)
        {
            var serializer = new Serializer();
            return serializer.Deserialize<GEB_Parser>(text);
        }
    }
}
