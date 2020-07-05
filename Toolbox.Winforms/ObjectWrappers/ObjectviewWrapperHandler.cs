using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ModelView;
using Toolbox.Core;
using Toolbox.Core.Animations;

namespace Toolbox.Winforms.ObjectWrappers
{
    public class ObjectViewWrapperHandler
    {
        public static List<object> GetSelectedObjects() {
            return ObjectView.SelectedObjects;
        }

        public static List<ObjectTreeNode> GetSelectedNodes() {
            return ObjectView.SelectedNodes;
        }

        public static Dictionary<Type, ObjectWrapper> Handlers = new Dictionary<Type, ObjectWrapper>()
        {
            { typeof(IFileFormat), new FileWrapper() },
            { typeof(ITextureContainer), new TextureContainerWrapper() },
            { typeof(STGenericTexture), new TextureWrapper() },
            { typeof(System.IO.Stream), new StreamWrapper() },
            { typeof(STSkeletonAnimation), new SkeletalAnimationWrapper() },
        };
    }

    public class ObjectWrapper
    {
        public object ActiveObject;

        public List<ObjectTreeNode> GetSelectedNodes<T>()
        {
            List<ObjectTreeNode> nodes = new List<ObjectTreeNode>();
            foreach (var obj in ObjectViewWrapperHandler.GetSelectedNodes())
            {
                if (obj == null)
                    continue;

                if (typeof(T).IsAssignableFrom(obj.Tag.GetType()))
                    nodes.Add(obj);
            }
            return nodes;
        }

        public List<T> GetSelectedObjects<T>()
        {
            List<T> tags = new List<T>();
            foreach (var obj in ObjectViewWrapperHandler.GetSelectedObjects())
            {
                if (obj == null)
                    continue;

                if (typeof(T).IsAssignableFrom(obj.GetType()))
                    tags.Add((T)obj);
            }
            return tags;
        }

        public virtual ToolMenuItem[] GetContextMenuItems()
        {
            return new ToolMenuItem[0];
        }
    }
}
