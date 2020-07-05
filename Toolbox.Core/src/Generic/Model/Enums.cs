using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Determines the face type to draw for a given primitive.
    /// </summary>
    public enum STPrimitiveType
    {
        Triangles,
        TriangleStrips,
        TriangleFans,
        Quad,
        QuadStrips,
        Points,
        Lines,
        LineLoop,
    }

    public enum STPolygonGroupType
    {
        /// <summary>
        /// Displays all the polygon groups.
        /// </summary>
        Default,
        /// <summary>
        /// Handles a polygon group as a level of detail.
        /// Only one polygon group will display at a time (given all are using this type).
        /// </summary>
        LevelOfDetail,
    }

    /// <summary>
    /// The type of texture to display in a texture map.
    /// </summary>
    public enum STTextureType
    {
        None,
        Diffuse,
        Normal,
        Emission,
        Specular,
    }

    /// <summary>
    /// The mode to wrap a texture when a coordinate reaches the border.
    /// </summary>
    public enum STTextureWrapMode
    {
        Repeat,
        Mirror,
        Clamp,
    }

    /// <summary>
    /// Determines how to display pixels on screen from further away.
    /// </summary>
    public enum STTextureMinFilter
    {
        LinearMipMapNearest,

        /// <summary>
        /// No scale is applied to the texture resulting in a sharp, pixelated image.
        /// </summary>
        Nearest,
        /// <summary>
        /// Image is interpolated bilinear resulting in a blurred, smooth image.
        /// </summary>
        Linear,

        NearestMipmapLinear,
        NearestMipmapNearest,
    }

    /// <summary>
    /// Determines how to display pixels on screen from up close.
    /// </summary>
    public enum STTextureMagFilter
    {
        /// <summary>
        /// No scale is applied to the texture resulting in a sharp, pixelated image.
        /// </summary>
        Nearest,
        /// <summary>
        /// Image is interpolated bilinear resulting in a blurred, smooth image.
        /// </summary>
        Linear,
    }
}
