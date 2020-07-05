using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using STLibrary.Forms;
using Toolbox.Core;
using Toolbox.Core.ModelView;
using Toolbox.Core.IO;
using Toolbox.Core.Animations;
using Toolbox.Core.Imaging;
using Toolbox.Winforms.ObjectWrappers;
using System.IO;

namespace Toolbox.Winforms
{
    public partial class ObjectView : STForm, IFileEditor
    {
        List<IFileFormat> fileFormats = new List<IFileFormat>();

        public IFileFormat[] GetFileFormats() {
            return fileFormats.ToArray();
        }

        //Every object view has a viewport attatched
        //This is so viewports in each workspace share the same settings for same files
        //and lists of models and info can all be loaded in easily
        public Viewport Viewport => WorkspacePanel == null ? null : WorkspacePanel.Viewport;
        public WorkspacePanel WorkspacePanel;

        public ObjectHiearchy ObjectHiearchy;
        public ObjectAssetView ObjectAssetView;

        STContextMenuStrip treeNodeContextMenu;

        public ImageList imgList;

        public static ImageList ImageList;

        public ObjectView()
        {
            InitializeComponent();

            treeNodeContextMenu = new STContextMenuStrip();

            ObjectHiearchy = new ObjectHiearchy(this) { Dock = DockStyle.Fill, };
            ObjectAssetView = new ObjectAssetView(this) { Dock = DockStyle.Fill, };
            chkUseActiveEditor.Checked = Runtime.ObjectEditor.OpenInActiveEditor;

            imgList = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(64, 64),
            };

            imgList.Images.Add("TextFile", Properties.Resources.TextFile);
            imgList.Images.Add("HeaderFile", Properties.Resources.HeaderFile);
            imgList.Images.Add("Texture", Properties.Resources.Texture);
            imgList.Images.Add("TextureContainer", Properties.Resources.TextureContainer);
            imgList.Images.Add("Folder", Properties.Resources.Folder);
            imgList.Images.Add("FruitFile", Properties.Resources.FruitFile);
            imgList.Images.Add("ShaderFile", Properties.Resources.Shader);
            imgList.Images.Add("File", Properties.Resources.File);
            imgList.Images.Add("Bone", Properties.Resources.Bone);
            imgList.Images.Add("Mesh", Properties.Resources.Mesh);
            imgList.Images.Add("Model", Properties.Resources.Model);

            var pluginFileIcons = FileManager.GetFileIconLoaders();
            Console.WriteLine($"pluginFileIcons {pluginFileIcons.ToList().Count}");
            foreach (var loader in pluginFileIcons)
            {
                foreach (var resource in loader.ImageList)
                {
                    if (!imgList.Images.ContainsKey(resource.Key))
                        imgList.Images.Add(resource.Key, LoadImageResource(resource.Value));
                }
            }

            ImageList = imgList;

            chkUseActiveEditor.Checked = Runtime.ObjectEditor.OpenInActiveEditor;

            ObjectHiearchy.LoadImageList(imgList);
            ObjectAssetView.LoadImageList(imgList);

            tabPage1.Controls.Add(ObjectHiearchy);
            tabPage3.Controls.Add(ObjectAssetView);

            ObjectHiearchy.OnNodeSelectionChanged += ObjectList_NodeSelectionChanged;
            ObjectAssetView.OnNodeSelectionChanged += ObjectList_NodeSelectionChanged;
            ObjectHiearchy.OnMouseRightClick += ObjectList_RightClicked;
            ObjectAssetView.OnMouseRightClick += ObjectList_RightClicked;

            ObjectHiearchy.OnMouseDoubleClick += ObjectList_DoubleClick;
        }

        public Bitmap LoadImageResource(byte[] imageData)
        {
            using (var ms = new System.IO.MemoryStream(imageData)) {
                return new Bitmap(ms);
            }
        }

        public void ReloadFile(IFileFormat fileFormat)
        {
            ObjectTreeNode hiearchyNode = ObjectListWrapperLoader.OpenFormat(imgList, fileFormat);

            ObjectHiearchy.ClearItems();
            ObjectHiearchy.Add(hiearchyNode);
            ObjectAssetView.LoadRoot(hiearchyNode);
            ObjectHiearchy.UpdateCache(hiearchyNode);

            if (fileFormat is ITextureContainer && hiearchyNode.ChildCount >= 1)
                ObjectHiearchy.SelectedNode = hiearchyNode.Children.FirstOrDefault();
            else if (ObjectHiearchy.Children.Count > 0)
                ObjectHiearchy.SelectedNode = ObjectHiearchy.Children.LastOrDefault();
        }

