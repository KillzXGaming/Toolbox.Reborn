using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Winforms
{
    public class FileImageKeys
    {
        public static Dictionary<string, string> Lookup = new Dictionary<string, string>()
        {
            { ".h", "HeaderFile"},
            { ".txt", "TextFile"},
            { ".cfg", "TextFile"},
            { ".xml", "TextFile"},
            { ".json", "TextFile"},
            { ".fruit", "FruitFile"},
            { ".dds", "Texture"},
            { ".jpg", "Texture"},
            { ".png", "Texture"},
            { ".tga", "Texture"},
            {".mshader", "ShaderFile"},
            {".glsl", "ShaderFile"},
            {".frag", "ShaderFile"},
        };
    }
}
