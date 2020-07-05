using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.OpenGL;

namespace Toolbox.Core
{
    public interface IModelFormat
    {
        ModelRenderer Renderer { get; }
        STGenericModel ToGeneric();
    }
}