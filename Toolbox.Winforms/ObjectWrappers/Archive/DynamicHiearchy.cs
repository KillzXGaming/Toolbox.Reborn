using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ModelView;

namespace Toolbox.Winforms
{
    public class DynamicHiearchy : ObjectTreeNode
    {
        private ObjectTreeNode ReferenceNode;

        public DynamicHiearchy(ObjectTreeNode node)
        {
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

        private bool expanded = false;
        public override void OnBeforeExpand()
        {
            if (expanded)
                return;

            Children.Clear();
            foreach (var node in ReferenceNode.Children)
                Children.Add(new DynamicHiearchy(node));

            expanded = true;
        }

        public override void OnAfterCollapse()
        {
        
        }
    }
}
