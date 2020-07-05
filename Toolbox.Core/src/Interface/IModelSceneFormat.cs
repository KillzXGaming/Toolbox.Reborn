using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.OpenGL;

namespace Toolbox.Core
{
    public interface IModelSceneFormat
    {
        ModelRenderer Renderer { get; }
        STGenericScene ToGeneric();
    }
}