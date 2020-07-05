using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Core.ModelView;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class FileWrapper : ObjectWrapper
    {
        public IFileFormat FileFormat => (IFileFormat)ActiveObject;

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> items = new List<ToolMenuItem>();
            items.Add(new ToolMenuItem("Save", SaveFile) { Enabled = FileFormat.CanSave, });
            return items.ToArray();
        }

        private void SaveFile(object sender, EventArgs e)
        {
            var selected = GetSelectedObjects<IFileFormat>();
            if (selected.Count == 0) return;

            SaveDialogCustom sfd = new SaveDialogCustom();
            string name = selected[0].FileInfo.FileName;
            sfd.AddFilter(selected[0]);
            sfd.FolderDialog = selected.Count > 1;
            sfd.FileName = name;
            sfd.DefaultExt = System.IO.Path.GetExtension(name);
            var result = sfd.ShowDialog();
            if (result == SaveDialogCustom.Result.OK) {
                if (selected.Count == 1)
                    STFileSaver.SaveFileFormat(selected[0], sfd.FileName);
                else
                {
                    foreach (var file in selected)
                        STFileSaver.SaveFileFormat(selected[0], $"{sfd.FolderPath}/{file.FileInfo.FileName}");
                }
            }
        }
    }
}
