using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Toolbox.Core.ModelView;
using Toolbox.Core.Animations;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class SkeletalAnimationWrapper : ObjectWrapper
    {
        public Stream StreamData => (Stream)ActiveObject;

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> items = new List<ToolMenuItem>();
            items.Add(new ToolMenuItem("Export Animation", Export));
            return items.ToArray();
        }

        private void Export(object sender, EventArgs e)
        {
            var selected = GetSelectedNodes<STSkeletonAnimation>();
            if (selected.Count == 0) return;


            FileManager.GetExportableAnimations();

            SaveDialogCustom sfd = new SaveDialogCustom();
            sfd.FolderDialog = selected.Count > 1;
            sfd.FileName = selected[0].Label;
            foreach (IFileFormat file in FileManager.GetExportableAnimations())
                sfd.AddFilter(file);

            var result = sfd.ShowDialog();
            if (result == SaveDialogCustom.Result.OK) {
                foreach (var file in FileManager.GetExportableAnimations())
                {
                    
                }

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
