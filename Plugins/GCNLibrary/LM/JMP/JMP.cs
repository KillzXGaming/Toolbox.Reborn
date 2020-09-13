using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core;
using System.Linq;

namespace GCNLibrary.LM
{
    public class JMP : IFileFormat, IConvertableTextFormat
    {
        public bool CanSave { get; set; } = true;

        public string[] Description { get; set; } = new string[] { "jmp" };
        public string[] Extension { get; set; } = new string[] { "*.jmp" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream) {
            return fileInfo.FilePath.Contains("jmp") || fileInfo.FilePath.Contains("path") || fileInfo.FilePath.Contains(".pa");
        }

        public TextFileType TextFileType => TextFileType.Yaml;
        public bool CanConvertBack => false;

        public string ConvertToString() {
            return ToText(Header);
        }

        public void ConvertFromString(string text)
        {
        }

        public JMP_Parser Header;

        public void Load(Stream stream) {
            Header = new JMP_Parser(stream);
        }

        public void Save(Stream stream) {
            Header.Save(stream);
        }

        public string ToText(JMP_Parser header)
        {
            StringBuilder sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {

                foreach (var record in header.Records) {
                    writer.WriteLine($"- Record:");
                    for (int i = 0; i < record.Values.Length; i++)
                        writer.WriteLine($"  - {header.Fields[i].Name}: {record.Values[i]}");
                }
            }
            Console.WriteLine($"CSV {sb.ToString()}");
            return sb.ToString();
        }
    }
}
