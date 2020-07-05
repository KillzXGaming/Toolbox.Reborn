using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Core.ModelView;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class TextureContainerWrapper : ObjectWrapper
    {
        public ITextureContainer TextureContainer => (ITextureContainer)ActiveObject;

        public override ToolMenuItem[] GetContextMenuItems()
        {
            bool canEdit = TextureContainer.TextureList.Any(x => x.CanEdit);

            List<ToolMenuItem> items = new List<ToolMenuItem>();
            items.Add(new ToolMenuItem("Export All", ExportAllTextures));
            items.Add(new ToolMenuItem("Import Texture", ImportTexture) { Enabled = canEdit, });
            items.Add(new ToolMenuItem("Replace Textures (From Folder)", ReplaceTexture) { Enabled = canEdit, });
            return items.ToArray();
        }

        private void ExportAllTextures(object sender, EventArgs e)
        {
            var selected = GetSelectedObjects<ITextureContainer>();
            if (selected.Count == 0) return;


            SaveDialogCustom sfd = new SaveDialogCustom();
            sfd.FolderDialog = true;

            var result = sfd.ShowDialog();
            if (result == SaveDialogCustom.Result.OK) {

                List<STGenericTexture> textures = new List<STGenericTexture>();
                foreach (var container in selected)
                    textures.AddRange(container.TextureList);

                TextureExportDialog textureDlg = new TextureExportDialog();
                textureDlg.LoadTextures(textures);
                if (textureDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
            }
        }

        private void ImportTexture(object sender, EventArgs e)
        {

        }

        private void ReplaceTexture(object sender, EventArgs e)
        {
            var selected = GetSelectedObjects<ITextureContainer>();
            if (selected.Count == 0) return;

            OpenDialogCustom ofd = new OpenDialogCustom();
            ofd.FolderDialog = true;
            var result = ofd.ShowDialog();
            if (result == OpenDialogCustom.Result.OK)
            {
                foreach (var file in ofd.GetFiles())
                {

                }
            }
        }
    }
}
