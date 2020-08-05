using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.OpenGL;

namespace Toolbox.Core
{
    public class ModelContainer : IDisposable
    {
        public List<ModelEntry> Models = new List<ModelEntry>();

        public void AddModel(IModelSceneFormat resource) {
            foreach (var model in resource.ToGeneric().Models) {
                Models.Add(new ModelEntry()
                {
                    Renderer = resource.Renderer,
                    GenericModel = model,
                });
            }
        }

        public void AddModel(IModelFormat model) {
            Models.Add(new ModelEntry()
            {
                Renderer = model.Renderer,
                GenericModel = model.ToGeneric(),
            });
        }

        public void RemoveModel(IModelFormat modelFormat) {
            List<ModelEntry> removedModels = new List<ModelEntry>();
            foreach (var model in Models)
            {
                if (modelFormat.Renderer == model.Renderer)
                    removedModels.Add(model);
            }
            foreach (var model in removedModels)
                Models.Remove(model);
            removedModels.Clear();
        }

        public STSkeleton SearchActiveSkeleton()
        {
            foreach (var model in Models)
            {
                var generic = model.GenericModel;
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

    public class ModelEntry
    {
        public STGenericModel GenericModel;
        public ModelRenderer Renderer;
    }
}
