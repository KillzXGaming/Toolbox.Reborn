using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Toolbox.Core.OpenGL
{
    public class Camera
    {
        public Matrix4 ModelMatrix { get; set; }
        public Matrix4 ViewMatrix { get; set; }
        public Matrix4 ProjectionMatrix { get; set; }
    }
}
