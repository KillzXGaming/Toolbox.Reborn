using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;

namespace GCNLibrary
{
    public class FileIcons : IFileIconLoader
    {
        public string Identify(string text, Stream stream)
        {
            if (text.Contains(".mdl")) return "Model";
            if (text.Contains(".bti")) return "Texture";
            if (text.Contains(".tpl")) return "TextureContainer";

            return "";
        }

        public Dictionary<string, byte[]> ImageList =>  new Dictionary<string, byte[]>()
        {

        };
    }
}
