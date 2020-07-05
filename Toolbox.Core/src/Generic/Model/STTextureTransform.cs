using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Toolbox.Core
{
    /// <summary>
    /// Represents a SRT transformation for texture coordinates
    /// from a <see cref="STGenericTextureMap"/>.
    /// </summary>
    public class STTextureTransform
    {
        /// <summary>
        /// The scale of the UV coordinates.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// The rotation in radians of the UV coordinates.
        /// </summary>
        public float Rotate { get; set; }

        /// <summary>
        /// The translation of the UV coordinates.
        /// </summary>
        public Vector2 Translate { get; set; }
    }
}
