using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Toolbox.Core.ModelView;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class StreamWrapper : ObjectWrapper
    {
        public Stream StreamData => (Stream)ActiveObject;

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> items = new List<ToolMenuItem>();
            items.Add(new ToolMenuItem("Export", Export));
            return items.ToArray();
        }

        private void Export(object sender, EventArgs e)
        {
            var selected = GetSelectedNodes<Stream>();
            if (selected.Count == 0) return;

            SaveDialogCustom sfd = new SaveDialogCustom();
            sfd.FolderDialog = selected.Count > 1;
            sfd.FileName = selected[0].Label;
            var result = sfd.ShowDialog();
            if (result == SaveDialogCustom.Result.OK) {
                if (selected.Count == 1)
                    StreamData.SaveToFile(sfd.FileName);
                else
                {
                    foreach (var file in selected)
                        ((Stream)file.Tag).SaveToFile($"{sfd.FolderPath}/{file.Label}");
                }
            }
        }
    }
}
