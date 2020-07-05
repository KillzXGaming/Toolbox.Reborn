using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Toolbox.Core.ModelView;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    public class ArchiveFileWrapper : ObjectTreeNode
    {
        private ObjectTreeNode ReferenceNode;

        public IArchiveFile ArchiveFile;

        public ArchiveFileWrapper(ObjectTreeNode node)
        {
            ArchiveFile = (IArchiveFile)node.Tag;

            ReferenceNode = node;
            Label = node.Label;
            Tag = node.Tag;
            ImageKey = node.ImageKey;

            if (node.ChildCount > 0)
            {
                ImageKey = "Folder";
                Children.Add(new ObjectTreeNode("dummy"));
            }
        }

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> menus = new List<ToolMenuItem>();
            menus.Add(new ToolMenuItem("Export All", ExportAll));
            menus.Add(new ToolMenuItem("Add File", AddFile) { Enabled = ArchiveFile.CanAddFiles, });
            menus.Add(new ToolMenuItem("Add Folder", AddFolder) { Enabled = ArchiveFile.CanAddFiles, });

            return menus.ToArray();
        }

        private bool expanded = false;
        public override void OnBeforeExpand()
        {
            if (expanded)
                return;

            Children.Clear();
            foreach (var node in ReferenceNode.Children)
                Children.Add(new ArchiveHiearchy(ArchiveFile, node));

            expanded = true;
        }

        public override void OnAfterCollapse()
        {

        }

        private void ExportAll(object sender, EventArgs e)
        {
            FolderSelectDialog ofd = new FolderSelectDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                var archiveFiles = ArchiveFile.Files.ToList();
                ProgressWindow.Start(() => ExtractFiles(ofd.SelectedPath, archiveFiles), MainForm.Instance);
            }
        }

        private void ExtractFiles(string folder, List<ArchiveFileInfo> archiveFiles)
        {
            for (int i = 0; i < archiveFiles.Count; i++)
            {
                var counter = (i * 100) / archiveFiles.Count;
                ProgressWindow.Update($"Extracting {archiveFiles[i].FileName}", counter);
                archiveFiles[i].FileWrite($"{folder}/{archiveFiles[i].FileName}");
            }
            ProgressWindow.Wait(1000);
            ProgressWindow.Update($"Finished!", 100);
            ProgressWindow.CloseProgressBar();
        }

        private void AddFile(object sender, EventArgs e)
        {

        }

        private void AddFolder(object sender, EventArgs e)
        {

        }
    }
}