        public void LoadFormat(ICompressionFormat fileFormat, System.IO.Stream stream, string fileName)
        {
            ObjectTreeNode dataNode = new ObjectTreeNode(System.IO.Path.GetFileName(fileName));
            dataNode.Tag = stream;
            ObjectHiearchy.Add(dataNode);
        }

        public void LoadFormat(IFileFormat fileFormat)
        {
            fileFormats.Add(fileFormat);
            ObjectTreeNode hiearchyNode = ObjectListWrapperLoader.OpenFormat(imgList, fileFormat);

            ObjectHiearchy.Add(hiearchyNode);
            ObjectAssetView.LoadRoot(hiearchyNode);
            ObjectHiearchy.UpdateCache(hiearchyNode);

            if (fileFormat is ITextureContainer && hiearchyNode.ChildCount >= 1)
            {
                ObjectHiearchy.Expand(hiearchyNode);
                ObjectHiearchy.SelectedNode = hiearchyNode.Children.FirstOrDefault();

                if (((ITextureContainer)fileFormat).DisplayIcons)
                    ReloadTextureIcons((ITextureContainer)fileFormat, hiearchyNode.Children);
            }
            else
                ObjectHiearchy.SelectedNode = hiearchyNode;
        }


        private void ReloadTextureIcons(ITextureContainer textureContainer, List<ObjectTreeNode> children)
        {
            var fileName = ((IFileFormat)textureContainer).FileInfo.FilePath;

            var textureList = textureContainer.TextureList.ToList();
            Thread Thread = new Thread((ThreadStart)(() =>
            {
                for (int i = 0; i < textureList.Count; i++)
                {
                    var image = textureList[i].GetBitmap();
                    if (image != null)
                    {
                        //  uint checksum = Core.Hashes.ChecksumQuick.GetChecksum(image);
                        //   children[i].ImageKey = checksum.ToString();
                        children[i].ImageKey = fileName + textureList[i].Name;

                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!this.imgList.Images.ContainsKey(children[i].ImageKey))
                                    this.imgList.Images.Add(children[i].ImageKey, image);
                            });
                        }
                        else
                        {
                            if (!this.imgList.Images.ContainsKey(children[i].ImageKey))
                                this.imgList.Images.Add(children[i].ImageKey, image);
                        }
                    }
                }
            }));
            Thread.Start();
        }


        private List<ObjectTreeNode> GetSelectedObjects() {
            return ObjectHiearchy.GetSelectedNodes();
        }

        private ObjectTreeNode GetSelectedObject(object sender)
        {
            if (sender == ObjectAssetView)
                return ObjectAssetView.SelectedNode;
            else if (sender == ObjectHiearchy)
                return ObjectHiearchy.SelectedNode;
            return null;
        }

        private void ObjectList_RightClicked(object sender, EventArgs e)
        {
            ObjectTreeNode obj = ObjectHiearchy.GetSelectedNodes().FirstOrDefault();
            if (obj == null || obj.Tag == null)
                return;

            LoadContextMenus(obj);
        }

        internal static List<object> SelectedObjects = new List<object>();
        internal static List<ObjectTreeNode> SelectedNodes = new List<ObjectTreeNode>();

        private void LoadContextMenus(ObjectTreeNode node)
        {
            SelectedObjects = new List<object>();
            SelectedNodes = new List<ObjectTreeNode>();
            foreach (var t in GetSelectedObjects()) {
                SelectedObjects.Add(t.Tag);
                SelectedNodes.Add(t);
            }

            List<ToolStripItem> items = new List<ToolStripItem>();

            int counter = 0;

            var tag = node.Tag;
            foreach (var type in ObjectViewWrapperHandler.Handlers)
            {
                if (type.Key.IsAssignableFrom(tag.GetType()))
                {
                    if (items.Count > 0)
                        items.Add(new STToolStripSeparator()); 

                    type.Value.ActiveObject = tag;
                    foreach (var menu in type.Value.GetContextMenuItems()) {
                        items.Add(new STToolStipMenuItem(menu.Name, null, menu.Click) { Enabled = menu.Enabled, });
                    }
                }
            }

            if (node is ArchiveHiearchy) {
                items.Add(new STToolStipMenuItem("Export Raw Data", null, ArchiveExport));
            }

            if (tag is IModelFormat)
                items.Add(new STToolStipMenuItem("Export Model", null, ExportModel));
            if (tag is IFileFormat)
            {
                if (File.Exists(((IFileFormat)tag).FileInfo.FilePath)) {
                    items.Add(new STToolStipMenuItem("Open In Explorer", null, SelectFileInExplorer, Keys.Control | Keys.Q));
                }
            }
            if (tag is IModelSceneFormat)
            {

            }

            var menus = node.GetContextMenuItems();
            foreach (var menu in menus) {
                if (menu is ToolMenuItemSeparator)
                    items.Add(new STToolStripSeparator());
                else
                    items.Add(new STToolStipMenuItem(menu.Name, null, menu.Click) { Enabled = menu.Enabled, });
            }

            if (items.Count > 0)
            {
                treeNodeContextMenu.Items.Clear();
                treeNodeContextMenu.Items.AddRange(items.ToArray());
                treeNodeContextMenu.Show(Cursor.Position);
            }
        }

        private void SelectFileInExplorer(object sender, EventArgs args)
        {
            var selected = GetSelectedTags<IFileFormat>();
            if (selected.Count == 0) return;

            if (selected.Count == 1)
            {
                string argument = "/select, \"" + selected[0].FileInfo.FilePath + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void ExportModel(object sender, EventArgs e)
        {
            var selected = GetSelectedTags<IModelFormat>();
            if (selected.Count == 0) return;

            if (selected.Count == 1)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".dae";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportModelSettings dlg = new ExportModelSettings();
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        var settings = dlg.Settings;
                        foreach (var mdl in selected)
                        {
                            Toolbox.Core.Collada.DAE.Export(sfd.FileName, settings, mdl);
                        }
                    }
                }
            }
            else
            {
                FolderSelectDialog ofd = new FolderSelectDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in selected)
                    {
                        ExportModelSettings dlg = new ExportModelSettings();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            var settings = dlg.Settings;
                            foreach (var mdl in selected)
                            {
                                Toolbox.Core.Collada.DAE.Export($"{ofd.SelectedPath}/{mdl}.dae", settings, mdl);
                            }
                        }
                    }
                }
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {
            var selected = GetSelectedTags<IFileFormat>();
            if (selected.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            string name = selected[0].FileInfo.FileName;
            sfd.FileName = name;
            sfd.DefaultExt = System.IO.Path.GetExtension(name);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                STFileSaver.SaveFileFormat(selected[0], sfd.FileName);
            }
        }

        private void ArchiveExport(object sender, EventArgs e)
        {
            var selected = GetSelectedNodes<ArchiveHiearchy>();
            if (selected.Count == 0) return;

            List<ArchiveFileInfo> archiveFiles = new List<ArchiveFileInfo>();
            foreach (var file in selected)
                archiveFiles.Add(file.ArchiveFileInfo);

            Console.WriteLine($"selected {selected.Count}");

            if (archiveFiles.Count == 1)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                string name = System.IO.Path.GetFileName(archiveFiles[0].FileName);
                sfd.FileName = name;
                sfd.DefaultExt = System.IO.Path.GetExtension(name);
                if (sfd.ShowDialog() == DialogResult.OK) {
                    Task.Run(() => FileWriteAsync(archiveFiles[0], sfd.FileName));
                }
            }
            else
            {
                FolderSelectDialog ofd = new FolderSelectDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    ProgressWindow.Start(() => ExtractFiles(ofd.SelectedPath, archiveFiles), MainForm.Instance);
                }
            }
        }

        private void ExtractFiles(string folder, List<ArchiveFileInfo> archiveFiles)
        {
            for (int i = 0; i < archiveFiles.Count; i++) {
                var counter = (i * 100) / archiveFiles.Count;
                ProgressWindow.Update($"Extracting {archiveFiles[i].FileName}", counter);
                archiveFiles[i].FileWrite($"{folder}/{archiveFiles[i].FileName}");
            }
            ProgressWindow.Wait(1000);
            ProgressWindow.Update($"Finished!", 100);
            ProgressWindow.CloseProgressBar();
        }

        public void FileWriteAsync(ArchiveFileInfo fileInfo, string filePath) {
            fileInfo.DecompressData(fileInfo.FileData).SaveToFile(filePath);
        }

        private void ArchiveReplace(object sender, EventArgs e)
        {

        }

        private void ArchiveRename(object sender, EventArgs e)
        {

        }

        private List<T> GetSelectedNodes<T>() where T : ObjectTreeNode, new()
        {
            List<T> nodes = new List<T>();
            foreach (var obj in GetSelectedObjects())
            {
                if (typeof(T).IsAssignableFrom(obj.GetType()))
                    nodes.Add((T)obj);
            }
            Console.WriteLine($"GetSelectedNodes {nodes.Count}");
            return nodes;
        }

        private List<T> GetSelectedTags<T>()
        {
            List<T> tags = new List<T>();
            foreach (var obj in GetSelectedObjects())
            {
                if (obj.Tag == null)
                    continue;

                if (typeof(T).IsAssignableFrom(obj.Tag.GetType()))
                    tags.Add((T)obj.Tag);
            }
            return tags;
        }

        private void SelectionChanged(ObjectTreeNode obj)
        {
            bool tryUpdateViewport = false;
            if (Runtime.SelectedBoneIndex != -1)
            {
                tryUpdateViewport = true;
                Runtime.SelectedBoneIndex = -1;
            }

            if (obj.Tag is ArchiveFileInfo)
            {
                var fileInfo = (ArchiveFileInfo)obj.Tag;

                ArchiveFilePanel hexEditor = GetActiveEditor<ArchiveFilePanel>();
                hexEditor.LoadFile(this, fileInfo);
            }
            else if (obj.Tag is STGenericTexture)
            {
                var tex = (STGenericTexture)obj.Tag;

                ImageEditorBase editor = GetActiveEditor<ImageEditorBase>();
                editor.LoadProperties(tex.DisplayProperties);
                editor.LoadImage(tex);
            }
            else if (obj.Tag is IPropertyDisplay)
            {
                var property = (IPropertyDisplay)obj.Tag;

                STPropertyGrid editor = GetActiveEditor<STPropertyGrid>();
                editor.LoadProperty(property.PropertyDisplay);
            }
            else if (obj.Tag is IEditorDisplay)
            {
                var property = (IEditorDisplay)obj.Tag;
                Console.WriteLine($"IEditorDisplay {property}");

                var gui = GUIGenerator.Generate(property);
                gui.Dock = DockStyle.Fill;

                STPanel editor = GetActiveEditor<STPanel>();
                editor.Controls.Clear();
                editor.Controls.Add(gui);
            }
            else if (obj.Tag is System.IO.Stream)
            {
                var stream = (System.IO.Stream)obj.Tag;

                HexEditor hexEditor = GetActiveEditor<HexEditor>();
                hexEditor.LoadData(stream);
            }
            else if (obj.Tag is IVideoFormat)
            {
                VideoPlayer editor = GetActiveEditor<VideoPlayer>();
                editor.LoadVideoFile((IVideoFormat)obj.Tag);
            }
            else if (obj.Tag is STGenericMesh)
            {
                var mesh = (STGenericMesh)obj.Tag;
                mesh.SelectMesh?.Invoke(mesh, EventArgs.Empty);

                if (WorkspacePanel == null)
                    WorkspacePanel = CreateWorkspacePanel();

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(WorkspacePanel);
                var meshEditor = workspace.GetActiveEditor<MeshEditor>();
                meshEditor.LoadMesh(mesh);
                workspace.UpdateViewport();
                ObjectHiearchy.Focus();
            }
            else if (obj.Tag is STGenericMaterial)
            {
                var mat = (STGenericMaterial)obj.Tag;

                if (WorkspacePanel == null)
                    WorkspacePanel = CreateWorkspacePanel();

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(WorkspacePanel);
                var materialEditor = workspace.GetActiveEditor<MaterialEditor>();
                materialEditor.LoadMaterial(mat);
                workspace.UpdateViewport();
            }
            else if (obj.Tag is IModelFormat)
            {
                if (WorkspacePanel == null)
                    WorkspacePanel = CreateWorkspacePanel();

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(WorkspacePanel);
                workspace.LoadFileFormat((IModelFormat)(obj.Tag));
                workspace.UpdateViewport();
            }
            else if (obj.Tag is IModelSceneFormat)
            {
                if (WorkspacePanel == null)
                    WorkspacePanel = CreateWorkspacePanel();

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(WorkspacePanel);
                workspace.LoadFileFormat((IModelSceneFormat)(obj.Tag));
                workspace.UpdateViewport();
            }
            else if (obj.Tag is STAnimation)
            {
                if (WorkspacePanel == null)
                    WorkspacePanel = CreateWorkspacePanel();

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(WorkspacePanel);
                workspace.LoadFileFormat((STAnimation)(obj.Tag));
                workspace.UpdateViewport();
            }
            else if (obj.Tag is STBone)
            {
                var bone = (STBone)obj.Tag;
                Runtime.SelectedBoneIndex = bone.Index;
                WorkspacePanel.UpdateViewport();
            }

            if (tryUpdateViewport && WorkspacePanel != null)
                WorkspacePanel.UpdateViewport();
            // else
            //    GetActiveEditor<STPanel>();
        }

        private WorkspacePanel CreateWorkspacePanel()
        {
            var workspace = new WorkspacePanel() { Dock = DockStyle.Fill };
            workspace.Viewport.ObjectSelected += Viewport_SelectionChanged;
            return workspace;
        }

        private void Viewport_SelectionChanged(object sender, EventArgs e)
        {
            if (Viewport == null) return;

            if (!Control.ModifierKeys.HasFlag(Keys.Shift))
                ObjectHiearchy.DeselectAll();

            ObjectHiearchy.SelectByTags(Viewport.GetSelectedMeshes());
        }

        private void ObjectList_NodeSelectionChanged(object sender, EventArgs e)
        {
            ObjectTreeNode obj = GetSelectedObject(sender);
            if (obj == null)
                return;

            if (Viewport != null)
                Viewport.DeselectAllObjects();

            SelectionChanged(obj);
        }

        private Control ActiveEditor
        {
            get
            {
                if (contentPanel.Controls.Count == 0) return null;
                return contentPanel.Controls[0];
            }
        }

        private T GetActiveEditor<T>() where T : Control, new()
        {
            T instance = new T();

            if (ActiveEditor?.GetType() == instance.GetType())
                return ActiveEditor as T;
            else
            {
                DisposeEdtiors();
                contentPanel.Controls.Clear();
                instance.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(instance);
            }

            return instance;
        }

        private T GetActiveEditor<T>(Control control) where T : Control, new()
        {
            if (ActiveEditor?.GetType() == control.GetType())
                return control as T;
            else
            {
                DisposeEdtiors();
                contentPanel.Controls.Clear();
                control.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(control);
            }

            return control as T;
        }

        private void DisposeEdtiors()
        {
            if (ActiveEditor == null) return;
            if (ActiveEditor is STUserControl)
                ((STUserControl)ActiveEditor).OnControlClosing();
        }

        private void DisposeFileFormats(IFileFormat fileFormat)
        {
            if (fileFormat is IArchiveFile) {
                foreach (var file in ((IArchiveFile)fileFormat).Files) {
                    if (file.FileFormat != null)
                        DisposeFileFormats(file.FileFormat);
                    file.FileData?.Dispose();
                }
            }

            if (fileFormat.FileInfo != null)
                fileFormat.FileInfo.Stream?.Dispose();
            if (fileFormat is IDisposable)
                ((IDisposable)fileFormat).Dispose();
        }

        private void ObjectList_DoubleClick(object sender, EventArgs e)
        {
            var nodes = GetSelectedObjects();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Tag is ArchiveFileInfo) {
                    var archiveInfo = (ArchiveFileInfo)nodes[i].Tag;
                    archiveInfo.FileFormat = archiveInfo.OpenFile();

                    if (archiveInfo.FileFormat != null)
                    {
                        var fileNode = ObjectListWrapperLoader.OpenFormat(imgList, archiveInfo.FileFormat);
                        nodes[i].Tag = archiveInfo.FileFormat;
                        foreach (var child in fileNode.Children)
                            nodes[i].AddChild(child);
                    }
                    SelectionChanged(nodes[i]);
                }
            }
        }

        private void sortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ObjectHiearchy.Sort();
        }

        private void ObjectView_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void ObjectView_FormClosing(object sender, FormClosingEventArgs e)
        {
            imgList.Images.Clear();
            ImageList.Images.Clear();

            foreach (var file in fileFormats)
                DisposeFileFormats(file);

            foreach (Control control in contentPanel.Controls) {
                if (control is STUserControl)
                    ((STUserControl)control).OnControlClosing();
                control.Dispose();
            }

            if (Viewport != null) {
                Viewport.OnControlClosing();
                Viewport.Dispose();
            }
        }

        private void chkUseActiveEditor_CheckedChanged(object sender, EventArgs e) {
            Runtime.ObjectEditor.OpenInActiveEditor = chkUseActiveEditor.Checked;
        }
    }
}
