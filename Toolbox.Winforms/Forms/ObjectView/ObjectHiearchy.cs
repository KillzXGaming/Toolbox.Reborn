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
using Toolbox.Core.ModelView;

namespace Toolbox.Winforms
{
    public partial class ObjectHiearchy : UserControl
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
                stListView1.EnsureModelVisible(value);
                stListView1.SelectObject(value);
                stListView1.Refresh();
            }
        }

        public List<ObjectTreeNode> Children { get; set; } = new List<ObjectTreeNode>();
        public List<ObjectTreeNode> CachedChildren { get; set; } = new List<ObjectTreeNode>();

        public static ImageList ImageList;

        public void BeginUpdate()
        {
            stListView1.BeginUpdate();
        }

        public void EndUpdate()
        {
            stListView1.EndUpdate();
        }

        public void UpdateCache(ObjectTreeNode cache)
        {
            CachedChildren.Clear();
            CachedChildren.Add(cache);
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

            this.stListView1.BooleanCheckStateGetter = delegate (Object row) {
                return ((ObjectTreeNode)row).Checked;
            };

            this.stListView1.BooleanCheckStatePutter = delegate (Object row, bool newValue) {
                ((ObjectTreeNode)row).Checked = newValue;
                return newValue; // return the value that you want the control to use
            };

            this.Label.ImageGetter = delegate (object row) {
                ObjectTreeNode node = (ObjectTreeNode)row;

                String key = node.ImageKey;
                if (ImageList.Images.ContainsKey(key) && !this.stListView1.LargeImageList.Images.ContainsKey(key))
                {
                    Image smallImage = ImageList.Images[key];
                    Image largeImage = ImageList.Images[key];
                    this.stListView1.SmallImageList.Images.Add(key, smallImage);
                    this.stListView1.LargeImageList.Images.Add(key, largeImage);
                }
                return key;
            };
        }

        public ObjectHiearchy(ObjectView objectView)
        {
            InitializeComponent();

            ParentObjectView = objectView;

            stListView1.BackColor = FormThemes.BaseTheme.FormBackColor;
            stListView1.ForeColor = FormThemes.BaseTheme.FormForeColor;
            stListView1.HeaderStyle = ColumnHeaderStyle.None;
            stListView1.FullRowSelect = false;
            stListView1.MultiSelect = true;
            stListView1.TreeColumnRenderer.LinePen = new Pen(Color.FromArgb(80, 80, 80));
            stListView1.HideSelection = false;
            stListView1.UseCustomSelectionColors = true;
            stListView1.UnfocusedSelectedForeColor = stListView1.SelectedForeColor;
            stListView1.UnfocusedSelectedBackColor = stListView1.SelectedBackColor;
            stListView1.MouseClick += listView1_MouseClick;
            stListView1.HideSelection = false;

            // Configure the tree
            stListView1.CanExpandGetter = delegate (object x) { return ((ObjectTreeNode)x).ChildCount > 0; };
            stListView1.ChildrenGetter = delegate (object x) { return ((ObjectTreeNode)x).Children; };

            stListView1.Roots = Children;
        }

        public void Expand(ObjectTreeNode node) {
            stListView1.Expand(node);
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
            stListView1.Sort(0);
            stListView1.Refresh();
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
            SelectByTags(nodes, Children, tags);

            stListView1.SelectObjects(nodes);
            stListView1.Refresh();
        }

        public void SelectByTags(List<ObjectTreeNode> nodes, List<ObjectTreeNode> children, IEnumerable<object> tags)
        {
            foreach (ObjectTreeNode node in children)
            {
                if (tags.Contains(node.Tag))
                {
                    nodes.Add(node);
                    stListView1.EnsureModelVisible(node);
                }
                SelectByTags(nodes, node.Children, tags);
            }
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
                foreach (var child in FetchAllChildren(CachedChildren))
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

        private List<ObjectTreeNode> FetchAllChildren(List<ObjectTreeNode> nodes)
        {
            List<ObjectTreeNode> children = new List<ObjectTreeNode>();
            children.AddRange(nodes);
            foreach (var node in nodes)
            {
                if (node is ArchiveHiearchy)
                    children.AddRange(FetchAllChildren(((ArchiveHiearchy)node).GetChildren()));
                else
                    children.AddRange(FetchAllChildren(node.Children));
            }

            return children;
        }

        private void stListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnNodeSelectionChanged?.Invoke(this, e);
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
            if (stListView1.SelectedObjects.Count > 0)
            {
                foreach (var obj in stListView1.SelectedObjects)
                {
                    if (!stListView1.IsExpanded(obj))
                        stListView1.Expand(obj);
                    else
                        stListView1.Collapse(obj);
                }
            }

            OnMouseDoubleClick?.Invoke(sender, e);
        }

        private void stListView1_Expanding(object sender, TreeBranchExpandingEventArgs e)
        {
            if (e.Model is ObjectTreeNode)
                ((ObjectTreeNode)e.Model).OnBeforeExpand();
        }

        private void stListView1_Collapsing(object sender, TreeBranchCollapsingEventArgs e)
        {
            if (e.Model is ObjectTreeNode)
                ((ObjectTreeNode)e.Model).OnAfterCollapse();
        }
    }
}
