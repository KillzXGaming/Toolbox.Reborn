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
using Toolbox.Core.ModelView;
using Toolbox.Core;
using STLibrary.Forms;

namespace STLibrary.Forms.MapEditor
{
    public partial class ObjectHiearchy : UserControl
    {
        public EventHandler OnNodeSelectionChanged;
        public EventHandler OnMouseDoubleClick;

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
            { ColorDepth = ColorDepth.Depth32Bit };

            stListView1.SmallImageList = new ImageList()
            { ColorDepth = ColorDepth.Depth32Bit };

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
        }

        public ObjectHiearchy()
        {
            InitializeComponent();

            stListView1.BackColor = FormThemes.BaseTheme.FormBackColor;
            stListView1.ForeColor = FormThemes.BaseTheme.FormForeColor;
            stListView1.HeaderStyle = ColumnHeaderStyle.None;
            stListView1.FullRowSelect = true;
            stListView1.MultiSelect = true;
            stListView1.HideSelection = false;
            stListView1.UseCustomSelectionColors = true;
            stListView1.UnfocusedSelectedForeColor = stListView1.SelectedForeColor;
            stListView1.UnfocusedSelectedBackColor = stListView1.SelectedBackColor;
            stListView1.MouseClick += listView1_MouseClick;

            // Configure the tree
            stListView1.CanExpandGetter = delegate (object x) { return ((ObjectTreeNode)x).ChildCount > 0; };
            stListView1.ChildrenGetter = delegate (object x) { return ((ObjectTreeNode)x).Children; };
            stListView1.Roots = Children;
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

        public void SelectObject(ObjectTreeNode obj)
        {
            stListView1.SelectObject(obj);
            stListView1.Refresh();
        }

        public void SelectObjects(List<ObjectTreeNode> objs)
        {
            stListView1.SelectObjects(objs);
            stListView1.Refresh();
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

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var nodes = GetSelectedNodes();
            if (e.Button == MouseButtons.Right)
            {
                foreach (var node in nodes)
                {
                 //   if (node is IContextMenuNode)
                      //  LoadContextMenus((IContextMenuNode)node);
                }
            }
        }

     /*   private void LoadContextMenus(IContextMenuNode menuNode)
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

            OnNodeSelectionChanged?.Invoke(sender, e);
        }

        public void UpdateSelectedObjects()
        {
            stListView1.UpdateObjects(stListView1.SelectedObjects);
        }

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
        }

        private void stListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void stListView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var nodes = GetSelectedNodes();
                if (nodes.Count > 0)
                {
                    var menus = nodes[0].GetContextMenuItems();
                    stContextMenuStrip1.Items.Clear();
                    foreach (var item in menus)
                        stContextMenuStrip1.Items.Add(ContextMenuConvert(item));

                    if (stContextMenuStrip1.Items.Count > 0)
                        stContextMenuStrip1.Show(Control.MousePosition);
                }
            }
        }

        private ToolStripItem ContextMenuConvert(ToolMenuItem menu)
        {
            if (menu is ToolMenuItemSeparator)
                return new STToolStripSeparator();

            ToolStripMenuItem toolStripItem = new STToolStipMenuItem(menu.Name);

            if (menu.Click != null)
                toolStripItem.Click += menu.Click;

            foreach (var child in menu.Children)
                toolStripItem.DropDownItems.Add(ContextMenuConvert(child));
            return toolStripItem;
        }
    }
}
