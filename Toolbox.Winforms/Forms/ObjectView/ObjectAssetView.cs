using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using STLibrary.Forms;
using Toolbox.Core;
using Toolbox.Core.ModelView;

namespace Toolbox.Winforms
{
    public partial class ObjectAssetView : UserControl
    {
        public ObjectView ParentObjectView;

        public EventHandler OnNodeSelectionChanged;
        public EventHandler OnMouseDoubleClick;
        public EventHandler OnMouseRightClick;
        
        public ObjectTreeNode SelectedNode
        {
            get
            {
                if (stListView1.SelectedObjects.Count > 0)
                    return (ObjectTreeNode)stListView1.SelectedObject;
                else
                    return null;
            }
            set
            {
                stListView1.SelectedObject = value;
                stListView1.EnsureModelVisible(value);
            }
        }

        public List<ObjectTreeNode> Children { get; set; } = new List<ObjectTreeNode>();
        public List<ObjectTreeNode> CachedChildren { get; set; } = new List<ObjectTreeNode>();

        public ImageList ImageList;

        public void BeginUpdate()
        {
            stListView1.BeginUpdate();
        }

        public void EndUpdate()
        {
            stListView1.EndUpdate();
        }

        public void LoadImageList(ImageList imgList)
        {
            ImageList = imgList;

            stListView1.LargeImageList = new ImageList()
            { 
                ImageSize = new Size(64, 64),
                ColorDepth = ColorDepth.Depth32Bit 
            };

            stListView1.SmallImageList = new ImageList()
            {
                ImageSize = new Size(22, 22),
                ColorDepth = ColorDepth.Depth32Bit
            };

            this.Label.ImageGetter = delegate (object row) {
                ObjectTreeNode node = (ObjectTreeNode)row;

                String key = node.ImageKey;
                if (ImageList.Images.ContainsKey(key) && !this.stListView1.LargeImageList.Images.ContainsKey(key))
                {
                    Image smallImage = this.ImageList.Images[key];
                    Image largeImage = this.ImageList.Images[key];
                    this.stListView1.SmallImageList.Images.Add(key, smallImage);
                    this.stListView1.LargeImageList.Images.Add(key, largeImage);
                }
                return key;
            };

            displayType.Items.Add(View.Details);
            displayType.Items.Add(View.LargeIcon);
            displayType.Items.Add(View.SmallIcon);
            displayType.Items.Add(View.List);
            displayType.Items.Add(View.Tile);

            displayType.SelectedItem = stListView1.View;
        }

        public ObjectAssetView(ObjectView objectView)
        {
            InitializeComponent();

            ParentObjectView = objectView;

            stListView1.BackColor = FormThemes.BaseTheme.FormBackColor;
            stListView1.ForeColor = FormThemes.BaseTheme.FormForeColor;
            stListView1.HeaderStyle = ColumnHeaderStyle.Clickable;
            stListView1.FullRowSelect = true;
            stListView1.MultiSelect = true;
            stListView1.HideSelection = false;
            stListView1.UseCustomSelectionColors = true;
            stListView1.UnfocusedSelectedForeColor = stListView1.SelectedForeColor;
            stListView1.UnfocusedSelectedBackColor = stListView1.SelectedBackColor;
            stListView1.MouseClick += listView1_MouseClick;
            ReloadColumns();
        }

        public void ClearItems()
        {
            Children.Clear();
            CachedChildren.Clear();
            stListView1.ClearObjects();
            stListView1.UpdateObjects(Children);
        }

        public void Add(ObjectTreeNode node)
        {
            Children.Add(node);
            CachedChildren.Add(node);
            stListView1.UpdateObjects(Children);
        }

        public void Sort()
        {
            stListView1.Sort();
        }

        public void RemoveByTags(List<object> tags)
        {
            foreach (var tag in tags)
                RemoveByTag(tag);

            stListView1.UpdateObjects(Children);
        }

        public void RemoveByTag(object tag)
        {
            List<ObjectTreeNode> removedItems = new List<ObjectTreeNode>();
            foreach (ObjectTreeNode node in Children)
            {
                if (node.Tag == tag)
                    removedItems.Add(node);
            }

            foreach (var item in removedItems)
            {
                Children.Remove(item);
                stListView1.RemoveObject(item);
            }
        }

        public void SelectByTags(IEnumerable<object> tags)
        {
            List<ObjectTreeNode> nodes = new List<ObjectTreeNode>();
            foreach (ObjectTreeNode node in Children)
            {
                if (tags.Contains(node.Tag))
                {
                    nodes.Add(node);
                    stListView1.EnsureModelVisible(node);
                }
            }

            stListView1.SelectObjects(nodes);
            stListView1.Refresh();
        }

        public void SelectByTag(object tag)
        {
            foreach (ObjectTreeNode node in Children)
            {
                if (node.Tag == tag)
                {
                    stListView1.SelectObject(node);
                    stListView1.EnsureModelVisible(node);
                }
            }
            stListView1.Refresh();
        }

