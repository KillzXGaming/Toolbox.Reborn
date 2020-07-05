using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Core.ModelView;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class TextureWrapper : ObjectWrapper
    {
        public STGenericTexture Texture => (STGenericTexture)ActiveObject;

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> items = new List<ToolMenuItem>();
            items.Add(new ToolMenuItem("Export", ExportTexture));
            items.Add(new ToolMenuItem("Replace", ReplaceTexture) { Enabled = Texture.CanEdit, });
            return items.ToArray();
        }

        private void ExportTexture(object sender, EventArgs e)
        {
            var selected = GetSelectedObjects<STGenericTexture>();
            if (selected.Count == 0) return;

            SaveDialogCustom sfd = new SaveDialogCustom();
            string name = selected[0].Name;
            foreach (var file in FileManager.GetExportableTextures())
            {
                if (file is ASTC && !Texture.IsASTC())
                    continue;

                if (file is IFileFormat)
                    sfd.AddFilter((IFileFormat)file);
            }

            sfd.DefaultExt = Runtime.GUI.DefaultImageExt;

            sfd.FolderDialog = selected.Count > 1;
            sfd.FileName = System.IO.Path.GetFileNameWithoutExtension(name);

            var result = sfd.ShowDialog();
            if (result == SaveDialogCustom.Result.OK) {
                if (selected.Count == 1)
                    selected[0].Export(sfd.FileName, new TextureExportSettings());
                else
                {
                    foreach (var file in selected)
                        file.Export($"{sfd.FolderPath}/{file.Name}" + sfd.DefaultExt, new TextureExportSettings());
                }
            }
        }

        private void ReplaceTexture(object sender, EventArgs e)
        {
            var selected = GetSelectedObjects<STGenericTexture>();
            if (selected.Count == 0) return;

            OpenDialogCustom ofd = new OpenDialogCustom();
            string name = selected[0].Name;
            foreach (var file in FileManager.GetImportableTextures())
            {
                if (file is IFileFormat)
                    ofd.AddFilter((IFileFormat)file);
            }

            ofd.MultiSelect = true;
            var result = ofd.ShowDialog();
            if (result == OpenDialogCustom.Result.OK)
            {
                TextureImportDialog importer = new TextureImportDialog();
                importer.LoadTextures(ofd.GetFiles(), selected[0]);
                if (importer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
            }
        }
    }
}
