using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Toolbox.Core
{
    public class OpenGLHelper
    {
        public static readonly Dictionary<STTextureMinFilter, TextureMinFilter> MinFilter = new Dictionary<STTextureMinFilter, TextureMinFilter>()
        {
            {  STTextureMinFilter.LinearMipMapNearest, TextureMinFilter.LinearMipmapLinear},
            {  STTextureMinFilter.Nearest, TextureMinFilter.Nearest},
            {  STTextureMinFilter.Linear, TextureMinFilter.Linear},
            {  STTextureMinFilter.NearestMipmapLinear, TextureMinFilter.NearestMipmapLinear},
            {  STTextureMinFilter.NearestMipmapNearest, TextureMinFilter.NearestMipmapNearest},
        };
        public static readonly Dictionary<STTextureMagFilter, TextureMagFilter> MagFilter = new Dictionary<STTextureMagFilter, TextureMagFilter>()
        {
            { STTextureMagFilter.Linear, TextureMagFilter.Linear},
            { STTextureMagFilter.Nearest, TextureMagFilter.Nearest},
            { (STTextureMagFilter)3, TextureMagFilter.Linear},
        };

        public static Dictionary<STTextureWrapMode, TextureWrapMode> WrapMode = new Dictionary<STTextureWrapMode, TextureWrapMode>(){
            { STTextureWrapMode.Repeat, TextureWrapMode.Repeat},
            { STTextureWrapMode.Mirror, TextureWrapMode.MirroredRepeat},
            { STTextureWrapMode.Clamp, TextureWrapMode.ClampToEdge},
            { (STTextureWrapMode)3, TextureWrapMode.ClampToEdge},
            { (STTextureWrapMode)4, TextureWrapMode.ClampToEdge},
            { (STTextureWrapMode)5, TextureWrapMode.ClampToEdge},
        };
    }
}
