using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Toolbox.Core;

namespace STLibrary.Forms.Rendering
{
    public class OpenTKHelper
    {
        public static void CreateContext()
        {
            // Make a permanent context to share resources.
            GraphicsContext.ShareContexts = true;
            var control = new OpenTK.GLControl();
            control.MakeCurrent();

            RenderTools.LoadTextures();
            Runtime.OpenTKInitialized = true;
        }
    }
}
