using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.ModelView
{
    public class ObjectTreeNode
    {
        public int ChildCount { get { return Children.Count; } }

        public List<ObjectTreeNode> Children { get; set; }

        public virtual string Label { get; set; }
        public virtual string ImageKey { get; set; }
        public virtual bool Checked { get; set; } = true;

        public ObjectTreeNode Parent { get; set; }

        public object Tag { get; set; }

        public bool Enable { get; set; } = true;

        public string FullPath
        {
            get
            {
                if (Parent != null)
                    return $"{Parent.FullPath}/{Label}";
                else
                    return Label;
            }
        }

        public string Type { get; set; } = "";
        public string Size { get; set; } = "";

        public void AddChild(ObjectTreeNode node)
        {
            node.Parent = this;
            Children.Add(node);
        }

        public ObjectTreeNode()
        {
            Children = new List<ObjectTreeNode>();
            ImageKey = "Folder";
        }

        public ObjectTreeNode(string text)
        {
            Children = new List<ObjectTreeNode>();
            Label = text;
            ImageKey = "Folder";
        }

        public virtual void OnClick()
        {

        }

        public virtual void OnDoubleClick()
        {

        }

        public virtual void OnBeforeExpand()
        {
        }

        public virtual void OnAfterCollapse()
        {

        }

        public string LoadFileDialog(string name = "")
        {
            GUI.SaveFileDialog sfd = new GUI.SaveFileDialog();
            if (sfd.ShowDialog() == GUI.SaveFileDialog.Result.OK) {
                return sfd.FileName;
            }
            return "";
        }

        public virtual ToolMenuItem[] GetContextMenuItems()
        {
            return new ToolMenuItem[0];
        }
    }
}
