using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    public class ModelContainer : IModelContainer, IDisposable
    {
        List<IModelFormat> Models = new List<IModelFormat>();

        public IEnumerable<IModelFormat> ModelList => Models;

        public void AddModel(IModelFormat model) {
            Models.Add(model);
        }

        public void RemoveModel(IModelFormat model) {
            if (Models.Contains(model))
                Models.Remove(model);
        }

        public STSkeleton SearchActiveSkeleton()
        {
            foreach (var model in Models)
            {
                var generic = model.ToGeneric();
                if (generic.Skeleton.Visible)
                    return generic.Skeleton;
            }
            return null;
        }

        public void Dispose()
        {
            foreach (var model in Models)
            {
                if (model.Renderer != null)
                    model.Renderer.Dispose();
                if (model is IDisposable)
                    ((IDisposable)model).Dispose();
            }
        }
    }
}
