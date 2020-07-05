using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using Toolbox.Core.ModelView;
using Toolbox.Core.IO;
using Toolbox.Core;
using Toolbox.Winforms.ObjectWrappers;

namespace Toolbox.Winforms
{
    public class ArchiveHiearchy : ObjectTreeNode
    {
        private ObjectTreeNode ReferenceNode;

        public List<ObjectTreeNode> GetChildren()
        {
            OnBeforeExpand();
            return Children;

            if (expanded) 
                return Children;
            else
                return ReferenceNode.Children;
        }

        public ArchiveFileInfo ArchiveFileInfo;
        public IArchiveFile ArchiveFile;

        public ArchiveHiearchy() { }

        public ArchiveHiearchy(IArchiveFile archiveFile, ObjectTreeNode node)
        {
            if (node.Tag is ArchiveFileInfo) {
                ArchiveFileInfo = (ArchiveFileInfo)node.Tag;
                if (ArchiveFileInfo.OpenFileFormatOnLoad) {
                    ArchiveFileInfo.FileFormat = ArchiveFileInfo.OpenFile();
                }
                if (ArchiveFileInfo.FileFormat != null)
                {
                    expanded = true;
                    var fileNode = ObjectListWrapperLoader.OpenFormat(ObjectView.ImageList, ArchiveFileInfo.FileFormat);
                    node.Tag = ArchiveFileInfo.FileFormat;
                    foreach (var child in fileNode.Children)
                        AddChild(child);
                }
            }

            ArchiveFile = archiveFile;
            ReferenceNode = node;
            Label = node.Label;
            Tag = node.Tag;
            ImageKey = node.ImageKey;

            if (node.ChildCount > 0)
            {
                ImageKey = "Folder";
                Children.Add(new ObjectTreeNode("dummy"));
            }

            if (ArchiveFileInfo != null)
            {
                foreach (var loader in FileManager.GetFileIconLoaders())
                {
                    string key = loader.Identify(Label, ArchiveFileInfo.FileData);
                    if (key != string.Empty)
                        ImageKey = key;
                }
            }
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

        public override ToolMenuItem[] GetContextMenuItems()
        {
            List<ToolMenuItem> menus = new List<ToolMenuItem>();
            menus.Add(new ToolMenuItem("Replace Raw Data", ReplaceAction));
            menus.Add(new ToolMenuItem("Export Raw Data to File Location", ExportToFileLocAction));
            return menus.ToArray();
        }

        private void LoadPreloadedFiles(List<ObjectTreeNode> nodes)
        {
            ProgressWindow.Start(() =>
            {
                foreach (var node in nodes)
                {
                    ProgressWindow.Update($"Opening {ArchiveFileInfo.FileName}", 0, true);

                    ArchiveFileInfo.FileFormat = ArchiveFileInfo.OpenFile();
                    if (ArchiveFileInfo.FileFormat != null)
                    {
                        var fileNode = ObjectListWrapperLoader.OpenFormat(new ImageList(), ArchiveFileInfo.FileFormat);
                        node.Tag = ArchiveFileInfo.FileFormat;
                        foreach (var child in fileNode.Children)
                            AddChild(child);
                    }
                }

                ProgressWindow.Wait(1000);
                ProgressWindow.Update($"Finished!", 100);
                ProgressWindow.CloseProgressBar();
            });
        }

        public override void OnAfterCollapse()
        {
        
        }

        public void FileWriteAsync(ArchiveFileInfo fileInfo, string filePath) {
            Task.Run(() => ArchiveFileInfo.DecompressData(ArchiveFileInfo.FileData).SaveToFile(filePath));
        }

        private void ReplaceAction(object sender, EventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Raw Data (*.*)|*.*";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK) {
                ArchiveFileInfo.FileData = ArchiveFileInfo.CompressData(new MemoryStream(File.ReadAllBytes(ofd.FileName)));
                ArchiveFileInfo.FileFormat = null;
                this.Children.Clear();
                this.Tag = ArchiveFileInfo;

                /*        if (ArchiveFileInfo.FileFormat != null) {
                            ArchiveFileInfo.FileFormat = null;

                            ArchiveFileInfo.FileFormat = ArchiveFileInfo.OpenFile();
                            if (ArchiveFileInfo.FileFormat != null)
                            {
                                var fileNode = ObjectListWrapperLoader.OpenFormat(new ImageList(), ArchiveFileInfo.FileFormat);
                                this.Tag = ArchiveFileInfo.FileFormat;
                                foreach (var child in fileNode.Children)
                                    this.AddChild(child);
                            }
                        }*/
            }
        }

        private void ExportToFileLocAction(object sender, EventArgs args)
        {
            string folder = ((IFileFormat)ArchiveFile).FileInfo.FolderPath;
            string filePath = Path.Combine(folder, Label);
            Console.WriteLine($"filePath {filePath}");

            ProgressWindow.Start(() =>
            {
                ArchiveFileInfo.FileWrite(filePath);
                ProgressFinished();
            });
            ProgressWindow.Update($"Exporting {Label}", 0, true);
        }

        private void ProgressFinished() {
            ProgressWindow.CloseProgressBar();
        }
    }
}
