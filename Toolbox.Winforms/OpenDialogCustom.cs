using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    public class OpenDialogCustom
    {
        public bool FolderDialog = false;
        public string FolderPath = "";

        public string Filter = "";
        public string FileName = "";
        public string DefaultExt = "";

        public string[] FileNames;

        public bool MultiSelect = false;

        private Dictionary<string, string> Extensions = new Dictionary<string, string>();

        public string[] GetFiles()
        {
            List<string> files = new List<string>();
            if (FolderDialog)
            {
                foreach (var file in Directory.GetFiles(FolderPath))
                    files.Add(file);
            }
            else
                files.AddRange(FileNames);
            return files.ToArray();
        }

        public string GetFilter()
        {
            if (Extensions.Count == 0)
                return "All Files (*.*)|*.*";

            string Filter = "All Supported Files|";
            List<string> FilterEach = new List<string>();
            foreach (var Extension in Extensions)
            {
                Filter += $"*{Extension.Key};";
                FilterEach.Add($"{Extension.Value} (*{Extension.Key}) |*{Extension.Key}|");
            }

            Filter += "|";
            Filter += string.Join("", FilterEach.ToArray());
            Filter += "All files(*.*)|*.*";
            return Filter;
        }

        public void AddFilter(IFileFormat fileFormat)
        {
            for (int i = 0; i < fileFormat.Extension.Length; i++)
            {
                string ext = fileFormat.Extension[i].Remove(0, 1);
                if (fileFormat.Description.Length > i)
                    AddFilter(ext, fileFormat.Description[i]);
                else if (fileFormat.Description.Length > 0)
                    AddFilter(ext, fileFormat.Description[0]);
                else
                    AddFilter(ext);
            }
        }

        public void AddFilter(string ext)
        {
            AddFilter(ext, ext);
        }

        public void AddFilter(string ext, string desc)
        {
            if (!Extensions.ContainsKey(ext))
                Extensions.Add(ext, desc);
        }

        public Result ShowDialog()
        {
            if (FolderDialog)
            {
                FolderSelectDialog dlg = new FolderSelectDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FolderPath = dlg.SelectedPath;
                    return Result.OK;
                }
                else
                    return Result.Cancel;
            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = MultiSelect;
                ofd.Filter = Filter == "" ? GetFilter() : Filter;
                ofd.FileName = FileName;
                ofd.DefaultExt = DefaultExt;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                    FileNames = ofd.FileNames;
                    return Result.OK;
                }
                else
                    return Result.Cancel;
            }
        }

        public enum Result
        {
            Cancel,
            OK,
        }
    }
}