        public void LoadHiearchy()
        {
        }

        public void DeselectAll()
        {
            stListView1.DeselectAll();
            stListView1.Refresh();
        }

        private void ReloadArchiveFolderColumns()
        {
            stListView1.AllColumns.Clear();
            stListView1.AllColumns.Add(Label);
            stListView1.AllColumns.Add(new OLVColumn() { Text = "Type", AspectName = "Type" });
            stListView1.AllColumns.Add(new OLVColumn() { Text = "Size", AspectName = "Size" });
            ReloadColumns();
            stListView1.RebuildColumns();
            stListView1.Refresh();
        }

        private void ReloadColumns()
        {
            foreach (OLVColumn item in stListView1.AllColumns)
            {
                var headerstyle = new HeaderFormatStyle();
                headerstyle.SetBackColor(FormThemes.BaseTheme.FormBackColor);
                headerstyle.SetForeColor(FormThemes.BaseTheme.FormForeColor);
                item.HeaderFormatStyle = headerstyle;
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var nodes = GetSelectedNodes();
            if (e.Button == MouseButtons.Right) {
                OnMouseRightClick?.Invoke(sender, e);
            }
        }

   /*     private void LoadContextMenus(IContextMenuNode menuNode)
        {
            List<ToolStripItem> menuItems = new List<ToolStripItem>();
            foreach (var item in menuNode.GetContextMenuItems())
                menuItems.Add(item);

            stContextMenuStrip1.Items.Clear();
            stContextMenuStrip1.Items.AddRange(menuItems.ToArray());
            if (stContextMenuStrip1.Items.Count > 0)
                stContextMenuStrip1.Show(Cursor.Position);
        }*/

        private void searchTB_TextChanged(object sender, EventArgs e)
        {
            if (searchTB.Text != string.Empty)
            {
                stListView1.ClearObjects();
                Children.Clear();
                foreach (var child in CachedChildren)
                {
                    bool HasText = child.Label.IndexOf(searchTB.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (HasText)
                        Children.Add(child);
                }

                stListView1.UpdateObjects(Children);
            }
            else
            {
                stListView1.ClearObjects();
                Children.Clear();
                foreach (var child in CachedChildren)
                    Children.Add(child);

                stListView1.UpdateObjects(Children);
            }
        }

        private void stListView1_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"SelectedObjects {stListView1.SelectedObjects.Count}");

            OnNodeSelectionChanged?.Invoke(this, e);
        }

        public void UpdateSelectedObjects()
        {
            stListView1.UpdateObjects(stListView1.SelectedObjects);
        }

        private ParentRedir PreviousItem = null;

        public List<ObjectTreeNode> GetSelectedNodes()
        {
            Console.WriteLine($"SelectedObjectsSE {stListView1.SelectedObjects.Count}");

            List<ObjectTreeNode> nodes = new List<ObjectTreeNode>();
            foreach (var obj in stListView1.SelectedObjects)
                if (obj is ObjectTreeNode)
                    nodes.Add((ObjectTreeNode)obj);
            return nodes;
        }

        private void stListView1_DoubleClick(object sender, EventArgs e)
        {
            OnMouseDoubleClick?.Invoke(sender, e);

            if (stListView1.SelectedObjects.Count == 1) {
                var obj = stListView1.SelectedObjects[0];

                if (obj is ParentRedir)
                    LoadFolder(((ParentRedir)obj).Parent);
                else
                    LoadFolder((ObjectTreeNode)obj);
            }
        }

        public void LoadRoot(ObjectTreeNode node)
        {
            Children.Clear();
            CachedChildren.Clear();
            Children.Add(node);
            CachedChildren.Add(node);

            stListView1.UpdateObjects(Children);

            ReloadArchiveFolderColumns();
        }

        public void LoadFolder(ObjectTreeNode node) {
            if (node.ChildCount == 0 || node == null) return;

            hiearchyTextView.Text = SetupDirectoryPath(node.FullPath);

            if (node.Parent != null)
                PreviousItem = new ParentRedir(node.Parent);

            Children.Clear();
            Children.Add(PreviousItem);
            foreach (var child in node.Children)
                Children.Add(child);

            stListView1.ClearObjects();
            stListView1.UpdateObjects(Children);
        }

        private string SetupDirectoryPath(string dir)
        {
            return " > " + dir.Replace("/", " > ");
        }

        private class ParentRedir : ObjectTreeNode
        {
            public ParentRedir(ObjectTreeNode node) {
                Parent = node;
                Label = "......";
            }
        }

        private void displayType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (displayType.SelectedIndex == -1)
                return;

            this.stListView1.View = (View)displayType.SelectedItem;
            if (stListView1.View == View.Details)
                stListView1.HeaderStyle = ColumnHeaderStyle.Clickable;
            else
                stListView1.HeaderStyle = ColumnHeaderStyle.None;
        }
    }
}
